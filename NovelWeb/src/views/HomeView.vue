<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { Search } from '@element-plus/icons-vue'
import { favoriteApi, novelApi } from '@/api'
import NovelCard from '@/components/NovelCard.vue'
import NoticeMarquee from '@/components/NoticeMarquee.vue'
import { useAuthStore } from '@/stores/auth'
import type { FavoriteItem, NovelListItem } from '@/types/api'
import { resolveImageUrl } from '@/utils/format'

const router = useRouter()
const auth = useAuthStore()

const keyword = ref('')

const latest = ref<NovelListItem[]>([])
const hottest = ref<NovelListItem[]>([])
const favorites = ref<FavoriteItem[]>([])

const loading = reactive({ latest: false, hottest: false, favorites: false })

async function loadLatest() {
  loading.latest = true
  try {
    const result = await novelApi.list({ page: 1, pagesize: 12, sortType: 'update' })
    latest.value = result.data || []
  } finally {
    loading.latest = false
  }
}

async function loadHottest() {
  loading.hottest = true
  try {
    const result = await novelApi.list({ page: 1, pagesize: 6, sortType: 'click' })
    hottest.value = result.data || []
  } finally {
    loading.hottest = false
  }
}

async function loadFavorites() {
  if (!auth.isLogin) {
    favorites.value = []
    return
  }
  loading.favorites = true
  try {
    const result = await favoriteApi.list({ page: 1, pageSize: 6, sortBy: true })
    favorites.value = result.data || []
  } catch {
    favorites.value = []
  } finally {
    loading.favorites = false
  }
}

function search() {
  router.push({ path: '/novels', query: keyword.value ? { keyword: keyword.value } : {} })
}

function openFavorite(item: FavoriteItem) {
  router.push(`/novels/${item.novelId}`)
}

onMounted(() => {
  // 三个区块互不依赖，同时发出，避免串行等待
  loadLatest()
  loadHottest()
  loadFavorites()
})
</script>

<template>
  <div class="home">
    <!-- 顶部：整幅主题图居中铺满，文案与搜索居中压在图上（参考站样式） -->
    <section class="hero">
      <div class="hero__bg" />
      <div class="hero__mask" />
      <div class="hero__content">
        <h1 class="hero__title">冰柏叶小说库</h1>
        <p class="hero__subtitle">在字里行间，遇见另一个世界</p>

        <div class="hero__search">
          <el-input
            v-model="keyword"
            size="large"
            placeholder="搜索书名 / 作者，例如：转生、无职"
            clearable
            @keyup.enter="search"
          >
            <template #suffix>
              <el-icon class="hero__search-icon" @click="search"><Search /></el-icon>
            </template>
          </el-input>
          <el-button type="primary" size="large" @click="search">搜索</el-button>
        </div>

        <div class="hero__links">
          <el-button link @click="router.push('/novels')">全部小说</el-button>
          <el-divider direction="vertical" />
          <el-button link @click="router.push('/categories')">按分类浏览</el-button>
          <el-divider direction="vertical" />
          <el-button link @click="router.push('/my-novels')">我的小说</el-button>
        </div>
      </div>
    </section>

    <!-- 主题图下面一行：循环播放最新一条系统广播 -->
    <NoticeMarquee class="home-notice" />

    <div class="page-padding">
      <!-- 我的收藏（登录后显示前 6 本） -->
      <template v-if="auth.isLogin">
        <div class="section-header">
          <div class="section-title">我的收藏</div>
          <el-button link type="primary" @click="router.push('/bookshelf')">
            全部收藏 →
          </el-button>
        </div>

        <div v-if="loading.favorites" class="fav-row">
          <el-skeleton v-for="i in 6" :key="i" animated>
            <template #template>
              <el-skeleton-item variant="image" style="width: 100%; aspect-ratio: 3 / 4" />
              <el-skeleton-item variant="text" style="margin-top: 8px" />
            </template>
          </el-skeleton>
        </div>
        <el-empty
          v-else-if="favorites.length === 0"
          description="还没有收藏，去书城逛逛吧"
          :image-size="70"
        />
        <div v-else class="fav-row">
          <div
            v-for="item in favorites"
            :key="item.favoriteId"
            class="fav-item"
            @click="openFavorite(item)"
          >
            <div class="fav-item__cover">
              <img
                :src="item.coverUrl ? resolveImageUrl(item.coverUrl) : '/images/no-cover.jpg'"
                :alt="item.novelName"
              />
            </div>
            <div class="fav-item__title text-ellipsis">{{ item.novelName }}</div>
            <div class="fav-item__author text-ellipsis">{{ item.author || '佚名' }}</div>
          </div>
        </div>
      </template>


      <!-- 热门点击（带左上角排序小标） -->
      <div class="section-header">
        <div class="section-title">热门点击</div>
        <el-button link type="primary" @click="router.push('/novels?sort=click')">
          更多 →
        </el-button>
      </div>

      <el-skeleton v-if="loading.hottest" :rows="3" animated />
      <el-empty v-else-if="hottest.length === 0" description="暂时还没有数据" />
      <div v-else class="card-grid">
        <NovelCard
          v-for="(novel, index) in hottest"
          :key="novel.novelId"
          :novel="novel"
          :rank="index + 1"
        />
      </div>

      <!-- 最新更新 -->
      <div class="section-header">
        <div class="section-title">最新更新</div>
        <el-button link type="primary" @click="router.push('/novels?sort=update')">
          更多 →
        </el-button>
      </div>

      <el-skeleton v-if="loading.latest" :rows="4" animated />
      <el-empty v-else-if="latest.length === 0" description="暂时还没有小说" />
      <div v-else class="card-grid">
        <NovelCard v-for="novel in latest" :key="novel.novelId" :novel="novel" />
      </div>
    </div>
  </div>
