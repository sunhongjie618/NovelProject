<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { commentApi } from '@/api'
import { useAuthStore } from '@/stores/auth'
import type { CommentItem } from '@/types/api'
import { fromNow, resolveImageUrl } from '@/utils/format'

const props = defineProps<{
  novelId: number
  chapterId?: number | null
}>()

const router = useRouter()
const auth = useAuthStore()

const comments = ref<CommentItem[]>([])
const total = ref(0)
const page = ref(1)
const pageSize = 10
const loading = ref(false)
const submitting = ref(false)
const content = ref('')

const isEmpty = computed(() => !loading.value && comments.value.length === 0)

async function load() {
  loading.value = true
  try {
    const result = await commentApi.list({
      novelId: props.novelId,
      chapterId: props.chapterId ?? undefined,
      page: page.value,
      pageSize,
      sortBy: true,
    })
    comments.value = result.data || []
    total.value = result.total || 0
  } catch {
    comments.value = []
    total.value = 0
  } finally {
    loading.value = false
  }
}

async function submit() {
  const text = content.value.trim()
  if (!text) {
    ElMessage.warning('评论内容不能为空')
    return
  }
  if (!auth.isLogin) {
    ElMessage.warning('登录后才能评论')
    router.push({ path: '/', query: { login: '1', redirect: router.currentRoute.value.fullPath } })
    return
  }
  submitting.value = true
  try {
    await commentApi.create({
      novelId: props.novelId,
      chapterId: props.chapterId ?? null,
      content: text,
    })
    content.value = ''
    page.value = 1
    await load()
    ElMessage.success('评论成功')
  } finally {
    submitting.value = false
  }
}

async function toggleLike(item: CommentItem) {
  if (!auth.isLogin) {
    ElMessage.warning('登录后才能点赞')
    router.push({ path: '/', query: { login: '1', redirect: router.currentRoute.value.fullPath } })
    return
  }
  const result = await commentApi.toggleLike(item.commentId)
  item.isLiked = result.isLike
  item.likeCount = result.likeCount
}

function handlePageChange(value: number) {
  page.value = value
  load()
}

onMounted(load)
</script>

<template>
  <div class="comment-section">
    <div class="section-header">
      <div class="section-title" style="margin: 0 0 12px">评论（{{ total }}）</div>
      <el-button type="primary" size="small" @click="submit" :loading="submitting">
        发表评论
      </el-button>
    </div>

    <!-- 发表框 -->
    <div class="comment-editor">
      <el-input
        v-model="content"
        type="textarea"
        :rows="3"
        maxlength="500"
        show-word-limit
        placeholder="友善发言，理性讨论～（当前评论只有一级，不支持回复别人的评论）"
      />
      <div class="comment-editor__footer">
        <span class="comment-editor__tip">
          当前版本评论为一级评论，暂不支持楼中楼回复
        </span>
        <el-button type="primary" :loading="submitting" @click="submit">发布</el-button>
      </div>
    </div>

    <!-- 列表 -->
    <div v-loading="loading" class="comment-list">
      <el-empty v-if="isEmpty" description="还没有评论，来抢占第一个沙发吧" />

      <div v-for="item in comments" :key="item.commentId" class="comment-item">
        <el-avatar :size="36" :src="resolveImageUrl(item.userAvatarUrl)">
          {{ (item.userNickName || '游').slice(0, 1) }}
        </el-avatar>

        <div class="comment-item__main">
          <div class="comment-item__head">
            <span class="comment-item__name">
              {{ item.userNickName || '游客' }}
            </span>
            <span class="comment-item__time">{{ fromNow(item.createTime) }}</span>
          </div>
          <div class="comment-item__content">{{ item.content }}</div>
          <div class="comment-item__actions">
            <el-button
              link
              size="small"
              :type="item.isLiked ? 'primary' : 'default'"
              @click="toggleLike(item)"
            >
              👍 {{ item.likeCount || 0 }}
            </el-button>
          </div>
        </div>
      </div>
    </div>

    <div v-if="total > pageSize" class="comment-pager">
      <el-pagination
        layout="prev, pager, next"
        :total="total"
        :page-size="pageSize"
        :current-page="page"
        @current-change="handlePageChange"
      />
    </div>
  </div>
</template>

<style scoped>
.comment-editor {
  background: rgba(255, 255, 255, 0.72);
  border: 1px solid rgba(255, 255, 255, 0.8);
  border-radius: 12px;
  padding: 12px;
  margin-bottom: 18px;
}

.comment-editor__footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-top: 10px;
}

.comment-editor__tip {
  font-size: 12px;
  color: #a0a8b0;
}

.comment-list {
  min-height: 80px;
}

.comment-item {
  display: flex;
  gap: 12px;
  padding: 14px 0;
  border-bottom: 1px solid #f0f2f5;
}

.comment-item:last-child {
  border-bottom: none;
}

.comment-item__main {
  flex: 1;
  min-width: 0;
}

.comment-item__head {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 6px;
}

.comment-item__name {
  font-weight: 600;
  font-size: 13px;
  color: #1f2d3d;
}

.comment-item__time {
  font-size: 12px;
  color: #a0a8b0;
}

.comment-item__content {
  font-size: 14px;
  line-height: 1.7;
  color: #303133;
  white-space: pre-wrap;
  word-break: break-word;
}

.comment-item__actions {
  margin-top: 6px;
}

.comment-pager {
  display: flex;
  justify-content: center;
  margin-top: 16px;
}
</style>
