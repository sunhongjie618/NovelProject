<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { Plus, Upload } from '@element-plus/icons-vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { UploadRequestOptions } from 'element-plus'
import { categoryApi, chapterApi, fileApi, novelApi } from '@/api'
import type { CategoryItem, NovelListItem } from '@/types/api'
import { formatWordCount, resolveImageUrl, statusText } from '@/utils/format'

/**
 * 我的小说：管理自己上传的作品（新建 / 加章节 / 删书）。
 * 后端没有 /api/Novels/mine，只能拉列表后按作者名（AddNovel 写入的 NickName claim）过滤。
 * 章节只支持「一章一章」上传，批量导入 TXT 不在这条链路上暴露。
 */
const router = useRouter()


const loading = ref(false)
const novels = ref<NovelListItem[]>([])
const categories = ref<CategoryItem[]>([])

// ---------------- 新建小说 ----------------
const createVisible = ref(false)
const creating = ref(false)
const createForm = reactive({
  title: '',
  categoryId: undefined as number | undefined,
  coverUrl: '',
  description: '',
})

// ---------------- 添加章节 ----------------
const chapterVisible = ref(false)
const chapterSaving = ref(false)
const currentNovel = ref<NovelListItem | null>(null)
const chapterForm = reactive({
  chapterNumber: 1,
  title: '',
  content: '',
  isLocked: false,
})

const page = ref(1)
const pageSize = 12
const total = ref(0)

async function loadMine() {
  loading.value = true
  try {
    // GET /api/Novels/mine → 服务端分页，只返回自己的作品，不再全量扫描 + 按昵称过滤
    const res = await novelApi.mine({ page: page.value, pagesize: pageSize })
    novels.value = res.data || []
    total.value = res.total || 0
  } catch {
    /* 拦截器已统一提示 */
  } finally {
    loading.value = false
  }
}

function changePage(value: number) {
  page.value = value
  loadMine()
}
function coverOf(item: NovelListItem) {
  return resolveImageUrl(item.coverUrl) || '/images/no-cover.jpg'
}

function openCreate() {
  createForm.title = ''
  createForm.categoryId = undefined
  createForm.coverUrl = ''
  createForm.description = ''
  createVisible.value = true
}

/** 封面上传：/api/Files/upload-cover，字段名 file，≤2MB */
async function uploadCover(options: UploadRequestOptions) {
  try {
    const res = await fileApi.uploadCover(options.file as File)
    createForm.coverUrl = res.fileUrl
    ElMessage.success('封面上传成功')
  } catch {
    /* 拦截器已统一提示 */
  }
  return false
}

// ---------------- 编辑小说 ----------------
const editVisible = ref(false)
const editSaving = ref(false)
const editId = ref(0)
const editForm = reactive({
  title: '',
  categoryId: undefined as number | undefined,
  status: 1,
  description: '',
  coverUrl: '',
})

async function openEdit(item: NovelListItem) {
  editId.value = item.novelId
  editForm.title = item.title
  editForm.categoryId = undefined
  editForm.status = item.status || 1
  editForm.description = item.description || ''
  editForm.coverUrl = item.coverUrl || ''
  editVisible.value = true
  // 列表 DTO 不带 categoryId，取详情补齐（顺带拿到最新内容）
  try {
    const detail = await novelApi.detail(item.novelId)
    editForm.title = detail.title || editForm.title
    editForm.categoryId = detail.categoryId
    editForm.description = detail.description || ''
    editForm.coverUrl = detail.coverUrl || ''
    editForm.status = detail.status || editForm.status
  } catch {
    /* 拦截器已统一提示 */
  }
}

/** 编辑弹框里的封面上传 */
async function uploadEditCover(options: UploadRequestOptions) {
  try {
    const res = await fileApi.uploadCover(options.file as File)
    editForm.coverUrl = res.fileUrl
    ElMessage.success('封面上传成功')
  } catch {
    /* 拦截器已统一提示 */
  }
  return false
}

