<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { Search } from '@element-plus/icons-vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { favoriteApi, novelApi } from '@/api'
import type { FavoriteItem } from '@/types/api'
import { formatDate, resolveImageUrl } from '@/utils/format'

/**
 * 我的收藏。
 * 后端没有「按关键词搜我的收藏」的接口，所以两步组合：
 *   1) 翻页拉全量收藏（GET /api/Favorites）
 *   2) 有关键词时再调 GET /api/Novels?keyword=… 做服务端模糊查询（书名 / 作者 LIKE），
 *      取回命中的 novelId 集合，与收藏列表取交集。
 * 这样复用了 GetNovels 的模糊查询能力，且不需要改后端。
 */
const router = useRouter()

const loading = ref(false)
const keyword = ref('')
const sortType = ref<'favorite' | 'name'>('favorite')
const page = ref(1)
const pageSize = 12

const allFavorites = ref<FavoriteItem[]>([])
/** 关键词命中的 novelId；null 表示没有关键词、不过滤 */
const matchedIds = ref<Set<number> | null>(null)

const filtered = computed(() => {
  let list = allFavorites.value
  if (matchedIds.value) {
    const ids = matchedIds.value
    list = list.filter((item) => ids.has(item.novelId))
  }
  const sorted = [...list]
  if (sortType.value === 'name') {
    sorted.sort((a, b) => (a.novelName || '').localeCompare(b.novelName || '', 'zh-Hans-CN'))
  } else {
    sorted.sort((a, b) => new Date(b.createTime).getTime() - new Date(a.createTime).getTime())
  }
  return sorted
})

const total = computed(() => filtered.value.length)
const visible = computed(() => {
  const start = (page.value - 1) * pageSize
  return filtered.value.slice(start, start + pageSize)
})

const shelfTruncated = ref(false)

async function loadAllFavorites() {
  const all: FavoriteItem[] = []
  let current = 1
  // 后端 /api/Favorites 的分页上限由 pagesize 决定，这里 100/页、最多 100 页（1 万本）
  for (;;) {
    const res = await favoriteApi.list({ page: current, pageSize: 100, sortBy: true })
    const data = res.data || []
    all.push(...data)
    const serverTotal = res.total || 0
    if (data.length === 0 || all.length >= serverTotal) break
    if (current >= 100) {
      shelfTruncated.value = true
      break
    }
    current += 1
  }
  allFavorites.value = all
}

async function searchByKeyword() {
  const kw = keyword.value.trim()
  if (!kw) {
    matchedIds.value = null
    return
  }
  // 关键词命中集：逐页取回，最多 10 页（5000 条命中），避免只搜到第一页
  const ids = new Set<number>()
  for (let page = 1; page <= 10; page += 1) {
    const res = await novelApi.list({ keyword: kw, page, pagesize: 500 })
    const rows = res.data || []
    rows.forEach((item) => ids.add(item.novelId))
    if (rows.length < 500 || page * 500 >= (res.total || 0)) break
  }
  matchedIds.value = ids
}

async function load() {
  loading.value = true
  try {
    if (allFavorites.value.length === 0) await loadAllFavorites()
    await searchByKeyword()
  } catch {
    /* 拦截器已统一提示 */
  } finally {
    loading.value = false
  }
}

let timer: number | undefined
watch(keyword, () => {
  page.value = 1
  window.clearTimeout(timer)
  timer = window.setTimeout(() => {
    load()
  }, 320)
})

watch(sortType, () => {
  page.value = 1
})

async function remove(item: FavoriteItem) {
  try {
    await ElMessageBox.confirm(`确认取消收藏《${item.novelName}》吗？`, '提示', {
      type: 'warning',
      confirmButtonText: '取消收藏',
      cancelButtonText: '再想想',
    })
  } catch {
    return
  }
  try {
    await favoriteApi.remove(item.favoriteId)
    allFavorites.value = allFavorites.value.filter((f) => f.favoriteId !== item.favoriteId)
    ElMessage.success('已取消收藏')
    if (visible.value.length === 0 && page.value > 1) page.value -= 1
  } catch {
    /* 拦截器已统一提示 */
  }
}

function open(item: FavoriteItem) {
  router.push(`/novels/${item.novelId}`)
}

function coverOf(item: FavoriteItem) {
  return resolveImageUrl(item.coverUrl) || '/images/no-cover.jpg'
}

onMounted(load)
</script>

