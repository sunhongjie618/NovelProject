import {
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
  type HubConnection,
} from '@microsoft/signalr'
import { reactive } from 'vue'
import { TOKEN_KEY } from '@/utils/request'

/** 全站广播（后端 ChatRoomHub.SendSystemNotification 发出） */
export interface SystemNotice {
  title: string
  content: string
  time: string
}

/** 公屏消息（站务公告）：后端载荷是 "发送者 时间: 内容" 的字符串 */
export interface PublicMessage {
  text: string
  /** 本地收到的时刻（ISO） */
  time: string
}

/** 私聊消息 */
export interface PrivateMessage {
  fromUserId: number
  fromUserName: string
  message: string
  messageId?: number
  time: string
}

/** 离线未读（后端 OnConnectedAsync 补发，注意：不带 messageId） */
export interface UnreadMessage {
  fromUserId: number
  fromUserName: string
  content: string
  createTime: string
}

/** 全站广播本地缓存的 key 与上限 */
const NOTICE_STORE_KEY = 'SIGNALR_NOTICES'
const NOTICE_LIMIT = 50

function loadStoredNotices(): SystemNotice[] {
  try {
    const raw = localStorage.getItem(NOTICE_STORE_KEY)
    if (!raw) return []
    const parsed = JSON.parse(raw)
    return Array.isArray(parsed) ? (parsed as SystemNotice[]).slice(0, NOTICE_LIMIT) : []
  } catch {
    return []
  }
}

const PUBLIC_STORE_KEY = 'PUBLIC_MESSAGES'
const PUBLIC_LIMIT = 100

function loadStoredPublic(): PublicMessage[] {
  try {
    const raw = localStorage.getItem(PUBLIC_STORE_KEY)
    if (!raw) return []
    const parsed = JSON.parse(raw)
    return Array.isArray(parsed) ? (parsed as PublicMessage[]).slice(0, PUBLIC_LIMIT) : []
  } catch {
    return []
  }
}

function savePublicMessages(list: PublicMessage[]) {
  try {
    localStorage.setItem(PUBLIC_STORE_KEY, JSON.stringify(list))
  } catch {
    /* 配额满或被禁用时忽略 */
  }
}

function saveNotices(list: SystemNotice[]) {
  try {
    localStorage.setItem(NOTICE_STORE_KEY, JSON.stringify(list))
  } catch {
    /* 配额满或被禁用时忽略 */
  }
}

type Handler = (...args: never[]) => void

/**
 * SignalR 单例。
 * - 后端 Hub 路径：/Hubs/ChatRoomHub（Program.cs:169）
 * - 认证方式：JWT 走 query 参数 access_token（Program.cs 的 JwtBearerEvents）
 */
class SignalRService {
  private connection: HubConnection | null = null
  private handlers = new Map<string, Set<Handler>>()
  /** 首次 connect 失败时的重试定时器（withAutomaticReconnect 只在「连上过」之后才生效） */
  private retryTimer: number | undefined
  private retryIndex = 0
  private manuallyStopped = false
  private static readonly RETRY_DELAYS = [1000, 3000, 8000, 15000, 30000]

  /** 供 UI 响应的连接状态 */
  readonly state = reactive({
    connected: false,
    connecting: false,
    lastError: '' as string,
  })

  /** 最近收到的全站广播（最新在前，最多保留 50 条，并写入 localStorage 以便刷新后仍可查看） */
  readonly notices = reactive<SystemNotice[]>(loadStoredNotices())

  /** 公屏消息 / 站务公告（最新在前，最多 100 条，同样落 localStorage） */
  readonly publicMessages = reactive<PublicMessage[]>(loadStoredPublic())

  private emit(event: string, ...args: unknown[]) {
    const set = this.handlers.get(event)
    if (!set) return
    set.forEach((fn) => (fn as unknown as (...a: unknown[]) => void)(...args))
  }

  on<T extends unknown[]>(event: string, handler: (...args: T) => void) {
    if (!this.handlers.has(event)) this.handlers.set(event, new Set())
    this.handlers.get(event)!.add(handler as unknown as Handler)
    return () => {
      this.handlers.get(event)?.delete(handler as unknown as Handler)
    }
  }