async function submitEdit() {
  if (!editForm.title.trim()) return ElMessage.warning('书名不能为空')
  editSaving.value = true
  try {
    // PUT /api/Novels/{novelId}，后端只更新非空字段，且校验作者本人
    await novelApi.update(editId.value, {
      title: editForm.title.trim(),
      categoryId: editForm.categoryId,
      description: editForm.description.trim() || undefined,
      coverUrl: editForm.coverUrl || undefined,
      status: editForm.status,
    })
    ElMessage.success('已保存')
    editVisible.value = false
    await loadMine()
  } catch {
    /* 拦截器已统一提示 */
  } finally {
    editSaving.value = false
  }
}

async function submitCreate() {
  if (!createForm.title.trim()) return ElMessage.warning('请填写书名')
  if (!createForm.categoryId) return ElMessage.warning('请选择分类')
  creating.value = true
  try {
    await novelApi.create({
      categoryId: createForm.categoryId,
      title: createForm.title.trim(),
      coverUrl: createForm.coverUrl,
      description: createForm.description.trim(),
    })
    ElMessage.success('创建成功')
    createVisible.value = false
    await loadMine()
  } catch {
    /* 拦截器已统一提示 */
  } finally {
    creating.value = false
  }
}

async function openChapter(item: NovelListItem) {
  currentNovel.value = item
  chapterForm.title = ''
  chapterForm.content = ''
  chapterForm.isLocked = false
  chapterForm.chapterNumber = 1
  try {
    const res = await chapterApi.list({ novelId: item.novelId, page: 1, pagesize: 1, isDes: true })
    chapterForm.chapterNumber = (res.total || 0) + 1
  } catch {
    /* 忽略：默认第 1 章 */
  }
  chapterVisible.value = true
}

async function submitChapter() {
  if (!currentNovel.value) return
  if (!chapterForm.title.trim()) return ElMessage.warning('请填写章节标题')
  if (!chapterForm.content.trim()) return ElMessage.warning('请填写章节正文')
  chapterSaving.value = true
  try {
    await chapterApi.create({
      novelId: currentNovel.value.novelId,
      title: chapterForm.title.trim(),
      content: chapterForm.content,
      isLocked: chapterForm.isLocked,
      chapterNumber: chapterForm.chapterNumber,
    })
    ElMessage.success(`第 ${chapterForm.chapterNumber} 章已发布`)
    chapterVisible.value = false
    await loadMine()
  } catch {
    /* 拦截器已统一提示 */
  } finally {
    chapterSaving.value = false
  }
}

async function removeNovel(item: NovelListItem) {
  try {
    await ElMessageBox.confirm(`确定删除《${item.title}》吗？章节也会一并删除。`, '删除小说', {
      type: 'warning',
      confirmButtonText: '删除',
      cancelButtonText: '取消',
    })
  } catch {
    return
  }
  try {
    await novelApi.remove(item.novelId)
    ElMessage.success('已删除')
    await loadMine()
  } catch {
    /* 拦截器已统一提示 */
  }
}

onMounted(async () => {
  try {
    const res = await categoryApi.list({ isEnable: true, orderBySortAsc: true })
    categories.value = res || []
  } catch {
    /* 忽略 */
  }
  await loadMine()
})
</script>

