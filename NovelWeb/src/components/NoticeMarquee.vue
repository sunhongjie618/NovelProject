<script setup lang="ts">
import { computed } from 'vue'
import { Bell } from '@element-plus/icons-vue'
import { signalR } from '@/utils/signalr'

/**
 * 首页滚动广播：只播放「最新一条」系统广播。
 *
 * 数据来源是后端 ChatRoomHub.SendSystemNotification → Clients.All.SendAsync("ReceiveSystemMessage")，
 * 前端落在 signalR.notices[0]（{ title, content, time }）。
 * 站务公告页（/announcements）走的是另一条通道 ReceivePublicMessage，两者互不影响。
 *
 * 样式：最左一枚小铃铛（与主题图左边缘对齐），其余空间里文字横向循环滚动。
 * 没有边框、没有标签、鼠标移上去也不暂停。
 */
const DEMO_NOTICE = '欢迎各位访问冰柏叶小说库，希望各位快快乐乐、生活幸福。'

const text = computed(() => {
  const notice = signalR.notices[0]
  if (!notice) return DEMO_NOTICE
  const title = (notice.title || '').trim()
  const content = (notice.content || '').trim()
  if (title && content) return `${title}：${content}`
  return title || content || DEMO_NOTICE
})
</script>

<template>
  <div class="notice-line">
    <el-icon class="notice-line__icon"><Bell /></el-icon>

    <div class="notice-line__track">
      <div class="notice-line__roll">
        <span class="notice-line__text">{{ text }}</span>
      </div>
    </div>
  </div>
</template>

<style scoped>
.notice-line {
  display: flex;
  align-items: center;
  gap: 12px;
  height: 34px;
  overflow: hidden;
}

.notice-line__icon {
  flex-shrink: 0;
  color: var(--app-green, #18a058);
  font-size: 15px;
}

.notice-line__track {
  flex: 1;
  min-width: 0;
  overflow: hidden;
  mask-image: linear-gradient(90deg, transparent, #000 48px, #000 calc(100% - 48px), transparent);
}

.notice-line__roll {
  display: inline-block;
  white-space: nowrap;
  /* padding-left:100% 把起点推到轨道右边界之外，于是文字从最右侧开始往左滚 */
  padding-left: 100%;
  animation: notice-line-roll 30s linear infinite;
  will-change: transform;
}

.notice-line__text {
  font-size: 13px;
  letter-spacing: 1px;
  color: #7b8794;
  padding-right: 0;
}

@keyframes notice-line-roll {
  from {
    transform: translateX(0);
  }
  to {
    /* 自身宽度 = 轨道宽 + 文字宽，正好把文字完整送出去 */
    transform: translateX(-100%);
  }
}

@media (prefers-reduced-motion: reduce) {
  .notice-line__roll {
    animation: none;
  }
}
</style>