  async connect() {
    this.manuallyStopped = false
    const token = localStorage.getItem(TOKEN_KEY)
    if (!token) {
      this.state.lastError = '未登录'
      return
    }
    if (
      this.connection &&
      (this.connection.state === HubConnectionState.Connected ||
        this.connection.state === HubConnectionState.Connecting ||
        this.connection.state === HubConnectionState.Reconnecting)
    ) {
      return
    }

    this.state.connecting = true
    this.state.lastError = ''

    const connection = new HubConnectionBuilder()
      .withUrl('/Hubs/ChatRoomHub', {
        accessTokenFactory: () => localStorage.getItem(TOKEN_KEY) || '',
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000])
      .configureLogging(LogLevel.Warning)
      .build()

    connection.on('ReceiveSystemMessage', (notice: SystemNotice) => {
      this.notices.unshift(notice)
      if (this.notices.length > NOTICE_LIMIT) this.notices.pop()
      saveNotices(this.notices)
      this.emit('ReceiveSystemMessage', notice)
    })

    connection.on('ReceivePrivateMessage', (msg: PrivateMessage) => {
      this.emit('ReceivePrivateMessage', msg)
    })

    connection.on('ReceiveUnreadMessage', (list: UnreadMessage[]) => {
      this.emit('ReceiveUnreadMessage', list)
    })

    connection.on('ReceivePublicMessage', (msg: string) => {
      if (typeof msg === 'string' && msg.trim()) {
        this.publicMessages.unshift({ text: msg, time: new Date().toISOString() })
        if (this.publicMessages.length > PUBLIC_LIMIT) this.publicMessages.pop()
        savePublicMessages(this.publicMessages)
      }
      this.emit('ReceivePublicMessage', msg)
    })

    connection.on('ReceiveChapterUpdateMessage', (msg: unknown) => {
      this.emit('ReceiveChapterUpdateMessage', msg)
    })

    connection.on('SendPrivateMessageSuccess', (payload: unknown) => {
      this.emit('SendPrivateMessageSuccess', payload)
    })

    // 后端四个不同事件名的失败提示，统一收口
    const failEvents = [
      'SendPrivateMessageFailed',
      'SendPrivateEmptyMessage',
      'SendMessageFailed',
      'SendMessageError',
    ]
    failEvents.forEach((name) => {
      connection.on(name, (msg: string) => this.emit('SendFailed', msg))
    })

    connection.onreconnecting(() => {
      this.state.connected = false
      this.state.connecting = true
    })

    connection.onreconnected(() => {
      this.state.connected = true
      this.state.connecting = false
    })

    connection.onclose((err) => {
      this.state.connected = false
      this.state.connecting = false
      if (err) this.state.lastError = err.message
    })

    this.connection = connection

    try {
      await connection.start()
      this.state.connected = true
      this.retryIndex = 0
    } catch (error) {
      this.state.connected = false
      this.state.lastError =
        error instanceof Error ? error.message : String(error)
      // 常见原因：后端没启动（F5 重启中）、token 过期、Hub 的 OnConnectedAsync 抛异常
      // eslint-disable-next-line no-console
      console.warn('[SignalR] 连接失败：', this.state.lastError)
      try {
        await connection.stop()
      } catch {
        /* 忽略 */
      }
      this.connection = null
      this.scheduleRetry()
    } finally {
      this.state.connecting = false
    }
  }

  /** 首次握手失败后的指数退避重试（连上过之后的断线由 withAutomaticReconnect 负责） */
  private scheduleRetry() {
    if (this.manuallyStopped) return
    if (!localStorage.getItem(TOKEN_KEY)) return
    const delays = SignalRService.RETRY_DELAYS
    const delay = delays[Math.min(this.retryIndex, delays.length - 1)]
    this.retryIndex += 1
    window.clearTimeout(this.retryTimer)
    this.retryTimer = window.setTimeout(() => {
      void this.connect()
    }, delay)
  }

  /** 手动重试：公告页「重试连接」按钮、窗口重新可见时调用 */
  retry() {
    this.retryIndex = 0
    this.state.lastError = ''
    return this.connect()
  }

  async disconnect() {
    this.manuallyStopped = true
    window.clearTimeout(this.retryTimer)
    if (this.connection) {
      try {
        await this.connection.stop()
      } catch {
        // 忽略
      }
      this.connection = null
    }
    this.state.connected = false
    this.state.connecting = false
  }

  /** 发送私聊消息；返回是否有错误（有错误时抛异常） */
  async sendPrivateMessage(toUserId: number, message: string) {
    if (!this.connection || this.connection.state !== HubConnectionState.Connected) {
      throw new Error('SignalR 未连接，请刷新页面重试')
    }
    await this.connection.invoke('SendPrivateMessage', toUserId, message)
  }

  /** 发送公屏消息 */
  async sendPublicMessage(message: string) {
    if (!this.connection || this.connection.state !== HubConnectionState.Connected) {
      throw new Error('SignalR 未连接')
    }
    await this.connection.invoke('SendPublicMessage', message)
  }

  /** 标记消息已读（注意：离线补发的未读没有 messageId，无法标记） */

  async markAsRead(messageId: number) {
    if (!this.connection || this.connection.state !== HubConnectionState.Connected) {
      return
    }
    await this.connection.invoke('MarkMessageAsRead', messageId)
  }
}

export const signalR = new SignalRService()