<template>
  <div class="page-padding">
    <div class="mn-head">
      <div>
        <h2 class="mn-head__title">我的小说</h2>
        <p class="mn-head__sub">
          共 {{ total }} 本 · 章节一次传一章，新建作品后即可添加
        </p>
      </div>
      <el-button type="primary" :icon="Plus" @click="openCreate">新建小说</el-button>
    </div>

    <el-skeleton v-if="loading" :rows="5" animated />

    <el-empty v-else-if="novels.length === 0" description="你还没有上传过小说">
      <el-button type="primary" @click="openCreate">创建第一本</el-button>
    </el-empty>

    <div v-else class="mn-list">
      <div v-for="item in novels" :key="item.novelId" class="mn-item">
        <img class="mn-item__cover" :src="coverOf(item)" :alt="item.title" loading="lazy" />

        <div class="mn-item__body">
          <div class="mn-item__title">{{ item.title }}</div>
          <div class="mn-item__meta">
            <span>{{ item.categoryName || '未分类' }}</span>
            <span>·</span>
            <span>{{ statusText(item.status) }}</span>
            <span>·</span>
            <span>{{ formatWordCount(item.wordCount) }}</span>
            <span>·</span>
            <span>点击 {{ item.clickCount }} / 收藏 {{ item.favoriteCount }}</span>
          </div>
          <div class="mn-item__desc">{{ item.description || '还没有简介' }}</div>

          <div class="mn-item__actions">
            <el-button type="primary" size="small" plain @click="openChapter(item)">
              添加章节
            </el-button>
            <el-button size="small" plain @click="openEdit(item)">编辑</el-button>
            <el-button size="small" @click="router.push(`/novels/${item.novelId}`)">查看</el-button>
            <el-button type="danger" size="small" plain @click="removeNovel(item)">删除</el-button>
          </div>
        </div>
      </div>
    </div>

    <!-- 编辑小说 -->
    <el-dialog v-model="editVisible" title="编辑小说" width="520px" append-to-body>
      <el-form label-width="72px" label-position="right">
        <el-form-item label="书名" required>
          <el-input v-model="editForm.title" maxlength="60" show-word-limit />
        </el-form-item>

        <el-form-item label="分类">
          <el-select v-model="editForm.categoryId" placeholder="不修改分类" clearable style="width: 100%">
            <el-option
              v-for="c in categories"
              :key="c.categoryId"
              :label="c.categoryName"
              :value="c.categoryId"
            />
          </el-select>
        </el-form-item>

        <el-form-item label="状态">
          <el-radio-group v-model="editForm.status">
            <el-radio-button :value="1">连载中</el-radio-button>
            <el-radio-button :value="2">已完结</el-radio-button>
            <el-radio-button :value="3">已下架</el-radio-button>
          </el-radio-group>
        </el-form-item>

        <el-form-item label="封面">
          <div class="cover-upload">
            <div class="cover-upload__preview">
              <img v-if="editForm.coverUrl" :src="resolveImageUrl(editForm.coverUrl)" alt="封面" />
              <span v-else>暂无封面</span>
            </div>
            <div class="cover-upload__side">
              <el-upload
                :show-file-list="false"
                accept="image/png,image/jpeg,image/gif"
                :http-request="uploadEditCover"
              >
                <el-button :icon="Upload" size="small">更换封面</el-button>
              </el-upload>
              <span class="cover-upload__tip">jpg / jpeg / png / gif，≤ 2MB</span>
            </div>
          </div>
        </el-form-item>

        <el-form-item label="简介">
          <el-input
            v-model="editForm.description"
            type="textarea"
            :rows="4"
            maxlength="1000"
            show-word-limit
            placeholder="一句话介绍这本书…"
          />
          <span class="cover-upload__tip">后端只更新非空字段：留空不会清空已有简介</span>
        </el-form-item>
      </el-form>

      <template #footer>
        <el-button @click="editVisible = false">取消</el-button>
        <el-button type="primary" :loading="editSaving" @click="submitEdit">保存修改</el-button>
      </template>
    </el-dialog>

    <!-- 新建小说 -->
    <el-dialog v-model="createVisible" title="新建小说" width="520px" append-to-body>
      <el-form label-width="72px" label-position="right">
        <el-form-item label="书名" required>
          <el-input v-model="createForm.title" maxlength="60" show-word-limit placeholder="例如：转生的大圣女" />
        </el-form-item>

        <el-form-item label="分类" required>
          <el-select v-model="createForm.categoryId" placeholder="请选择分类" style="width: 100%">
            <el-option
              v-for="c in categories"
              :key="c.categoryId"
              :label="c.categoryName"
              :value="c.categoryId"
            />
          </el-select>
        </el-form-item>

        <el-form-item label="封面">
          <div class="cover-upload">
            <div class="cover-upload__preview">
              <img v-if="createForm.coverUrl" :src="resolveImageUrl(createForm.coverUrl)" alt="封面" />
              <span v-else>暂无封面</span>
            </div>
            <div class="cover-upload__side">
              <el-upload
                :show-file-list="false"
                :http-request="uploadCover"
                accept="image/jpeg,image/png,image/gif"
              >
                <el-button :icon="Upload" size="small">上传封面</el-button>
              </el-upload>
              <p class="cover-upload__tip">jpg / png / gif，≤ 2MB</p>
            </div>
          </div>
        </el-form-item>

        <el-form-item label="简介">
          <el-input
            v-model="createForm.description"
            type="textarea"
            :rows="4"
            maxlength="1000"
            show-word-limit
            placeholder="一句话介绍这本书…"
          />
        </el-form-item>
      </el-form>

      <template #footer>
        <el-button @click="createVisible = false">取消</el-button>
        <el-button type="primary" :loading="creating" @click="submitCreate">创建小说</el-button>
      </template>
    </el-dialog>

    <!-- 添加章节（单章） -->
    <el-dialog
      v-model="chapterVisible"
      :title="currentNovel ? `添加章节 · ${currentNovel.title}` : '添加章节'"
      width="640px"
      append-to-body
    >
      <el-form label-width="72px">
        <el-form-item label="章节序号">
          <el-input-number v-model="chapterForm.chapterNumber" :min="1" style="width: 140px" />
          <span class="chapter-tip">默认接在最后一章之后</span>
        </el-form-item>

        <el-form-item label="标题" required>
          <el-input v-model="chapterForm.title" maxlength="80" placeholder="例如：第一章 雪夜来客" />
        </el-form-item>

        <el-form-item label="正文" required>
          <el-input
            v-model="chapterForm.content"
            type="textarea"
            :rows="12"
            placeholder="粘贴或输入本章正文…"
          />
        </el-form-item>

        <el-form-item label="锁定">
          <el-switch v-model="chapterForm.isLocked" />
          <span class="chapter-tip">锁定后需要解锁才能阅读</span>
        </el-form-item>
      </el-form>

      <template #footer>
        <el-button @click="chapterVisible = false">取消</el-button>
        <el-button type="primary" :loading="chapterSaving" @click="submitChapter">发布章节</el-button>
      </template>
    </el-dialog>
    <div v-if="total > pageSize" class="mn-pager">
      <el-pagination
        :current-page="page"
        :page-size="pageSize"
        :total="total"
        layout="prev, pager, next, jumper"
        background
        @current-change="changePage"
      />
    </div>  </div>