</template>

<style scoped>
.home-notice {
  max-width: 1240px;
  margin: 12px auto 0;
}

.hero {
  position: relative;
  max-width: 1240px;
  height: 340px;
  margin: 20px auto 0;
  border-radius: 20px;
  overflow: hidden;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 18px 44px rgba(15, 26, 36, 0.22);
}

.hero__bg {
  position: absolute;
  inset: 0;
  background-image: url('/images/hero.jpg');
  background-size: cover;
  background-position: center;
  transform: scale(1.01);
}

.hero__mask {
  position: absolute;
  inset: 0;
  background: linear-gradient(
    180deg,
    rgba(8, 12, 18, 0.5) 0%,
    rgba(8, 12, 18, 0.28) 40%,
    rgba(8, 12, 18, 0.62) 100%
  );
}

.hero__content {
  position: relative;
  z-index: 1;
  width: min(720px, 88%);
  text-align: center;
  color: #fff;
}


.hero__title {
  margin: 0 0 10px;
  font-size: 46px;
  font-weight: 800;
  line-height: 1.2;
  letter-spacing: 6px;
  text-shadow: 0 6px 26px rgba(0, 0, 0, 0.6);
}

.hero__subtitle {
  margin: 0 0 22px;
  font-size: 13px;
  line-height: 1.6;
  letter-spacing: 2px;
  color: rgba(255, 255, 255, 0.78);
  text-shadow: 0 2px 12px rgba(0, 0, 0, 0.55);
}

.hero__search {
  display: flex;
  gap: 10px;
  padding: 8px;
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.94);
  box-shadow: 0 14px 34px rgba(0, 0, 0, 0.3);
}

.hero__search :deep(.el-input__wrapper) {
  box-shadow: none;
  background: transparent;
}

.hero__search-icon {
  cursor: pointer;
  color: #8a94a0;
}

.hero__links {
  margin-top: 16px;
}

.hero__links :deep(.el-button) {
  color: rgba(255, 255, 255, 0.88);
  text-shadow: 0 2px 10px rgba(0, 0, 0, 0.5);
}

.hero__links :deep(.el-divider--vertical) {
  border-color: rgba(255, 255, 255, 0.35);
}

@media (max-width: 900px) {
  .hero {
    height: 260px;
  }

  .hero__title {
    font-size: 30px;
    letter-spacing: 3px;
  }
}
/* 收藏小卡 */
.fav-row {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(110px, 1fr));
  gap: 16px;
  min-height: 60px;
}

.fav-item {
  cursor: pointer;
  text-align: center;
  transition: transform 0.2s ease;
}

.fav-item:hover {
  transform: translateY(-4px);
}

.fav-item__cover {
  aspect-ratio: 3 / 4;
  border-radius: 10px;
  overflow: hidden;
  background: #eef3f7;
  box-shadow: 0 6px 16px rgba(31, 45, 61, 0.12);
}

.fav-item__cover img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.fav-item__title {
  margin-top: 8px;
  font-size: 13px;
  font-weight: 600;
}

.fav-item__author {
  font-size: 12px;
  color: #909399;
}

@media (max-width: 1100px) {
  .hero__inner {
    grid-template-columns: 1fr;
  }

  .hero__right {
    min-height: 220px;
  }
}
</style>
