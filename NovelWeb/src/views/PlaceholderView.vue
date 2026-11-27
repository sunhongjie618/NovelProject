<script setup lang="ts">
import { useRoute, useRouter } from 'vue-router'

defineProps<{ title?: string }>()

const route = useRoute()
const router = useRouter()
</script>

<template>
  <div class="page-container">
    <el-card shadow="never">
      <el-result
        icon="info"
        :title="title || '页面开发中'"
        :sub-title="`当前路由：${route.fullPath}`"
      >
        <template #extra>
          <el-button type="primary" @click="router.push('/')">回到首页</el-button>
        </template>
      </el-result>
    </el-card>
  </div>
</template>