</template>

<style scoped>
.mn-pager {
  display: flex;
  justify-content: center;
  margin-top: 24px;
}

.mn-head {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
  margin-bottom: 18px;
}

.mn-head__title {
  margin: 0 0 6px;
  font-size: 26px;
  font-weight: 800;
  color: #1f2d3d;
  letter-spacing: 1px;
}

.mn-head__warn {
  color: #e6a23c;
}

.mn-head__tip {
  color: #98a2b3;
  margin-left: 8px;
}

.mn-head__sub {
  margin: 0;
  font-size: 12px;
  color: #8a97a5;
}

.mn-list {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.mn-item {
  display: flex;
  gap: 18px;
  padding: 16px;
  background: #fff;
  border-radius: 14px;
  box-shadow: 0 2px 14px rgba(23, 43, 77, 0.05);
}

.mn-item__cover {
  width: 92px;
  height: 123px;
  border-radius: 8px;
  object-fit: cover;
  flex-shrink: 0;
  background: #eef2f5;
}

.mn-item__body {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
}

.mn-item__title {
  font-size: 16px;
  font-weight: 700;
  color: #1f2d3d;
}

.mn-item__meta {
  margin-top: 7px;
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 12.5px;
  color: #8a97a5;
}

.mn-item__desc {
  margin-top: 8px;
  font-size: 13px;
  line-height: 1.6;
  color: #62707c;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.mn-item__actions {
  margin-top: auto;
  padding-top: 12px;
  display: flex;
  gap: 8px;
}

.cover-upload {
  display: flex;
  gap: 14px;
}

.cover-upload__preview {
  width: 92px;
  height: 123px;
  border-radius: 8px;
  background: #f2f5f8;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  color: #a8b3bd;
  overflow: hidden;
}

.cover-upload__preview img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.cover-upload__side {
  display: flex;
  flex-direction: column;
  gap: 6px;
  justify-content: center;
}

.cover-upload__tip {
  margin: 0;
  font-size: 12px;
  color: #9aa6b2;
}

.chapter-tip {
  margin-left: 10px;
  font-size: 12px;
  color: #9aa6b2;
}
</style>
