<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import { ElMessage } from 'element-plus'
import { useAuthStore } from '@/stores/auth'

const props = defineProps<{
  modelValue: boolean
  /** 打开时默认展示哪个页签 */
  mode?: 'login' | 'register'
}>()

const emit = defineEmits<{
  (e: 'update:modelValue', value: boolean): void
  (e: 'success'): void
}>()

const auth = useAuthStore()

const visible = computed({
  get: () => props.modelValue,
  set: (value: boolean) => emit('update:modelValue', value),
})

const activeTab = ref<'login' | 'register'>('login')
const loading = ref(false)

const loginForm = reactive({ userName: '', password: '' })
const registerForm = reactive({ userName: '', nickName: '', password: '', confirm: '' })

watch(
  () => props.modelValue,
  (open) => {
    if (open) {
      activeTab.value = props.mode || 'login'
      loginForm.password = ''
      registerForm.password = ''
      registerForm.confirm = ''
      document.body.style.overflow = 'hidden'
    } else {
      document.body.style.overflow = ''
    }
  },
)

function close() {
  visible.value = false
}

async function doLogin() {
  if (!loginForm.userName.trim() || !loginForm.password) {
    ElMessage.warning('请输入用户名和密码')
    return
  }
  loading.value = true
  try {
    await auth.login({ userName: loginForm.userName.trim(), password: loginForm.password })
    ElMessage.success(`欢迎回来，${auth.nickName}`)
    close()
    emit('success')
  } catch {
    // 错误提示已由 axios 拦截器统一处理
  } finally {
    loading.value = false
  }
}

