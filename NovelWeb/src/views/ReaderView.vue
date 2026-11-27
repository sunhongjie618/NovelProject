<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ArrowDown, ArrowLeft, ArrowUp, List, SetUp, Reading } from '@element-plus/icons-vue'
import { chapterApi } from '@/api'
import type { ChapterDetail, ChapterListItem } from '@/types/api'

const route = useRoute()
const router = useRouter()

const chapterId = computed(() => Number(route.params.chapterId))
const novelId = computed(() => Number(route.query.novelId) || 0)

const chapter = ref<ChapterDetail | null>(null)
const chapters = ref<ChapterListItem[]>([])
const loading = ref(false)
const catalogVisible = ref(false)
const catalogPage = ref(1)
const catalogPageSize = 100
const catalogTotal = ref(0)
const settingVisible = ref(false)

/** 阅读设置（只存在本地，不落库） */
const settings = ref({
  fontSize: 18,
  lineHeight: 2,
  theme: 'light' as 'light' | 'sepia' | 'dark',
})

const themeClass = computed(() => `theme-${settings.value.theme}`)
const hasNovelId = computed(() => novelId.value > 0)

async function loadChapter() {
  loading.value = true
  try {
    const detail = await chapterApi.detail(chapterId.value, novelId.value || undefined)
    chapter.value = detail

    // 后端只在带 novelId 时才写阅读记录，链接里没有就补回来
    if (!hasNovelId.value && detail.novelId) {
      router.replace({ query: { ...route.query, novelId: String(detail.novelId) } })
      await loadCatalog(detail.novelId)
    } else {
      await loadCatalog(novelId.value)
    }
  } finally {
    loading.value = false
  }
}

async function loadCatalog(id: number, page = catalogPage.value) {
  if (!id) return
  try {
    const result = await chapterApi.list({
      novelId: id,
      page,
      pagesize: catalogPageSize,
      isDes: false,
    })
    chapters.value = result.data || []
    catalogTotal.value = result.total || chapters.value.length
    catalogPage.value = page
  } catch {
    chapters.value = []
    catalogTotal.value = 0
  }
}

function changeCatalogPage(page: number) {
  catalogPage.value = page
  loadCatalog(novelId.value, page)
}

function goto(chapterIdValue: number | null) {
  if (!chapterIdValue) return
  router.push({
    path: `/read/${chapterIdValue}`,
    query: { novelId: String(novelId.value) },
  })
}

function backToDetail() {
  const id = novelId.value || chapter.value?.novelId
  if (id) router.push(`/novels/${id}`)
  else router.back()
}

function selectChapter(item: ChapterListItem) {
  catalogVisible.value = false
  goto(item.chapterId)
}

watch(chapterId, loadChapter)

onMounted(loadChapter)
</script>

<template>
  <div class="reader" :class="themeClass">
    <!-- 顶栏 -->
    <header class="reader__bar">
      <div class="reader__bar-left">
        <el-button link @click="backToDetail">
          <el-icon><ArrowLeft /></el-icon>
          详情
        </el-button>
        <span class="reader__bar-title text-ellipsis">
          {{ chapter?.title || '加载中…' }}
        </span>
      </div>

      <div class="reader__bar-right">
        <el-button link @click="catalogVisible = true">
          <el-icon><List /></el-icon>
          目录
        </el-button>
        <el-button link @click="settingVisible = true">
          <el-icon><SetUp /></el-icon>
          设置
        </el-button>
      </div>
    </header>

    <!-- 正文 -->
    <main v-loading="loading" class="reader__body">
      <article
        class="reader__article"
        :style="{ fontSize: settings.fontSize + 'px', lineHeight: settings.lineHeight }"
      >
        <h1 class="reader__title">
          第 {{ chapter?.chapterNumber }} 章　{{ chapter?.title }}
        </h1>
        <div class="reader__content">{{ chapter?.content || '' }}</div>
      </article>
    </main>

    <!-- 右侧悬浮按钮 -->
    <div class="reader__float">
      <button class="float-btn" :disabled="!chapter?.preChapterId" @click="goto(chapter?.preChapterId ?? null)">
        <el-icon><ArrowUp /></el-icon>
        <span>上一章</span>
      </button>
      <button class="float-btn" :disabled="!chapter?.nextChapterId" @click="goto(chapter?.nextChapterId ?? null)">
        <el-icon><ArrowDown /></el-icon>
        <span>下一章</span>
      </button>
      <button class="float-btn" @click="catalogVisible = true">
        <el-icon><List /></el-icon>
        <span>目录</span>
      </button>
      <button class="float-btn" @click="backToDetail">
        <el-icon><Reading /></el-icon>
        <span>详情</span>
      </button>
    </div>

    <!-- 目录抽屉 -->
    <el-drawer v-model="catalogVisible" title="目录" direction="rtl" size="380px">
      <div class="catalog">
        <div
          v-for="item in chapters"
          :key="item.chapterId"
          class="catalog__item"
          :class="{ 'is-active': item.chapterId === chapterId }"
          @click="selectChapter(item)"
        >
          <span class="catalog__no">{{ item.chapterNumber }}</span>
          <span class="text-ellipsis">{{ item.title }}</span>
        </div>
      </div>

      <div v-if="catalogTotal > catalogPageSize" class="catalog__pager">
        <el-pagination
          :current-page="catalogPage"
          :page-size="catalogPageSize"
          :total="catalogTotal"
          layout="prev, pager, next"
          small
          background
          @current-change="changeCatalogPage"
        />
      </div>
    </el-drawer>

    <!-- 设置抽屉 -->
    <el-drawer v-model="settingVisible" title="阅读设置" direction="rtl" size="320px">
      <div class="setting">
        <div class="setting__row">
          <span>字号</span>
          <el-slider v-model="settings.fontSize" :min="14" :max="28" :step="1" style="width: 150px" />
          <span class="setting__value">{{ settings.fontSize }}px</span>
        </div>
        <div class="setting__row">
          <span>行距</span>
          <el-slider v-model="settings.lineHeight" :min="1.5" :max="2.6" :step="0.1" style="width: 150px" />
          <span class="setting__value">{{ settings.lineHeight.toFixed(1) }}</span>
        </div>
        <div class="setting__row">
          <span>背景</span>
          <el-radio-group v-model="settings.theme" size="small">
            <el-radio-button value="light">白天</el-radio-button>
            <el-radio-button value="sepia">羊皮纸</el-radio-button>
            <el-radio-button value="dark">夜间</el-radio-button>
          </el-radio-group>
        </div>
      </div>
    </el-drawer>
  </div>
