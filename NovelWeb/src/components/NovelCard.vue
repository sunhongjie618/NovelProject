<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRouter } from 'vue-router'
import type { NovelListItem } from '@/types/api'
import {
  formatNumber,
  formatWordCount,
  resolveImageUrl,
  statusText,
} from '@/utils/format'

const props = defineProps<{
  novel: NovelListItem
  /** 排序榜序号，传了就显示左上角小标 */
  rank?: number
}>()

const router = useRouter()
const broken = ref(false)

const cover = computed(() => {
  const url = resolveImageUrl(props.novel.coverUrl)
  // 没有封面 / 加载失败 → 统一用站内默认封面
  return url || '/images/no-cover.jpg'
})
const isFinished = computed(() => props.novel.status === 2)

function open() {
  router.push(`/novels/${props.novel.novelId}`)
}
</script>

<template>
  <div class="novel-card" @click="open">
    <div class="novel-card__cover">
      <img
        :src="broken ? '/images/no-cover.jpg' : cover"
        :alt="novel.title"
        loading="lazy"
        @error="broken = true"
      />

      <span v-if="rank" class="novel-card__rank">{{ rank }}</span>
      <span class="novel-card__status" :class="{ 'is-finished': isFinished }">
        {{ statusText(novel.status) }}
      </span>
    </div>

    <div class="novel-card__body">
      <div class="novel-card__title text-ellipsis-2">{{ novel.title }}</div>
      <div class="novel-card__meta text-ellipsis">
        {{ novel.authorName || '佚名' }} · {{ formatWordCount(novel.wordCount) }}
      </div>
      <div class="novel-card__footer">
        <span class="novel-card__cat">{{ novel.categoryName || '未分类' }}</span>
        <span class="novel-card__stats">
          <span title="点击量">👁 {{ formatNumber(novel.clickCount) }}</span>
          <span title="收藏数">★ {{ formatNumber(novel.favoriteCount) }}</span>
        </span>
      </div>
    </div>
  </div>
</template>

<style scoped>
.novel-card__rank {
  position: absolute;
  top: 8px;
  left: 8px;
  min-width: 24px;
  height: 24px;
  padding: 0 6px;
  border-radius: 8px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  font-weight: 700;
  color: #fff;
  background: rgba(255, 92, 92, 0.92);
  backdrop-filter: blur(4px);
}

.novel-card__stats {
  display: inline-flex;
  gap: 8px;
  font-size: 12px;
  color: #8a94a0;
  white-space: nowrap;
}
</style>
