<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { Search } from '@element-plus/icons-vue'
import { categoryApi, novelApi } from '@/api'
import NovelCard from '@/components/NovelCard.vue'
import type { CategoryItem, NovelListItem } from '@/types/api'

const route = useRoute()
const router = useRouter()

const SORTS = [
  { value: 'update', label: '最近更新' },
  { value: 'click', label: '点击最多' },
  { value: 'favorite', label: '收藏最多' },
]

const categories = ref<CategoryItem[]>([])
const novels = ref<NovelListItem[]>([])
const total = ref(0)
const loading = ref(false)

const keyword = ref((route.query.keyword as string) || '')
const categoryId = ref<number | undefined>(
  route.query.categoryId ? Number(route.query.categoryId) : undefined,
)
const sortType = ref((route.query.sort as string) || 'update')
const isCompleted = ref<boolean | undefined>(
  route.query.status === 'finished' ? true : route.query.status === 'serial' ? false : undefined,
)
const page = ref(route.query.page ? Number(route.query.page) : 1)
const pageSize = 20

const currentSortLabel = computed(
  () => SORTS.find((item) => item.value === sortType.value)?.label || '最近更新',
)

async function loadCategories() {
  categories.value = await categoryApi.list({ isEnable: true, orderBySortAsc: true })
}

async function loadNovels() {
  loading.value = true
  try {
    const result = await novelApi.list({
      categoryId: categoryId.value,
      keyword: keyword.value || undefined,
      page: page.value,
      pagesize: pageSize,
      sortType: sortType.value,
      isCompleted: isCompleted.value,
    })
    novels.value = result.data || []
    total.value = result.total || 0
  } finally {
    loading.value = false
  }
}

/** 把筛选条件写回 URL，刷新 / 分享都能还原 */
function syncQuery() {
  router.replace({
    path: '/novels',
    query: {
      ...(keyword.value ? { keyword: keyword.value } : {}),
      ...(categoryId.value ? { categoryId: String(categoryId.value) } : {}),
      ...(sortType.value && sortType.value !== 'update' ? { sort: sortType.value } : {}),
      ...(isCompleted.value === true ? { status: 'finished' } : {}),
      ...(isCompleted.value === false ? { status: 'serial' } : {}),
      ...(page.value > 1 ? { page: String(page.value) } : {}),
    },
  })
}

function changeSort(value: string) {
  sortType.value = value
  page.value = 1
  syncQuery()
  loadNovels()
}

function changeCategory(value?: number) {
  categoryId.value = value
  page.value = 1
  syncQuery()
  loadNovels()
}

function changeStatus(value: 'all' | 'serial' | 'finished') {
  isCompleted.value = value === 'all' ? undefined : value === 'finished'
  page.value = 1
  syncQuery()
  loadNovels()
}

function doSearch() {
  page.value = 1
  syncQuery()
  loadNovels()
}

function handlePageChange(value: number) {
  page.value = value
  syncQuery()
  loadNovels()
}

onMounted(async () => {
  await loadCategories()
  await loadNovels()
})

// 首页「更多」或别的页面跳进来时，参数变了要重新查
watch(
  () => route.fullPath,
  () => {
    if (route.name !== 'novels') return
    keyword.value = (route.query.keyword as string) || ''
    categoryId.value = route.query.categoryId ? Number(route.query.categoryId) : undefined
    sortType.value = (route.query.sort as string) || 'update'
    page.value = route.query.page ? Number(route.query.page) : 1
  },
)
</script>