</template>

<style scoped>
.reader {
  display: flex;
  flex-direction: column;
  height: 100vh;
  transition: background 0.2s ease, color 0.2s ease;
}

.reader.theme-light {
  background: #ffffff;
  color: #2c3338;
}

.reader.theme-sepia {
  background: #f6efe0;
  color: #4a3f2f;
}

.reader.theme-dark {
  background: #1b1f24;
  color: #c9d1d9;
}

.reader__bar {
  height: 52px;
  flex: 0 0 52px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 0 20px;
  border-bottom: 1px solid rgba(144, 147, 153, 0.18);
}

.reader__bar-left,
.reader__bar-right {
  display: flex;
  align-items: center;
  gap: 10px;
  min-width: 0;
}

.reader__bar-title {
  font-size: 13px;
  opacity: 0.75;
  max-width: 40vw;
}

.reader__body {
  flex: 1;
  overflow-y: auto;
  padding: 24px 20px 80px;
}

.reader__article {
  max-width: 820px;
  margin: 0 auto;
}

.reader__title {
  font-size: 1.3em;
  font-weight: 700;
  text-align: center;
  margin: 8px 0 28px;
}

.reader__content {
  white-space: pre-wrap;
  word-break: break-word;
  letter-spacing: 0.4px;
}

/* 右侧浮动按钮 */
.reader__float {
  position: fixed;
  right: 18px;
  bottom: 40px;
  display: flex;
  flex-direction: column;
  gap: 10px;
  z-index: 20;
}

.float-btn {
  width: 58px;
  height: 58px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 2px;
  font-size: 12px;
  border-radius: 12px;
  border: 1px solid rgba(144, 147, 153, 0.3);
  background: rgba(255, 255, 255, 0.9);
  color: #4a5462;
  cursor: pointer;
  transition: all 0.18s ease;
  backdrop-filter: blur(6px);
}

.float-btn:hover:not(:disabled) {
  color: var(--app-green);
  border-color: var(--app-green);
  transform: translateY(-2px);
}

.float-btn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

/* 目录 */
.catalog {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.catalog__pager {
  display: flex;
  justify-content: center;
  padding: 16px 0 8px;
}

.catalog__item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 8px 10px;
  border-radius: 8px;
  font-size: 13px;
  cursor: pointer;
  color: #4a5462;
}

.catalog__item:hover {
  background: var(--app-green-soft);
  color: var(--app-green);
}

.catalog__item.is-active {
  background: var(--app-green-soft);
  color: var(--app-green);
  font-weight: 600;
}

.catalog__no {
  flex: 0 0 34px;
  color: #a0a8b0;
  font-size: 12px;
}

/* 设置 */
.setting {
  display: flex;
  flex-direction: column;
  gap: 22px;
}

.setting__row {
  display: flex;
  align-items: center;
  gap: 12px;
  font-size: 13px;
}

.setting__value {
  width: 44px;
  text-align: right;
  color: #909399;
}
</style>
