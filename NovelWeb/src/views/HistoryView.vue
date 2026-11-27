<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Delete } from '@element-plus/icons-vue'
import { readingRecordApi } from '@/api'
import type { ReadingRecordItem } from '@/types/api'
import { formatDateTime } from '@/utils/format'

const router = useRouter()

const records = ref<ReadingRecordItem[]>([])
const total = ref(0)
const page = ref(1)
const pageSize = 20
const loading = ref(false)

/** 按「年-月」分组，参考图里的时间轴效果 */
const grouped = computed(() => {
  const map = new Map<string, ReadingRecordItem[]>()
  records.value.forEach((item) => {
    const date = new Date(item.lastReadTime)
    const key = Number.isNaN(date.getTime())
      ? '未知时间'
      : `${date.getFullYear()} 年 ${date.getMonth() + 1} 月`
    if (!map.has(key)) map.set(key, [])
    map.get(key)!.push(item)
  })
  return Array.from(map.entries()).map(([label, list]) => ({ label, list }))
})

async function load() {
  loading.value = true
  try {
    const result = await readingRecordApi.list({ page: page.value, pageSize })
    records.value = result.data || []
    total.value = result.total || 0
  } finally {
    loading.value = false
  }
}

function open(item: ReadingRecordItem) {
  if (item.lastReadChapterId) {
    router.push({
      path: `/read/${item.lastReadChapterId}`,
      query: { novelId: String(item.novelId) },
    })
  } else {
    router.push(`/novels/${item.novelId}`)
  }
}

function handlePageChange(value: number) {
  page.value = value
  load()
}

/** 正在删除的记录 id，用来只让那一行转圈 */
const deletingId = ref<number | null>(null)

async function remove(item: ReadingRecordItem) {
  try {
    await ElMessageBox.confirm(
      `确定删除《${item.title}》这条阅读记录吗？删除后不影响书籍本身。`,
      '删除阅读记录',
      { type: 'warning', confirmButtonText: '删除', cancelButtonText: '取消' },
    )
  } catch {
    return // 用户点了取消
  }

  deletingId.value = item.recordId
  try {
    await readingRecordApi.remove(item.recordId)
    ElMessage.success('已删除')

    records.value = records.value.filter((r) => r.recordId !== item.recordId)
    total.value = Math.max(0, total.value - 1)

    // 当前页被删空且不是第一页 → 回退一页重新拉，避免停在空白页
    if (records.value.length === 0 && page.value > 1) {
      page.value -= 1
      await load()
    }
  } catch {
    // 404（记录不存在或不是你的）由 axios 拦截器统一提示，这里不再重复弹
  } finally {
    deletingId.value = null
  }
}

onMounted(load)
</script>

<template>
  <div class="page-padding">
    <div class="section-header">
      <div class="section-title" style="margin: 0 0 8px">阅读历史</div>
      <span class="count">共 {{ total }} 条 · 记录由后端在打开章节时自动写入</span>
    </div>

    <p class="tip">
      记录由后端在打开章节时自动写入；鼠标移到某一行可以单独删除，后台清理服务也会按时间自动清理旧记录。
    </p>

    <div v-loading="loading" class="history">
      <el-empty v-if="!loading && records.length === 0" description="还没有阅读记录">
        <el-button type="primary" @click="router.push('/novels')">去书城看看</el-button>
      </el-empty>

      <div v-for="group in grouped" :key="group.label" class="group">
        <div class="group__label">{{ group.label }}</div>

        <div class="group__list">
          <div
            v-for="item in group.list"
            :key="item.recordId"
            class="record"
            @click="open(item)"
          >
            <span class="record__dot" />
            <div class="record__main">
              <div class="record__title text-ellipsis">{{ item.title }}</div>
              <div class="record__meta">
                <span>{{ item.authorName || '佚名' }}</span>
                <span>阅读于 {{ formatDateTime(item.lastReadTime) }}</span>
                <span v-if="!item.lastReadChapterId" class="record__warn">未记录章节</span>
              </div>
            </div>
            <span class="record__action">继续阅读 →</span>
            <el-button
              class="record__del"
              link
              type="danger"
              size="small"
              :icon="Delete"
              :loading="deletingId === item.recordId"
              @click.stop="remove(item)"
            >
              删除
            </el-button>
          </div>
        </div>
      </div>
    </div>

    <div v-if="total > pageSize" class="pager">
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
.count {
  font-size: 13px;
  color: #909399;
}

.tip {
  margin: -4px 0 20px;
  padding: 10px 14px;
  border-radius: 10px;
  background: #fdf6ec;
  color: #b88230;
  font-size: 12px;
  line-height: 1.7;
}

.history {
  min-height: 160px;
}

.group {
  margin-bottom: 26px;
}

.group__label {
  font-size: 13px;
  font-weight: 600;
  color: #a0a8b0;
  margin-bottom: 12px;
  padding-left: 4px;
}

.group__list {
  border-left: 2px solid #eef1f4;
  padding-left: 18px;
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.record {
  position: relative;
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 12px 16px;
  border-radius: 12px;
  cursor: pointer;
  transition: background 0.18s ease;
}

.record:hover {
  background: rgba(255, 255, 255, 0.85);
}

.record__dot {
  position: absolute;
  left: -25px;
  top: 50%;
  transform: translateY(-50%);
  width: 9px;
  height: 9px;
  border-radius: 50%;
  background: var(--app-green);
  box-shadow: 0 0 0 3px rgba(24, 160, 88, 0.15);
}

.record__main {
  flex: 1;
  min-width: 0;
}

.record__title {
  font-size: 15px;
  font-weight: 600;
  color: #1f2d3d;
}

.record__meta {
  margin-top: 4px;
  display: flex;
  flex-wrap: wrap;
  gap: 14px;
  font-size: 12px;
  color: #909399;
}

.record__warn {
  color: #e6a23c;
}

.record__action {
  flex: 0 0 auto;
  font-size: 12px;
  color: var(--app-green);
  opacity: 0;
  transition: opacity 0.18s ease;
}

.record:hover .record__action {
  opacity: 1;
}

.record__del {
  flex: 0 0 auto;
  opacity: 0;
  transition: opacity 0.18s ease;
}

.record:hover .record__del,
.record__del.is-loading {
  opacity: 1;
}

.pager {
  display: flex;
  justify-content: center;
  margin-top: 10px;
}
</style>
