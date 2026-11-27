import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import { authApi } from '@/api'
import { TOKEN_KEY, USER_KEY } from '@/utils/request'
import type { LoginRequest, LoginResponse, RegisterRequest } from '@/types/api'

interface UserInfo {
  userId: number
  nickName: string
}

function readUser(): UserInfo | null {
  const raw = localStorage.getItem(USER_KEY)
  if (!raw) return null
  try {
    return JSON.parse(raw) as UserInfo
  } catch {
    return null
  }
}

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string>(localStorage.getItem(TOKEN_KEY) || '')
  const user = ref<UserInfo | null>(readUser())

  const isLogin = computed(() => !!token.value)
  const nickName = computed(() => user.value?.nickName || '未登录')
  const userId = computed(() => user.value?.userId || 0)

  function setLogin(data: LoginResponse) {
    token.value = data.token
    user.value = { userId: data.userId, nickName: data.nickName }
    localStorage.setItem(TOKEN_KEY, data.token)
    localStorage.setItem(USER_KEY, JSON.stringify(user.value))
  }

  function clear() {
    token.value = ''
    user.value = null
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(USER_KEY)
  }

  async function login(payload: LoginRequest) {
    const data = await authApi.login(payload)
    setLogin(data)
    return data
  }

  async function register(payload: RegisterRequest) {
    return authApi.register(payload)
  }

  return { token, user, isLogin, nickName, userId, login, register, setLogin, clear }
})
