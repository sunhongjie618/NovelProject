import { fileURLToPath, URL } from 'node:url'
import vue from '@vitejs/plugin-vue'
import { defineConfig } from 'vite'

/** 后端地址：F5 启动后 Swagger 就在这个地址 */
const BACKEND = 'https://localhost:7112'

// https://vite.dev/config/
export default defineConfig({
  plugins: [vue()],

  resolve: {
    alias: {
      // 这样到处都能写 import xxx from '@/api'
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },

  server: {
    port: 5173,
    // 首屏要编译的模块提前预热，避免「打开页面先空白几秒」
    warmup: {
      clientFiles: [
        './src/main.ts',
        './src/App.vue',
        './src/router/index.ts',
        './src/components/AppLayout.vue',
        './src/components/NovelCard.vue',
        './src/views/HomeView.vue',
      ],
    },
    proxy: {
      // 走代理的三大好处：
      // 1. 不触发 CORS（后端只放行了 5500，5173 会被拦）
      // 2. 绕开后端自签 HTTPS 证书（secure:false）
      // 3. 前端代码里写 /api/xxx 就行，不用关心后端端口
      '/api': {
        target: BACKEND,
        changeOrigin: true,
        secure: false,
      },
      // 封面图等静态资源
      '/Picture': {
        target: BACKEND,
        changeOrigin: true,
        secure: false,
      },
      // SignalR 私信，ws:true 才能转发 WebSocket 握手
      '/Hubs': {
        target: BACKEND,
        changeOrigin: true,
        secure: false,
        ws: true,
      },
    },
  },
})