<template>
  <div class="page-padding">
    <!-- 排序小标（左上角） -->
    <div class="sort-bar">
      <div class="sort-bar__left">
        <span class="sort-bar__label">排序</span>
        <div class="sort-chips">
          <span
            v-for="item in SORTS"
            :key="item.value"
            class="sort-chip"
            :class="{ 'is-active': sortType === item.value }"
            @click="changeSort(item.value)"
          >
            {{ item.label }}
          </span>
        </div>
      </div>

      <div class="sort-bar__right">
        <el-input
          v-model="keyword"
          placeholder="搜索书名或作者"
          clearable
          style="width: 220px"
          @keyup.enter="doSearch"
        >
          <template #suffix>
            <el-icon style="cursor: pointer" @click="doSearch"><Search /></el-icon>
          </template>
        </el-input>
      </div>
    </div>

    <!-- 分类 + 状态 -->
    <div class="filter-bar">
      <span class="filter-bar__label">分类</span>
      <span
        class="filter-chip"
        :class="{ 'is-active': categoryId === undefined }"
        @click="changeCategory(undefined)"
      >
        全部
      </span>
      <span
        v-for="item in categories"
        :key="item.categoryId"
        class="filter-chip"
        :class="{ 'is-active': categoryId === item.categoryId }"
        @click="changeCategory(item.categoryId)"
      >
        {{ item.categoryName }}
      </span>
    </div>

    <div class="filter-bar">
      <span class="filter-bar__label">状态</span>
      <span
        class="filter-chip"
        :class="{ 'is-active': isCompleted === undefined }"
        @click="changeStatus('all')"
      >
        全部
      </span>
      <span
        class="filter-chip"
        :class="{ 'is-active': isCompleted === false }"
        @click="changeStatus('serial')"
      >
        连载中
      </span>
      <span
        class="filter-chip"
        :class="{ 'is-active': isCompleted === true }"
        @click="changeStatus('finished')"
      >
        已完结
      </span>
    </div>

    <!-- 列表 -->
    <div v-loading="loading" class="novel-list">
      <el-empty v-if="!loading && novels.length === 0" description="没有找到匹配的小说" />
      <div v-else class="card-grid">
        <NovelCard
          v-for="(novel, index) in novels"
          :key="novel.novelId"
          :novel="novel"
          :rank="(page - 1) * pageSize + index + 1"
        />
      </div>
    </div>

    <div v-if="total > pageSize" class="pager">
      <span class="pager__hint">共 {{ total }} 本 · 当前按「{{ currentSortLabel }}」排序</span>
      <el-pagination
        layout="prev, pager, next, jumper"
        :total="total"
        :page-size="pageSize"
        :current-page="page"
        @current-change="handlePageChange"
      />
    </div>
  </div>
</template>

<style scoped>
.sort-bar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  padding: 10px 14px;
  margin-bottom: 12px;
  border-radius: 12px;
  background: rgba(255, 255, 255, 0.78);
  border: 1px solid rgba(255, 255, 255, 0.9);
}

.sort-bar__left {
  display: flex;
  align-items: center;
  gap: 12px;
}

.sort-bar__label,
.filter-bar__label {
  font-size: 13px;
  color: #8a94a0;
  flex: 0 0 auto;
}

.sort-chips {
  display: flex;
  gap: 8px;
}

.sort-chip {
  padding: 4px 12px;
  border-radius: 999px;
  font-size: 13px;
  color: #5a6472;
  background: #f2f5f8;
  cursor: pointer;
  transition: all 0.18s ease;
  user-select: none;
}

.sort-chip:hover {
  color: var(--app-green);
}

.sort-chip.is-active {
  color: #fff;
  background: var(--app-green);
  box-shadow: 0 4px 12px rgba(24, 160, 88, 0.28);
}

.filter-bar {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 8px;
  margin-bottom: 12px;
}

.filter-chip {
  padding: 3px 12px;
  border-radius: 999px;
  font-size: 13px;
  color: #5a6472;
  cursor: pointer;
  transition: all 0.18s ease;
  user-select: none;
}

.filter-chip:hover {
  color: var(--app-green);
}

.filter-chip.is-active {
  color: var(--app-green);
  background: var(--app-green-soft);
  font-weight: 600;
}

.novel-list {
  min-height: 200px;
}

.pager {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 10px;
  margin-top: 24px;
}

.pager__hint {
  font-size: 12px;
  color: #909399;
}
</style>
