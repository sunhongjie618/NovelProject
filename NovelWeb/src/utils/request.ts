import axios from 'axios'
import type { AxiosRequestConfig } from 'axios'
import { ElMessage } from 'element-plus'
import type { ApiResponse } from '@/types/api'

export const TOKEN_KEY = 'novel_token'
export const USER_KEY = 'novel_user'

/** 所有请求统一走 /api，由 vite proxy 转发到后端 https://localhost:7112 */
const instance = axios.create({
  baseURL: '/api',
  timeout: 15000,
})

// ---------- 请求拦截：自动带 token ----------
instance.interceptors.request.use((config) => {
  const token = localStorage.getItem(TOKEN_KEY)
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

function redirectToLogin() {
  localStorage.removeItem(TOKEN_KEY)
  localStorage.removeItem(USER_KEY)
  const { pathname, search } = window.location
  if (!pathname.startsWith('/login')) {
    window.location.href = `/?login=1&redirect=${encodeURIComponent(pathname + search)}`
  }
}

/**
 * 统一请求方法，已经帮你把 ApiResponse 信封拆掉了
 * 用法：const novel = await request<NovelDetail>({ url: '/Novels/1' })
 */
export async function request<T = unknown>(config: AxiosRequestConfig): Promise<T> {
  try {
    const res = await instance.request<ApiResponse<T>>(config)
    const body = res.data

    // 后端所有接口都返回 { code, message, data }
    if (body && typeof body === 'object' && 'code' in body) {
      // 注意：不能按 HTTP 状态判断成败，后端大量错误也是 400 + code:400
      if (body.code === 200) {
        return body.data
      }
      if (body.code === 401) {
        redirectToLogin()
        return Promise.reject(new Error('登录已过期'))
      }
      const msg = body.message || '请求失败'
      ElMessage.error(msg)
      return Promise.reject(new Error(msg))
    }

    // 极少数接口返回裸数据，原样返回
    return body as unknown as T
  } catch (error) {
    // 上面自己抛出的业务错误，已经提示过了，直接往外抛
    if (!axios.isAxiosError(error)) {
      return Promise.reject(error)
    }

    const status = error.response?.status

    // 后端没配 OnChallenge，401 的 body 是空的，读不到 message
    if (status === 401) {
      redirectToLogin()
      return Promise.reject(new Error('请先登录'))
    }

    if (error.code === 'ECONNABORTED') {
      ElMessage.error('请求超时，请检查后端是否启动')
      return Promise.reject(new Error('请求超时'))
    }

    const data = error.response?.data as Partial<ApiResponse> | undefined
    const msg = data?.message || `请求失败（${status ?? '网络异常'}）`
    ElMessage.error(msg)
    return Promise.reject(new Error(msg))
  }
}

/** 便捷方法 */
export const http = {
  get: <T>(url: string, params?: object) =>
    request<T>({ url, method: 'get', params }),

  post: <T>(url: string, data?: unknown) =>
    request<T>({ url, method: 'post', data }),

  put: <T>(url: string, data?: unknown) =>
    request<T>({ url, method: 'put', data }),

  delete: <T>(url: string, params?: object) =>
    request<T>({ url, method: 'delete', params }),

  /** 上传文件（multipart/form-data） */
  upload: <T>(url: string, file: File, fieldName = 'file') => {
    const form = new FormData()
    form.append(fieldName, file)
    return request<T>({ url, method: 'post', data: form })
  },
}

export { instance as rawAxios }
