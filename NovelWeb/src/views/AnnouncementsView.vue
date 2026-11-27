<script setup lang="ts">
import { computed, ref } from 'vue'
import { Promotion, Search } from '@element-plus/icons-vue'
import { formatDateTime } from '@/utils/format'
import { signalR } from '@/utils/signalr'

/**
 * 站务公告页。
 * 数据来源：公屏消息事件 `ReceivePublicMessage`（后端 NotificationController 的 notice 端点推送），
 * 载荷是字符串 `"发送者 yyyy-MM-dd HH:mm:ss: 内容"`。本地缓存最近 100 条。
 * 后续「小说更新」这类推送也走这个列表，等后端控制器写好再接。
 */
const keyword = ref('')
const expanded = ref<number[]>([])

interface Row {
  body: string
  from: string
  receivedAt: string
}

/** 把 "发送者 时间: 内容" 拆成 来源 / 正文 */
function parse(text: string): Row {
  const i = text.lastIndexOf(': ')
  const head = i > 0 ? text.slice(0, i) : ''
  const body = i > 0 ? text.slice(i + 2) : text
  // head 形如 "系统 2026-11-27 15:00:00"，只保留发送者
  const from = head.replace(/\d{4}-\d{1,2}-\d{1,2}.*$/, '').trim() || '系统'
  return { body: body.trim(), from, receivedAt: '' }
}

const list = computed<Row[]>(() => {
  const rows = signalR.publicMessages.map((item) => ({
    ...parse(item.text),
    receivedAt: formatDateTime(item.time),
  }))
  const kw = keyword.value.trim().toLowerCase()
  if (!kw) return rows
  return rows.filter(
    (r) => r.body.toLowerCase().includes(kw) || r.from.toLowerCase().includes(kw),
  )
})

function isOpen(index: number) {
  return expanded.value.includes(index)
}

function toggle(index: number) {
  const i = expanded.value.indexOf(index)
  if (i >= 0) expanded.value.splice(i, 1)
  else expanded.value.push(index)
}
</script>

<template>
  <div class="page-padding">
    <div class="ann-head">
      <div>
        <h2 class="ann-head__title">站务公告</h2>
        <p class="ann-head__sub">
          接收 <code>ReceivePublicMessage</code> 推送的站内公告，本地保留最近 100 条（首页顶部滚动条显示的是<code>ReceiveSystemMessage</code> 系统广播）；小说更新类推送等后端接口就绪后接入
        </p>
      </div>

      <div class="ann-head__tools">
        <el-input v-model="keyword" placeholder="搜索公告正文" clearable style="width: 230px">
          <template #prefix>
            <el-icon><Search /></el-icon>
          </template>
        </el-input>
        <el-tooltip
          v-if="!signalR.state.connected"
          :content="signalR.state.lastError || '正在连接…'"
          placement="bottom"
        >
          <el-button size="small" :loading="signalR.state.connecting" @click="signalR.retry()">
            重试连接
          </el-button>
        </el-tooltip>
        <el-tag :type="signalR.state.connected ? 'success' : 'info'" effect="plain" round>
          {{ signalR.state.connected ? '实时接收中' : '未连接' }}
        </el-tag>
      </div>
    </div>

    <el-empty
      v-if="list.length === 0"
      :description="keyword ? '没有匹配的公告' : '还没有收到任何公告'"
      :image-size="90"
    />

    <div v-else class="ann-table">
      <div class="ann-table__head">
        <span>公告</span>
        <span class="ann-table__time">发送方</span>
      </div>

      <div
        v-for="(item, index) in list"
        :key="index"
        class="ann-row"
        :class="{ 'is-open': isOpen(index) }"
        @click="toggle(index)"
      >
        <div class="ann-row__main">
          <div class="ann-row__title">
            <el-icon class="ann-row__pin"><Promotion /></el-icon>
            <span class="ann-row__body">{{ item.body }}</span>
          </div>
          <div class="ann-row__meta">收到于 {{ item.receivedAt }}</div>
        </div>
        <div class="ann-row__time">{{ item.from }}</div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.ann-head {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 20px;
  margin-bottom: 16px;
}

.ann-head__title {
  margin: 0 0 6px;
  font-size: 26px;
  font-weight: 800;
  color: #1f2d3d;
  letter-spacing: 1px;
}

.ann-head__sub {
  margin: 0;
  font-size: 12px;
  color: #8a97a5;
}

.ann-head__sub code {
  padding: 1px 5px;
  border-radius: 4px;
  background: #f2f5f8;
  color: #4c5c6b;
  font-size: 11px;
}

.ann-head__tools {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-shrink: 0;
}

.ann-table {
  background: #fff;
  border-radius: 14px;
  padding: 6px 22px 10px;
  box-shadow: 0 2px 14px rgba(23, 43, 77, 0.05);
}

.ann-table__head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 4px;
  font-size: 12px;
  color: #98a4b0;
  border-bottom: 1px solid #f0f3f6;
}

.ann-table__time {
  width: 110px;
  text-align: right;
  flex-shrink: 0;
}

.ann-row {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 20px;
  padding: 15px 4px;
  border-bottom: 1px solid #f5f7f9;
  cursor: pointer;
  transition: background 0.15s ease;
}

.ann-row:last-child {
  border-bottom: none;
}

.ann-row:hover {
  background: #fafcfd;
}

.ann-row__main {
  min-width: 0;
  flex: 1;
}

.ann-row__title {
  display: flex;
  align-items: flex-start;
  gap: 7px;
}

.ann-row__pin {
  color: #f0a020;
  font-size: 15px;
  margin-top: 3px;
  flex-shrink: 0;
}

.ann-row__body {
  font-size: 14.5px;
  line-height: 1.65;
  font-weight: 600;
  color: #24343f;
  white-space: pre-wrap;
  display: -webkit-box;
  -webkit-line-clamp: 1;
  line-clamp: 1;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.ann-row.is-open .ann-row__body {
  display: block;
  -webkit-line-clamp: unset;
  line-clamp: unset;
  overflow: visible;
  font-weight: 500;
}

.ann-row__meta {
  margin-top: 5px;
  font-size: 12px;
  color: #9aa6b2;
}

.ann-row__time {
  width: 110px;
  flex-shrink: 0;
  text-align: right;
  font-size: 12.5px;
  color: #7b8794;
}

@media (max-width: 760px) {
  .ann-head {
    flex-direction: column;
    align-items: stretch;
  }

  .ann-table__head {
    display: none;
  }

  .ann-row {
    flex-direction: column;
    gap: 6px;
  }

  .ann-row__time {
    width: auto;
    text-align: left;
  }
}
</style>
