<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  Bell,
  Clock,
  Expand,
  Fold,
  Grid,
  HomeFilled,
  Notebook,
  Reading,
  Star,
} from '@element-plus/icons-vue'
import { useAuthStore } from '@/stores/auth'
import { signalR } from '@/utils/signalr'
import AuthDialog from '@/components/AuthDialog.vue'

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()

const collapsed = ref(false)
const authVisible = ref(false)
const authMode = ref<'login' | 'register'>('login')

function openAuth(mode: 'login' | 'register' = 'login') {
  authMode.value = mode
  authVisible.value = true
}

/** 守卫重定向到 /?login=1&redirect=... 时，自动把登录弹框打开 */
function syncAuthByQuery() {
  if (route.query.login === '1' && !auth.isLogin) {
    openAuth(route.query.mode === 'register' ? 'register' : 'login')
  }
}

function handleAuthSuccess() {
  const redirect = route.query.redirect as string | undefined
  if (route.query.login || route.query.redirect) {
    router.replace(redirect || '/')
  }
}

const menus = [
  { path: '/', title: '首页', icon: HomeFilled },
  { path: '/bookshelf', title: '我的收藏', icon: Star },
  { path: '/history', title: '阅读历史', icon: Clock },
  { path: '/novels', title: '小说', icon: Reading },
  { path: '/categories', title: '分类', icon: Grid },
  { path: '/announcements', title: '站务公告', icon: Bell },
  { path: '/my-novels', title: '我的小说', icon: Notebook },
]

/** 未登录时点这些菜单 → 原地弹登录框，不跳转 */
const needLoginPaths = ['/bookshelf', '/history', '/my-novels']

/**
 * el-menu 的点击被拦下时，它内部的高亮已经切到被点项了，
 * 这里用 key 强制重建一次，让它重新以 activeMenu 为准。
 */
const menuTick = ref(0)

function onMenuSelect(path: string) {
  if (needLoginPaths.includes(path) && !auth.isLogin) {
    openAuth('login')
    menuTick.value += 1
    return
  }
  router.push(path)
}

const activeMenu = computed(() => {
  const path = route.path
  if (path.startsWith('/novels')) return '/novels'
  return path
})

function onUserCommand(command: string) {
  if (command === 'logout') {
    auth.clear()
    void signalR.disconnect()
    router.push('/')
  }
}

/** 切回标签页时如果掉线了，补一次重连（后端重启、休眠唤醒都会断开） */
function onVisibilityChange() {
  if (document.visibilityState === 'visible' && auth.isLogin && !signalR.state.connected) {
    void signalR.retry()
  }
}

onMounted(() => {
  if (auth.isLogin) void signalR.connect()
  syncAuthByQuery()
  document.addEventListener('visibilitychange', onVisibilityChange)
})

watch(
  () => route.query.login,
  () => syncAuthByQuery(),
)

watch(
  () => auth.token,
  (token) => {
    if (token) void signalR.connect()
    else void signalR.disconnect()
  },
)

onUnmounted(() => {
  document.removeEventListener('visibilitychange', onVisibilityChange)
  void signalR.disconnect()
})
</script>

<template>
  <div class="app-layout" :class="{ 'is-collapsed': collapsed }">
    <aside class="sidebar">
      <div class="brand" @click="router.push('/')">
        <img src="/images/leaf.svg" alt="冰柏叶小说库" class="brand__logo" />
        <span v-show="!collapsed" class="brand__text">冰柏叶小说库</span>
      </div>

      <el-menu
        :key="`${activeMenu}-${menuTick}`"
        class="sidebar-menu"
        :default-active="activeMenu"
        :collapse="collapsed"
        :collapse-transition="false"
        @select="onMenuSelect"
      >
        <el-menu-item v-for="item in menus" :key="item.path" :index="item.path">
          <el-icon><component :is="item.icon" /></el-icon>
          <template #title>{{ item.title }}</template>
        </el-menu-item>
      </el-menu>

      <div class="collapse-btn" @click="collapsed = !collapsed">
        <el-icon><component :is="collapsed ? Expand : Fold" /></el-icon>
        <span v-show="!collapsed">收起菜单</span>
      </div>
    </aside>

    <div class="main">
      <header class="topbar">
        <div class="topbar__right">
          <el-dropdown v-if="auth.isLogin" @command="onUserCommand">
            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item command="logout" divided>退出登录</el-dropdown-item>
              </el-dropdown-menu>
            </template>
            <span class="user-entry">
              <el-avatar :size="26">{{ (auth.nickName || '?').slice(0, 1) }}</el-avatar>
              <span class="user-entry__name">{{ auth.nickName }}</span>
            </span>
          </el-dropdown>
          <template v-else>
            <el-button link @click="openAuth('login')">登录</el-button>
            <el-divider direction="vertical" />
            <el-button link @click="openAuth('register')">注册</el-button>
          </template>
        </div>
      </header>

      <main class="content">
        <router-view />
      </main>
    </div>

    <AuthDialog v-model="authVisible" :mode="authMode" @success="handleAuthSuccess" />
  </div>
</template>

<style scoped>
.app-layout {
  display: flex;
  height: 100vh;
  overflow: hidden;
}

.sidebar {
  display: flex;
  flex: 0 0 var(--app-sidebar-width);
  flex-direction: column;
  width: var(--app-sidebar-width);
  background: #fff;
  border-right: 1px solid #eef1f4;
  overflow: hidden;
  transition: width 0.2s, flex-basis 0.2s;
}

.is-collapsed .sidebar {
  flex-basis: var(--app-sidebar-width-collapsed);
  width: var(--app-sidebar-width-collapsed);
}

.brand {
  display: flex;
  align-items: center;
  gap: 10px;
  height: 60px;
  padding: 0 14px;
  border-bottom: 1px solid #f4f6f8;
  cursor: pointer;
  overflow: hidden;
  white-space: nowrap;
}

.brand__logo {
  flex: 0 0 30px;
  width: 30px;
  height: 30px;
  object-fit: contain;
}

.brand__text {
  font-size: 16px;
  font-weight: 800;
  color: #1f2d3d;
  letter-spacing: 1px;
  white-space: nowrap;
}

.sidebar-menu {
  flex: 1;
  padding-top: 8px;
  border-right: none;
  overflow-y: auto;
  overflow-x: hidden;
}

.sidebar-menu:not(.el-menu--collapse) {
  width: 100%;
}

.collapse-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  height: 48px;
  font-size: 13px;
  color: #8a94a0;
  border-top: 1px solid #f4f6f8;
  cursor: pointer;
  user-select: none;
  white-space: nowrap;
}

.collapse-btn:hover {
  color: var(--app-green);
  background: var(--app-green-soft);
}

.main {
  display: flex;
  flex: 1;
  flex-direction: column;
  min-width: 0;
  overflow: hidden;
}

.topbar {
  z-index: 10;
  display: flex;
  flex: 0 0 44px;
  align-items: center;
  justify-content: flex-end;
  gap: 16px;
  height: 44px;
  padding: 0 24px;
  background: transparent;
  border-bottom: none;
}

.topbar__right {
  display: flex;
  flex: none;
  align-items: center;
  justify-content: flex-end;
  gap: 4px;
  width: 180px;
}

.user-entry {
  display: flex;
  align-items: center;
  gap: 8px;
  color: #303133;
  cursor: pointer;
  outline: none;
}

.user-entry__name {
  max-width: 110px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.content {
  flex: 1;
  overflow-y: auto;
}
</style>
