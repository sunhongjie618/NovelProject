import { http } from '@/utils/request'
import type {
  AddChapterRequest,
  AddCommentRequest,
  AddNovelRequest,
  CategoryItem,
  ChapterDetail,
  ChapterListItem,
  CommentItem,
  FavoriteItem,
  FileUploadResult,
  LoginRequest,
  LoginResponse,
  NovelDetail,
  NovelListItem,
  NovelListQuery,
  PagedData,
  ReadingRecordItem,
  RegisterRequest,
  ToggleFavoriteResult,
  ToggleLikeResult,
  UpdateNovelRequest,
} from '@/types/api'

// ============================================================
// 后端 24 个业务端点的封装，路径与 Controller 一一对应
// baseURL 已经是 /api，所以这里不再写 /api 前缀
// ============================================================

export const authApi = {
  /** POST /api/Auth/register */
  register: (data: RegisterRequest) => http.post<null>('/Auth/register', data),
  /** POST /api/Auth/login */
  login: (data: LoginRequest) => http.post<LoginResponse>('/Auth/login', data),
}

export const categoryApi = {
  /** GET /api/Categories?isEnable=&orderBySortAsc= （匿名可访问） */
  list: (params?: { isEnable?: boolean; orderBySortAsc?: boolean }) =>
    http.get<CategoryItem[]>('/Categories', params),
  /** GET /api/Categories/{categoryId} */
  detail: (categoryId: number) => http.get<CategoryItem>(`/Categories/${categoryId}`),
  /** POST /api/Categories （需登录） */
  create: (data: {
    categoryName: string
    sortOrder: number
    description: string
    isEnable: boolean
  }) => http.post<number>('/Categories', data),
  /** DELETE /api/Categories/{categoryId} */
  remove: (categoryId: number) => http.delete<null>(`/Categories/${categoryId}`),
}

export const novelApi = {
  /** GET /api/Novels?categoryId&keyword&page&pagesize&sortBy&isCompleted&sortType */
  list: (query: NovelListQuery) => http.get<PagedData<NovelListItem>>('/Novels', query),
  /** GET /api/Novels/{novelId} */
  detail: (novelId: number) => http.get<NovelDetail>(`/Novels/${novelId}`),
  /** POST /api/Novels */
  create: (data: AddNovelRequest) => http.post<number>('/Novels', data),
  /** PUT /api/Novels/{novelId} */
  update: (novelId: number, data: UpdateNovelRequest) =>
    http.put<null>(`/Novels/${novelId}`, data),
  /** GET /api/Novels/mine?page&pagesize  只返回当前登录用户的作品（服务端分页） */
  mine: (query?: { page?: number; pagesize?: number }) =>
    http.get<PagedData<NovelListItem>>('/Novels/mine', query),
  /** DELETE /api/Novels/{novelId} */
  remove: (novelId: number) => http.delete<null>(`/Novels/${novelId}`),
  /** POST /api/Novels/{novelId}/import-txt  （form-data 字段名 file） */
  importTxt: (novelId: number, file: File) =>
    http.upload<null>(`/Novels/${novelId}/import-txt`, file),
}

export const chapterApi = {
  /** GET /api/Chapters?novelId&page&pagesize&isDes */
  list: (params: { novelId: number; page?: number; pagesize?: number; isDes?: boolean }) =>
    http.get<PagedData<ChapterListItem>>('/Chapters', params),
  /**
   * GET /api/Chapters/{chapterId}?novelId=
   * 注意：[Authorize] 状态下后端会自动写入阅读记录（ChaptersController.cs:123）
   * 所以前端不需要单独调「上报阅读进度」的接口
   */
  detail: (chapterId: number, novelId?: number) =>
    http.get<ChapterDetail>(`/Chapters/${chapterId}`, { novelId }),
  /** POST /api/Chapters */
  create: (data: AddChapterRequest) => http.post<number>('/Chapters', data),
  /** DELETE /api/Chapters/{chapterId}?novelId= */
  remove: (chapterId: number, novelId: number) =>
    http.delete<null>(`/Chapters/${chapterId}`, { novelId }),
}

export const commentApi = {
  /** GET /api/Comments?novelId&chapterId&page&pageSize&sortBy */
  list: (params: {
    novelId?: number
    chapterId?: number
    page?: number
    pageSize?: number
    sortBy?: boolean
  }) => http.get<PagedData<CommentItem>>('/Comments', params),
  /** POST /api/Comments （允许匿名，游客 userId 为 null） */
  create: (data: AddCommentRequest) => http.post<null>('/Comments', data),
  /** POST /api/Comments/{commentId}  切换点赞（需登录） */
  toggleLike: (commentId: number) =>
    http.post<ToggleLikeResult>(`/Comments/${commentId}`),
}

export const favoriteApi = {
  /** POST /api/Favorites  切换收藏（需登录，body 是 { novelId }） */
  toggle: (novelId: number) =>
    http.post<ToggleFavoriteResult>('/Favorites', { novelId }),
  /** GET /api/Favorites?page&pageSize&sortBy */
  list: (params?: { page?: number; pageSize?: number; sortBy?: boolean }) =>
    http.get<PagedData<FavoriteItem>>('/Favorites', params),
  /** GET /api/Favorites/check?novelId=  这本书我收藏了没（单条查询） */
  check: (novelId: number) => http.get<boolean>('/Favorites/check', { novelId }),
  /** POST /api/Favorites/remove  body 是 { favoriteId } */
  remove: (favoriteId: number) =>
    http.post<null>('/Favorites/remove', { favoriteId }),
}

export const readingRecordApi = {
  /** GET /api/ReadingRecords/{novelId}/last  这本书上次读到哪（无记录时返回 null） */
  lastRead: (novelId: number) =>
    http.get<{ recordId: number; lastReadChapterId: number | null; lastReadTime: string } | null>(
      `/ReadingRecords/${novelId}/last`,
    ),
  /** GET /api/ReadingRecords?page&pageSize  （记录由后端在打开章节时自动维护） */
  list: (params?: { page?: number; pageSize?: number }) =>
    http.get<PagedData<ReadingRecordItem>>('/ReadingRecords', params),
  /** DELETE /api/ReadingRecords?recordId=  删除单条阅读记录（只能删自己的，删不到返回 404） */
  remove: (recordId: number) =>
    http.delete<number>('/ReadingRecords', { recordId }),
}

export const fileApi = {
  /** POST /api/Files/upload-cover  form-data 字段名是 file，≤2MB，仅 jpg/jpeg/png/gif */
  uploadCover: (file: File) => http.upload<FileUploadResult>('/Files/upload-cover', file),
}
