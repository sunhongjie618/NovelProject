// ============================================================
// 与后端 DTO 一一对应的类型定义
// 注意：后端 System.Text.Json 默认不改名，所以字段名就是 C# 属性名（大驼峰）
// 唯一例外是分页匿名对象，字段是全小写：total / data / page / pagesize
// ============================================================

/** 统一响应信封：所有接口都套这一层 */
export interface ApiResponse<T = unknown> {
  code: number
  message: string
  data: T
}

/** 分页结果（小说列表是 pagesize，其他模块可能是 pageSize，都声明成可选 */
export interface PagedData<T> {
  total: number
  data: T[]
  page?: number
  pagesize?: number
  pageSize?: number
}

// ---------------- 认证 ----------------

export interface LoginRequest {
  userName: string
  password: string
}

export interface RegisterRequest {
  userName: string
  password: string
  nickName: string
}

export interface LoginResponse {
  token: string
  nickName: string
  userId: number
}

// ---------------- 分类 ----------------

export interface CategoryItem {
  categoryId: number
  categoryName: string
  sortOrder: number
  description: string
  isEnable: boolean
}

// ---------------- 小说 ----------------

/** 列表项 */
export interface NovelListItem {
  novelId: number
  title: string
  authorName: string
  coverUrl: string
  description: string
  status: number // 1 连载 2 完结 3 下架
  wordCount: number
  categoryName: string
  lastUpdateTime: string
  clickCount: number
  favoriteCount: number
}

/** 详情，比列表多四个字段 */
export interface NovelDetail extends NovelListItem {
  categoryId: number
  publishTime: string
  chapterCount: number
}

export interface NovelListQuery {
  categoryId?: number
  keyword?: string
  page?: number
  pagesize?: number
  sortBy?: boolean
  isCompleted?: boolean
  sortType?: string
}

export interface AddNovelRequest {
  categoryId: number
  title: string
  coverUrl: string
  description: string
}

export interface UpdateNovelRequest {
  categoryId?: number
  title?: string
  coverUrl?: string
  description?: string
  status?: number
}

// ---------------- 章节 ----------------

export interface ChapterListItem {
  chapterId: number
  chapterNumber: number
  title: string
  wordCount: number
  publishTime: string
  lastUpdateTime: string
  isLocked: boolean
}

export interface ChapterDetail extends ChapterListItem {
  novelId: number
  content: string
  preChapterId: number | null
  nextChapterId: number | null
}

export interface AddChapterRequest {
  novelId: number
  title: string
  content: string
  isLocked: boolean
  chapterNumber: number
}

// ---------------- 评论 ----------------

export interface CommentItem {
  commentId: number
  userId: number | null
  userNickName: string
  userAvatarUrl: string
  chapterId: number | null
  content: string
  createTime: string
  likeCount: number
  isLiked: boolean
}

export interface AddCommentRequest {
  novelId: number
  chapterId?: number | null
  content: string
}

export interface ToggleLikeResult {
  likeCount: number
  isLike: boolean
}

// ---------------- 收藏 ----------------

export interface FavoriteItem {
  favoriteId: number
  userId: number
  novelId: number
  novelName: string
  coverUrl: string
  author: string
  description: string
  favoriteCount: number
  createTime: string
}

export interface ToggleFavoriteResult {
  isFavorited: boolean
  favoriteCount: number
}

// ---------------- 阅读记录 ----------------

export interface ReadingRecordItem {
  recordId: number
  novelId: number
  lastReadChapterId: number | null
  lastReadTime: string
  title: string
  authorName: string
  coverUrl: string
}

export interface UpdateReadingRecordRequest {
  novelId: number
  chapterId: number
}

// ---------------- 文件上传 ----------------

export interface FileUploadResult {
  fileUrl: string
  fileName: string
}

// ---------------- 常量 ----------------

export const NOVEL_STATUS = {
  SERIALIZING: 1,
  FINISHED: 2,
  OFFLINE: 3,
} as const

export const NOVEL_STATUS_TEXT: Record<number, string> = {
  1: '连载中',
  2: '已完结',
  3: '已下架',
}
