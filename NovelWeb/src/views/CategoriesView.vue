<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { categoryApi } from '@/api'
import type { CategoryItem } from '@/types/api'

const router = useRouter()
const categories = ref<CategoryItem[]>([])
const loading = ref(false)

async function load() {
  loading.value = true
  try {
    categories.value = await categoryApi.list({ isEnable: true, orderBySortAsc: true })
  } finally {
    loading.value = false
  }
}

function open(item: CategoryItem) {
  router.push({ path: '/novels', query: { categoryId: String(item.categoryId) } })
}

onMounted(load)
</script>

<template>
  <div class="page-padding">
    <div class="section-title">全部分类</div>
    <p class="tip">点击分类查看该分类下的小说；分类由后台维护。</p>

    <div v-loading="loading" class="cat-grid">
      <el-empty v-if="!loading && categories.length === 0" description="暂无分类" />
      <div v-for="item in categories" :key="item.categoryId" class="cat-card" @click="open(item)">
        <div class="cat-card__top">
          <span class="cat-card__name">{{ item.categoryName }}</span>
          <el-tag v-if="!item.isEnable" size="small" type="info">已停用</el-tag>
        </div>
        <p class="cat-card__desc text-ellipsis-2">
          {{ item.description || '这个分类还没有简介' }}
        </p>
        <div class="cat-card__footer">
          <span>排序 {{ item.sortOrder }}</span>
          <span class="cat-card__more">浏览 →</span>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.tip {
  margin: -8px 0 18px;
  font-size: 13px;
  color: #909399;
}

.cat-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 16px;
  min-height: 120px;
}

.cat-card {
  padding: 16px 18px;
  border-radius: 14px;
  cursor: pointer;
  background: rgba(255, 255, 255, 0.78);
  border: 1px solid rgba(255, 255, 255, 0.9);
  box-shadow: 0 6px 18px rgba(31, 45, 61, 0.07);
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.cat-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 12px 26px rgba(31, 45, 61, 0.14);
}

.cat-card__top {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 8px;
}

.cat-card__name {
  font-size: 16px;
  font-weight: 700;
  color: #1f2d3d;
}

.cat-card__desc {
  margin: 0 0 12px;
  font-size: 13px;
  line-height: 1.6;
  color: #7d8896;
  min-height: 40px;
}

.cat-card__footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  font-size: 12px;
  color: #a0a8b0;
}

.cat-card__more {
  color: var(--app-green);
}
</style>