async function doRegister() {
  if (!registerForm.userName.trim()) {
    ElMessage.warning('请输入用户名')
    return
  }
  if (registerForm.password.length < 6) {
    ElMessage.warning('密码至少 6 位（后端 Identity 密码策略）')
    return
  }
  if (registerForm.password !== registerForm.confirm) {
    ElMessage.warning('两次输入的密码不一致')
    return
  }
  loading.value = true
  try {
    await auth.register({
      userName: registerForm.userName.trim(),
      password: registerForm.password,
      nickName: registerForm.nickName.trim() || registerForm.userName.trim(),
    })
    ElMessage.success('注册成功，已帮你自动登录')
    await auth.login({
      userName: registerForm.userName.trim(),
      password: registerForm.password,
    })
    close()
    emit('success')
  } catch {
    // 同上
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <Teleport to="body">
    <Transition name="auth-fade">
      <div v-if="visible" class="auth-mask" @click.self="close">
        <div class="auth-panel" role="dialog" aria-modal="true">
          <button class="auth-panel__close" type="button" aria-label="关闭" @click="close">×</button>

          <div class="auth-panel__header">
            <img src="/images/leaf.svg" alt="冰柏叶小说库" class="auth-panel__logo" />
            <div>
              <div class="auth-panel__brand">冰柏叶小说库</div>
              <div class="auth-panel__slogan">登录后可以收藏、评论、上传小说与私信</div>
            </div>
          </div>

          <div class="auth-tabs">
            <button
              type="button"
              class="auth-tabs__item"
              :class="{ 'is-active': activeTab === 'login' }"
              @click="activeTab = 'login'"
            >
              登录
            </button>
            <button
              type="button"
              class="auth-tabs__item"
              :class="{ 'is-active': activeTab === 'register' }"
              @click="activeTab = 'register'"
            >
              注册
            </button>
          </div>

          <!-- 登录 -->
          <div v-if="activeTab === 'login'" class="auth-form">
            <label class="auth-field">
              <span class="auth-field__label">用户名</span>
              <el-input
                v-model="loginForm.userName"
                size="large"
                placeholder="请输入用户名"
                @keyup.enter="doLogin"
              />
            </label>
            <label class="auth-field">
              <span class="auth-field__label">密码</span>
              <el-input
                v-model="loginForm.password"
                type="password"
                size="large"
                show-password
                placeholder="至少 6 位"
                @keyup.enter="doLogin"
              />
            </label>
            <el-button
              type="primary"
              size="large"
              class="auth-submit"
              :loading="loading"
              @click="doLogin"
            >
              登录
            </el-button>
            <div class="auth-switch">
              <span>还没有账号？</span>
              <button type="button" class="auth-switch__btn" @click="activeTab = 'register'">
                立即注册
              </button>
            </div>
          </div>

          <!-- 注册 -->
          <div v-else class="auth-form">
            <label class="auth-field">
              <span class="auth-field__label">用户名</span>
              <el-input
                v-model="registerForm.userName"
                size="large"
                placeholder="登录用，注册后不可改"
              />
            </label>
            <label class="auth-field">
              <span class="auth-field__label">昵称（可选）</span>
              <el-input v-model="registerForm.nickName" size="large" placeholder="不填则默认用用户名" />
            </label>
            <label class="auth-field">
              <span class="auth-field__label">密码</span>
              <el-input
                v-model="registerForm.password"
                type="password"
                size="large"
                show-password
                placeholder="至少 6 位"
              />
            </label>
            <label class="auth-field">
              <span class="auth-field__label">确认密码</span>
              <el-input
                v-model="registerForm.confirm"
                type="password"
                size="large"
                show-password
                placeholder="再输入一次"
                @keyup.enter="doRegister"
              />
            </label>
            <el-button
              type="primary"
              size="large"
              class="auth-submit"
              :loading="loading"
              @click="doRegister"
            >
              注册并登录
            </el-button>
            <div class="auth-switch">
              <span>已有账号？</span>
              <button type="button" class="auth-switch__btn" @click="activeTab = 'login'">
                去登录
              </button>
            </div>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
.auth-mask {
  position: fixed;
  inset: 0;
  z-index: 3000;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(12, 18, 26, 0.45);
  backdrop-filter: blur(2px);
  padding: 20px;
}

.auth-panel {
  position: relative;
  width: 400px;
  max-width: 100%;
  max-height: calc(100vh - 40px);
  overflow-y: auto;
  background: #fff;
  border-radius: 18px;
  padding: 22px 24px 24px;
  box-shadow: 0 24px 60px rgba(10, 20, 30, 0.28);
}

.auth-panel__close {
  position: absolute;
  top: 10px;
  right: 12px;
  width: 30px;
  height: 30px;
  border: none;
  background: transparent;
  font-size: 22px;
  line-height: 1;
  color: #98a2ae;
  cursor: pointer;
  border-radius: 8px;
}

.auth-panel__close:hover {
  background: #f3f5f7;
  color: #303133;
}

.auth-panel__header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 18px;
}

.auth-panel__logo {
  width: 42px;
  height: 42px;
  object-fit: contain;
}

.auth-panel__brand {
  font-size: 16px;
  font-weight: 700;
  color: #1f2d3d;
  line-height: 1.3;
}

.auth-panel__slogan {
  font-size: 12px;
  color: #909399;
}

.auth-tabs {
  display: flex;
  gap: 26px;
  border-bottom: 1px solid #eef1f4;
  margin-bottom: 16px;
}

.auth-tabs__item {
  position: relative;
  padding: 8px 0 10px;
  border: none;
  background: transparent;
  font-size: 15px;
  font-weight: 600;
  color: #8a94a0;
  cursor: pointer;
}

.auth-tabs__item.is-active {
  color: #18a058;
}

.auth-tabs__item.is-active::after {
  content: '';
  position: absolute;
  left: 0;
  right: 0;
  bottom: -1px;
  height: 2px;
  border-radius: 2px;
  background: #18a058;
}

.auth-form {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.auth-field {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.auth-field__label {
  font-size: 12px;
  color: #606266;
}

.auth-submit {
  width: 100%;
  margin-top: 4px;
}

.auth-switch {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 4px;
  font-size: 12px;
  color: #909399;
  margin-top: 4px;
}

.auth-switch__btn {
  border: none;
  background: transparent;
  color: #18a058;
  font-size: 12px;
  cursor: pointer;
  padding: 0;
}

.auth-switch__btn:hover {
  text-decoration: underline;
}

.auth-fade-enter-active,
.auth-fade-leave-active {
  transition: opacity 0.18s ease;
}

.auth-fade-enter-from,
.auth-fade-leave-to {
  opacity: 0;
}
</style>
