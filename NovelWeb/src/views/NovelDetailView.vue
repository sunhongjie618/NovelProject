<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { chapterApi, favoriteApi, novelApi, readingRecordApi } from '@/api'
import CommentSection from '@/components/CommentSection.vue'
import { useAuthStore } from '@/stores/auth'
import type { ChapterListItem, NovelDetail } from '@/types/api'
import {
  formatNumber,
  formatDate,
  formatWordCount,
  resolveImageUrl,
  statusText,
} from '@/utils/format'

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()

const novelId = computed(() => Number(route.params.novelId))
const novel = ref<NovelDetail | null>(null)
const chapters = ref<ChapterListItem[]>([])
const loading = ref(false)
const chapterLoading = ref(false)
const favoriting = ref(false)
const isFavorited = ref(false)
const activeTab = ref<'catalog' | 'comment'>('catalog')
const lastChapterId = ref<number | null>(null)

/** 章节目录分页 */
const chapterPage = ref(1)
const chapterPageSize = 100
const chapterTotal = ref(0)
/** 第一章 id（用于「开始阅读」，与当前目录页码无关） */
const firstChapterId = ref<number | null>(null)

const cover = computed(
  () => resolveImageUrl(novel.value?.coverUrl) || '/images/no-cover.jpg',
)

async function loadDetail() {
  loading.value = true
  try {
    novel.value = await novelApi.detail(novelId.value)
  } finally {
    loading.value = false
  }
}

async function loadChapters(page = chapterPage.value) {
  chapterLoading.value = true
  try {
    const result = await chapterApi.list({
      novelId: novelId.value,
      page,
      pagesize: chapterPageSize,
      isDes: false,
    })
    chapters.value = result.data || []
    chapterTotal.value = result.total || chapters.value.length
    if (page === 1) firstChapterId.value = chapters.value[0]?.chapterId ?? null
  } catch {
    chapters.value = []
  } finally {
    chapterLoading.value = false
  }
}

function changeChapterPage(value: number) {
  chapterPage.value = value
  loadChapters(value)
}

/**
 * 后端没有「这本书我收藏了没」的查询接口（只有 POST 切换），
 * 只能拉一次收藏列表在本地匹配（页数很少，代价可接受）。
 */
async function loadFavoriteState() {
  if (!auth.isLogin) return
  try {
    // GET /api/Favorites/check?novelId= → 一次索引查询
    isFavorited.value = await favoriteApi.check(novelId.value)
  } catch {
    isFavorited.value = false
  }
}

async function loadReadingRecord() {
  if (!auth.isLogin) return
  try {
    // GET /api/ReadingRecords/{novelId}/last → 一条记录，没有就是 null
    const record = await readingRecordApi.lastRead(novelId.value)
    lastChapterId.value = record?.lastReadChapterId ?? null
  } catch {
    lastChapterId.value = null
  }
}
async function toggleFavorite() {
  if (!auth.isLogin) {
    ElMessage.warning('登录后才能收藏')
    router.push({ path: '/', query: { login: '1', redirect: route.fullPath } })
    return
  }
  favoriting.value = true
  try {
    const result = await favoriteApi.toggle(novelId.value)
    isFavorited.value = result.isFavorited
    if (novel.value) novel.value.favoriteCount = result.favoriteCount
    ElMessage.success(result.isFavorited ? '已加入收藏' : '已取消收藏')
  } finally {
    favoriting.value = false
  }
}

function readChapter(chapterId: number) {
  router.push({ path: `/read/${chapterId}`, query: { novelId: String(novelId.value) } })
}

async function startReading() {
  if (lastChapterId.value) {
    readChapter(lastChapterId.value)
    return
  }
  if (firstChapterId.value) {
    readChapter(firstChapterId.value)
    return
  }
  // 目录停在后面的页时，单独取第一页第一条
  try {
    const result = await chapterApi.list({ novelId: novelId.value, page: 1, pagesize: 1, isDes: false })
    const first = result.data?.[0]
    if (first) {
      readChapter(first.chapterId)
      return
    }
  } catch {
    /* 落到下面的提示 */
  }
  ElMessage.info('这本小说还没有章节')
}

onMounted(async () => {
  await loadDetail()
  await Promise.all([loadChapters(), loadFavoriteState(), loadReadingRecord()])
})
</script>

