<template>
  <div class="view-container">
    <div class="header-section">
      <h2>管理员审核</h2>
      <button @click="load" class="icon-button" title="刷新列表">
        <svg width="16" height="16" viewBox="0 0 16 16" fill="currentColor" xmlns="http://www.w3.org/2000/svg">
          <path d="M13.6 2.3C11.8 0.5 8.9 0 6.6 0.9L7.3 2.6C9 1.9 11 2.3 12.4 3.6C14.3 5.5 14.3 8.5 12.4 10.4C10.5 12.3 7.5 12.3 5.6 10.4C3.7 8.5 3.7 5.5 5.6 3.6L6.5 4.5H1.5V-0.5L3.4 1.4C0.8 4 -0.8 8.2 0.8 11.8C2.3 15.4 6.5 17 10.1 15.4C13.7 13.9 15.3 9.7 13.8 6.1C13.8 6 13.7 5.9 13.6 5.8V5.8C15 8.1 14.8 11 13.1 13.1C10.7 15.5 6.8 15.5 4.4 13.1C2 10.7 2 6.8 4.4 4.4C5.9 2.9 8.1 2.3 10.2 2.9L9.5 1.2C7.8 0.5 5.9 0.8 4.4 1.8L13.6 2.3Z"/>
        </svg>
      </button>
    </div>
    
    <div class="card settings-card">
      <div class="settings-row">
        <div class="settings-label">
          <label>管理员密钥 (X-Admin-Key)</label>
          <span class="caption">用于验证管理员权限</span>
        </div>
        <div class="settings-control">
          <FluentInput 
            v-model="key" 
            type="password"
            placeholder="Key" 
            class="compact-input-component"
          />
          <button @click="saveKey">保存</button>
        </div>
      </div>
    </div>
    
    <div v-if="items.length === 0" class="empty-state">
      <p class="subtitle">没有待审核的插件</p>
    </div>
    
    <div v-else class="list-view">
      <div v-for="p in items" :key="p.id" class="list-item">
        <div class="item-content">
          <div class="item-header">
            <span class="item-title">{{ p.name }}</span>
            <span class="item-version">v{{ p.version }}</span>
          </div>
          <div class="item-subtitle">{{ p.slug }}</div>
          <div class="item-desc">{{ p.description }}</div>
        </div>
        
        <div class="item-actions">
          <button @click="approve(p.id)" class="primary">通过</button>
          <button @click="reject(p.id)" class="danger">拒绝</button>
        </div>
      </div>
    </div>
    
    <div v-if="message" :class="['toast', { error: isError }]">
      {{ message }}
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { PluginsApi, setAdminKey, getAdminKey } from '../api.js'
import FluentInput from '../components/FluentInput.vue'

const items = ref([])
const key = ref(getAdminKey())
const message = ref('')
const isError = ref(false)

function saveKey() {
  setAdminKey(key.value)
  showMessage('密钥已保存', false)
}

function showMessage(msg, error = false) {
  message.value = msg
  isError.value = error
  setTimeout(() => message.value = '', 3000)
}

async function load() {
  try {
    const { data } = await PluginsApi.listPending()
    items.value = data
    message.value = ''
    isError.value = false
  } catch (e) {
    showMessage('加载失败：' + (e?.response?.status === 401 ? '未授权或密钥错误' : e.message), true)
  }
}

async function approve(id) {
  try { 
    await PluginsApi.approve(id)
    showMessage('已通过审核')
    await load()
  } catch (e) { 
    showMessage('审核失败：' + e.message, true)
  }
}

async function reject(id) {
  const reason = prompt('输入拒绝原因（可选）') || ''
  try { 
    await PluginsApi.reject(id, reason)
    showMessage('已拒绝')
    await load() 
  } catch (e) { 
    showMessage('拒绝失败：' + e.message, true)
  }
}

onMounted(load)
</script>

<style scoped>
.view-container {
  max-width: 800px;
  margin: 0 auto;
}

.header-section {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.icon-button {
  background: transparent;
  border: none;
  padding: 8px;
  border-radius: 4px;
  color: var(--fluent-text-secondary);
}

.icon-button:hover {
  background-color: rgba(0,0,0,0.05);
  color: var(--fluent-text-primary);
}

.settings-card {
  padding: 16px;
  margin-bottom: 24px;
}

.settings-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.settings-label {
  display: flex;
  flex-direction: column;
}

.settings-control {
  display: flex;
  gap: 8px;
  align-items: flex-start; /* Align input and button to top */
}

/* Override component styles */
.compact-input-component {
  width: 200px;
  margin-bottom: 0 !important;
}

.empty-state {
  text-align: center;
  padding: 40px;
  color: var(--fluent-text-secondary);
}

.list-view {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.list-item {
  background-color: var(--fluent-bg-card);
  border: 1px solid var(--fluent-border-default);
  border-radius: var(--fluent-radius-control);
  padding: 16px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  transition: background-color 0.1s;
}

.list-item:hover {
  background-color: var(--fluent-bg-control-hover);
}

.item-content {
  flex: 1;
  margin-right: 24px;
}

.item-header {
  display: flex;
  align-items: baseline;
  gap: 8px;
  margin-bottom: 4px;
}

.item-title {
  font-weight: 600;
  font-size: 16px;
  color: var(--fluent-text-primary);
}

.item-version {
  font-size: 12px;
  color: var(--fluent-text-secondary);
  background-color: #f3f3f3;
  padding: 1px 6px;
  border-radius: 4px;
}

.item-subtitle {
  font-family: 'Consolas', monospace;
  font-size: 12px;
  color: var(--fluent-text-secondary);
  margin-bottom: 4px;
}

.item-desc {
  font-size: 14px;
  color: var(--fluent-text-primary);
}

.item-actions {
  display: flex;
  gap: 8px;
}

.toast {
  position: fixed;
  bottom: 24px;
  left: 50%;
  transform: translateX(-50%);
  background-color: #323130;
  color: white;
  padding: 12px 24px;
  border-radius: 4px;
  box-shadow: 0 4px 8px rgba(0,0,0,0.2);
  z-index: 1000;
  font-size: 14px;
  animation: slideUp 0.3s ease-out;
}

.toast.error {
  background-color: #d13438;
}

@keyframes slideUp {
  from { transform: translate(-50%, 20px); opacity: 0; }
  to { transform: translate(-50%, 0); opacity: 1; }
}
</style>
