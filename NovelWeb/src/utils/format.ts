/** 后端 FilesController 返回的是 /pictures/xxx，而静态目录实际是 /Picture */
export function resolveImageUrl(url?: string | null): string {
  if (!url) return ''
  let result = url.trim()
  if (!result) return ''
  result = result.replace(/\/pictures\//gi, '/Picture/')
  return result
}

export function formatWordCount(count?: number | null): string {
  const value = count || 0
  if (value >= 10000) return `${(value / 10000).toFixed(1)} 万字`
  return `${value} 字`
}

export function formatNumber(count?: number | null): string {
  const value = count || 0
  if (value >= 10000) return `${(value / 10000).toFixed(1)}w`
  return String(value)
}

/** 1 连载 / 2 完结 / 3 下架 */
export function statusText(status?: number | null): string {
  switch (status) {
    case 1:
      return '连载中'
    case 2:
      return '已完结'
    case 3:
      return '已下架'
    default:
      return '未知'
  }
}

/** 相对时间：刚刚 / x 分钟前 / x 小时前 / x 天前 / yyyy-MM-dd */
export function fromNow(input?: string | null): string {
  if (!input) return ''
  const date = new Date(input)
  if (Number.isNaN(date.getTime())) return ''
  const diff = Date.now() - date.getTime()
  const minute = 60 * 1000
  const hour = 60 * minute
  const day = 24 * hour

  if (diff < minute) return '刚刚'
  if (diff < hour) return `${Math.floor(diff / minute)} 分钟前`
  if (diff < day) return `${Math.floor(diff / hour)} 小时前`
  if (diff < 30 * day) return `${Math.floor(diff / day)} 天前`
  return formatDate(input)
}

export function formatDate(input?: string | null): string {
  if (!input) return ''
  const date = new Date(input)
  if (Number.isNaN(date.getTime())) return ''
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`
}

export function formatDateTime(input?: string | null): string {
  if (!input) return ''
  const date = new Date(input)
  if (Number.isNaN(date.getTime())) return ''
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${formatDate(input)} ${pad(date.getHours())}:${pad(date.getMinutes())}`
}