<template>
  <div class="page-padding">
    <div class="shelf-head">
      <div>
        <h2 class="shelf-head__title">我的收藏</h2>
        <p class="shelf-head__sub">
          共 {{ total }} 本 · 支持按书名 / 作者模糊搜索
          <span v-if="shelfTruncated" class="shelf-head__warn">（收藏量过大，仅载入前 1 万本）</span>
        </p>
      </div>
    </div>

    <div class="shelf-bar">
      <el-input
        v-model="keyword"
        placeholder="搜索我收藏里的书名或作者"
        clearable
        class="shelf-bar__search"
      >
        <template #prefix>
          <el-icon><Search /></el-icon>
        </template>
      </el-input>

      <div class="shelf-bar__sort">
        <span class="shelf-bar__label">排序</span>
        <el-radio-group v-model="sortType" size="small">
          <el-radio-button value="favorite">收藏时间</el-radio-button>
          <el-radio-button value="name">书名</el-radio-button>
        </el-radio-group>
      </div>
    </div>

    <el-skeleton v-if="loading" :rows="6" animated />

    <el-empty
      v-else-if="total === 0"
      :description="keyword ? '没有匹配的收藏，换个关键词试试' : '还没有收藏，去书城逛逛吧'"
    >
      <el-button v-if="!keyword" type="primary" @click="router.push('/novels')">
        去书城逛逛
      </el-button>
    </el-empty>

    <div v-else class="shelf-grid">
      <div v-for="item in visible" :key="item.favoriteId" class="book">
        <div class="book__cover" @click="open(item)">
          <img :src="coverOf(item)" :alt="item.novelName" loading="lazy" />
          <button class="book__remove" type="button" @click.stop="remove(item)">移出收藏</button>
        </div>
        <div class="book__title" :title="item.novelName" @click="open(item)">
          {{ item.novelName }}
        </div>
        <div class="book__meta">
          <span class="book__author">{{ item.author || '佚名' }}</span>
          <span>{{ formatDate(item.createTime) }} 收藏</span>
        </div>
      </div>
    </div>

    <div v-if="total > pageSize" class="shelf-pager">
      <el-pagination
        v-model:current-page="page"
        :page-size="pageSize"
        :total="total"
        layout="prev, pager, next"
        background
        hide-on-single-page
      />
    </div>
  </div>
</template>

<style scoped>
.shelf-head {
  margin-bottom: 14px;
}

.shelf-head__title {
  margin: 0 0 6px;
  font-size: 26px;
  font-weight: 800;
  color: #1f2d3d;
  letter-spacing: 1px;
}

.shelf-head__warn {
  color: #e6a23c;
}

.shelf-head__sub {
  margin: 0;
  font-size: 12px;
  color: #8a97a5;
}

.shelf-bar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 18px;
  flex-wrap: wrap;
  margin-bottom: 18px;
}

.shelf-bar__search {
  max-width: 420px;
}

.shelf-bar__sort {
  display: flex;
  align-items: center;
  gap: 10px;
}

.shelf-bar__label {
  font-size: 12.5px;
  color: #8a97a5;
}

.shelf-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(168px, 1fr));
  gap: 22px 18px;
}

.book__cover {
  position: relative;
  border-radius: 10px;
  overflow: hidden;
  aspect-ratio: 3 / 4;
  background: #eef2f5;
  cursor: pointer;
  box-shadow: 0 3px 12px rgba(23, 43, 77, 0.08);
  transition:
    transform 0.18s ease,
    box-shadow 0.18s ease;
}

.book__cover:hover {
  transform: translateY(-4px);
  box-shadow: 0 10px 24px rgba(23, 43, 77, 0.16);
}

.book__cover img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
}

.book__remove {
  position: absolute;
  left: 0;
  right: 0;
  bottom: 0;
  height: 30px;
  border: none;
  background: rgba(12, 18, 26, 0.66);
  color: #fff;
  font-size: 12.5px;
  cursor: pointer;
  opacity: 0;
  transition: opacity 0.18s ease;
}

.book__cover:hover .book__remove {
  opacity: 1;
}

.book__title {
  margin-top: 9px;
  font-size: 13.5px;
  line-height: 1.45;
  font-weight: 600;
  color: #24343f;
  cursor: pointer;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.book__title:hover {
  color: var(--app-green, #18a058);
}

.book__meta {
  margin-top: 5px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  font-size: 11.5px;
  color: #9aa6b2;
}

.book__author {
  max-width: 50%;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.shelf-pager {
  display: flex;
  justify-content: center;
  margin-top: 26px;
}

@media (max-width: 640px) {
  .shelf-grid {
    grid-template-columns: repeat(auto-fill, minmax(130px, 1fr));
    gap: 16px 12px;
  }
}
</style>
