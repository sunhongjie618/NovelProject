import { createRouter, createWebHistory } from 'vue-router'
import type { RouteRecordRaw } from 'vue-router'
import { TOKEN_KEY } from '@/utils/request'
import AppLayout from '@/components/AppLayout.vue'

const routes: RouteRecordRaw[] = [
  // ---------- 带左侧菜单的主布局 ----------
  {
    path: '/',
    component: AppLayout,
    children: [
      {
        path: '',
        name: 'home',
        component: () => import('@/views/HomeView.vue'),
        meta: { title: '首页' },
      },
      {
        path: 'novels',
        name: 'novels',
        component: () => import('@/views/NovelsView.vue'),
        meta: { title: '小说' },
      },
      {
        path: 'novels/:novelId(\\d+)',
        name: 'novel-detail',
        component: () => import('@/views/NovelDetailView.vue'),
        meta: { title: '小说详情' },
      },
      {
        path: 'announcements',
        name: 'announcements',
        component: () => import('@/views/AnnouncementsView.vue'),
        meta: { title: '站务公告' },
      },
      {
        path: 'categories',
        name: 'categories',
        component: () => import('@/views/CategoriesView.vue'),
        meta: { title: '分类' },
      },
      {
        path: 'bookshelf',
        name: 'bookshelf',
        component: () => import('@/views/BookshelfView.vue'),
        meta: { title: '我的收藏', requiresAuth: true },
      },
      {
        path: 'history',
        name: 'history',
        component: () => import('@/views/HistoryView.vue'),
        meta: { title: '阅读历史', requiresAuth: true },
      },
      {
        path: 'my-novels',
        name: 'my-novels',
        component: () => import('@/views/MyNovelsView.vue'),
        meta: { title: '我的小说', requiresAuth: true },
      },
    ],
  },

  // ---------- 全屏页面（不带菜单） ----------
  {
    path: '/read/:chapterId(\\d+)',
    name: 'reader',
    component: () => import('@/views/ReaderView.vue'),
    meta: { title: '阅读' },
  },
  // 登录/注册不再单独占页面，改为在布局内弹框（AuthDialog）
  { path: '/login', redirect: { path: '/', query: { login: '1' } } },
  { path: '/register', redirect: { path: '/', query: { login: '1', mode: 'register' } } },

  // ---------- 兼容旧地址 ----------
  { path: '/novel/:novelId(\\d+)', redirect: (to) => `/novels/${to.params.novelId}` },
  { path: '/records', redirect: '/history' },
  { path: '/upload', redirect: '/my-novels' },
  { path: '/:pathMatch(.*)*', redirect: '/' },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
  scrollBehavior() {
    return { top: 0 }
  },
})

// 未登录访问需要登录的页面 → 回到首页并自动弹出登录框，同时记住来源
router.beforeEach((to) => {
  const token = localStorage.getItem(TOKEN_KEY)

  if (to.meta.requiresAuth && !token) {
    return { path: '/', query: { login: '1', redirect: to.fullPath } }
  }

  return true
})

router.afterEach((to) => {
  const title = (to.meta.title as string) || ''
  document.title = title ? `${title} · 冰柏叶小说库` : '冰柏叶小说库'
})

export default router