<template>
  <div v-loading="loading" class="detail">
    <!-- 顶部信息区 -->
    <section class="detail__hero">
      <div
        v-if="cover"
        class="detail__hero-bg"
        :style="{ backgroundImage: `url('${cover}')` }"
      />
      <div class="detail__hero-mask" />

      <div class="detail__hero-content">
        <div class="detail__cover">
          <img :src="cover" :alt="novel?.title" />
        </div>

        <div class="detail__info">
          <h1 class="detail__title">{{ novel?.title || '加载中…' }}</h1>

          <div class="detail__tags">
            <el-tag size="small" type="success" effect="plain">
              {{ statusText(novel?.status) }}
            </el-tag>
            <el-tag v-if="novel?.categoryName" size="small" effect="plain">
              {{ novel.categoryName }}
            </el-tag>
            <el-tag size="small" effect="plain" type="info">
              {{ formatWordCount(novel?.wordCount) }}
            </el-tag>
          </div>

          <div class="detail__meta">
            <span>作者：{{ novel?.authorName || '佚名' }}</span>
            <span>发布于 {{ formatDate(novel?.publishTime) }}</span>
            <span>共 {{ novel?.chapterCount || chapters.length }} 章</span>
          </div>

          <div class="detail__stats">
            <div class="stat">
              <strong>{{ formatNumber(novel?.clickCount) }}</strong>
              <span>点击</span>
            </div>
            <div class="stat">
              <strong>{{ formatNumber(novel?.favoriteCount) }}</strong>
              <span>收藏</span>
            </div>
            <div class="stat">
              <strong>{{ novel?.chapterCount || chapters.length }}</strong>
              <span>章节</span>
            </div>
          </div>

          <div class="detail__actions">
            <el-button type="primary" size="large" @click="startReading">
              {{ lastChapterId ? '继续阅读' : '开始阅读' }}
            </el-button>
            <el-button
              size="large"
              :type="isFavorited ? 'success' : 'default'"
              :loading="favoriting"
              @click="toggleFavorite"
            >
              {{ isFavorited ? '★ 已收藏' : '☆ 收藏' }}
            </el-button>
            <el-button size="large" @click="activeTab = 'comment'">评论</el-button>
          </div>
        </div>
      </div>
    </section>

    <!-- 简介 + 目录 / 评论 -->
    <div class="page-padding">
      <div class="intro">
        <div class="section-title" style="margin-top: 0">简介</div>
        <p class="intro__text">
          {{ novel?.description || '作者还没有填写简介。' }}
        </p>
      </div>

      <el-tabs v-model="activeTab" class="detail__tabs">
        <el-tab-pane :label="`目录（${chapterTotal}）`" name="catalog">
          <div v-loading="chapterLoading" class="catalog">
            <el-empty v-if="!chapterLoading && chapters.length === 0" description="还没有章节" />
            <div v-else class="catalog__grid">
              <div
                v-for="chapter in chapters"
                :key="chapter.chapterId"
                class="catalog__item"
                @click="readChapter(chapter.chapterId)"
              >
                <span class="catalog__no">第 {{ chapter.chapterNumber }} 章</span>
                <span class="catalog__name text-ellipsis">{{ chapter.title }}</span>
                <el-tag v-if="chapter.isLocked" size="small" type="warning">锁</el-tag>
              </div>
            </div>

            <div v-if="chapterTotal > chapterPageSize" class="catalog__pager">
              <el-pagination
                :current-page="chapterPage"
                :page-size="chapterPageSize"
                :total="chapterTotal"
                layout="prev, pager, next, jumper"
                background
                small
                @current-change="changeChapterPage"
              />
            </div>
          </div>
        </el-tab-pane>

        <el-tab-pane label="评论" name="comment">
          <CommentSection :novel-id="novelId" />
        </el-tab-pane>
      </el-tabs>
    </div>
  </div>
</template>

<style scoped>
.detail__hero {
  position: relative;
  overflow: hidden;
  padding: 32px 28px 36px;
}

.detail__hero-bg {
  position: absolute;
  inset: -40px;
  background-size: cover;
  background-position: center;
  filter: blur(28px) saturate(1.1);
  transform: scale(1.15);
  opacity: 0.75;
}

.detail__hero-mask {
  position: absolute;
  inset: 0;
  background: linear-gradient(
    180deg,
    rgba(244, 246, 248, 0.72) 0%,
    rgba(244, 246, 248, 0.95) 100%
  );
}

.detail__hero-content {
  position: relative;
  display: flex;
  gap: 26px;
  align-items: flex-start;
}

.detail__cover {
  flex: 0 0 200px;
  width: 200px;
  aspect-ratio: 3 / 4;
  border-radius: 14px;
  overflow: hidden;
  background: linear-gradient(135deg, #eef3f7, #dfe8ee);
  box-shadow: 0 16px 36px rgba(31, 45, 61, 0.22);
}

.detail__cover img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.detail__cover-empty {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 34px;
  font-weight: 700;
  color: #9fb0bd;
}

.detail__info {
  flex: 1;
  min-width: 0;
}

.detail__title {
  margin: 0 0 12px;
  font-size: 28px;
  font-weight: 800;
  color: #1f2d3d;
}

.detail__tags {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-bottom: 10px;
}

.detail__meta {
  display: flex;
  flex-wrap: wrap;
  gap: 16px;
  font-size: 13px;
  color: #7d8896;
  margin-bottom: 16px;
}

.detail__stats {
  display: flex;
  gap: 28px;
  margin-bottom: 20px;
}

.stat {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
}

.stat strong {
  font-size: 20px;
  color: var(--app-green);
}

.stat span {
  font-size: 12px;
  color: #909399;
}

.detail__actions {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
}

.intro__text {
  margin: 0 0 24px;
  font-size: 14px;
  line-height: 1.9;
  color: #4a5462;
  white-space: pre-wrap;
}

.catalog {
  min-height: 120px;
}

.catalog__pager {
  display: flex;
  justify-content: center;
  margin-top: 20px;
}

.catalog__grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
  gap: 8px 16px;
}

.catalog__item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 12px;
  border-radius: 10px;
  cursor: pointer;
  font-size: 13px;
  color: #4a5462;
  transition: background 0.18s ease, color 0.18s ease;
}

.catalog__item:hover {
  background: var(--app-green-soft);
  color: var(--app-green);
}

.catalog__no {
  flex: 0 0 auto;
  color: #a0a8b0;
}

.catalog__name {
  flex: 1;
  min-width: 0;
}
</style>
