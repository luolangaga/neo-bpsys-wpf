<template>
  <div class="view-container">
    <div class="command-bar">
      <h2>已审核插件</h2>
      <div class="search-box">
        <FluentInput 
          v-model="q" 
          placeholder="搜索" 
          @keyup.enter="load"
          class="search-input-component"
        >
          <template #suffix>
            <button class="icon-button" @click="load" title="搜索">
              <svg width="16" height="16" viewBox="0 0 16 16" fill="currentColor" xmlns="http://www.w3.org/2000/svg">
                <path d="M15.7 14.3L11.5 10.1C12.3 9.1 12.8 7.8 12.8 6.4C12.8 2.9 9.9 0 6.4 0C2.9 0 0 2.9 0 6.4C0 9.9 2.9 12.8 6.4 12.8C7.8 12.8 9.1 12.3 10.1 11.5L14.3 15.7C14.5 15.9 14.8 16 15 16C15.2 16 15.5 15.9 15.7 15.7C16.1 15.3 16.1 14.7 15.7 14.3ZM6.4 11.2C3.8 11.2 1.6 9 1.6 6.4C1.6 3.8 3.8 1.6 6.4 1.6C9 1.6 11.2 3.8 11.2 6.4C11.2 9 9 11.2 6.4 11.2Z"/>
              </svg>
            </button>
          </template>
        </FluentInput>
      </div>
    </div>
    
    <div v-if="items.length === 0" class="empty-state">
      <p class="subtitle">没有找到相关内容</p>
    </div>
    
    <div v-else class="grid-view">
      <div v-for="p in items" :key="p.id" class="card fluent-card">
        <div class="card-header">
          <div class="icon-placeholder">
            {{ p.name.charAt(0).toUpperCase() }}
          </div>
          <div class="header-text">
            <h3 class="plugin-title">{{ p.name }}</h3>
            <span class="caption">{{ p.slug }}</span>
          </div>
        </div>
        
        <div class="card-body">
          <p class="description">{{ p.description }}</p>
          <div class="meta-row">
            <span class="version-tag">v{{ p.version }}</span>
          </div>
        </div>
        
        <div class="card-footer">
          <a :href="p.downloadUrl" target="_blank" class="fluent-link-button">
            获取
          </a>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { onMounted, ref } from 'vue'
import { PluginsApi } from '../api.js'
import FluentInput from '../components/FluentInput.vue'

const items = ref([])
const q = ref('')

async function load() {
  const { data } = await PluginsApi.listApproved(q.value)
  items.value = data
}

onMounted(load)
</script>

<style scoped>
.view-container {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.command-bar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 10px;
}

.search-box {
  width: 300px;
}

/* Override default margin of the component */
.search-input-component {
  margin-bottom: 0 !important;
}

.icon-button {
  background: transparent;
  border: none;
  padding: 0;
  min-width: 24px;
  height: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--fluent-text-secondary);
  cursor: pointer;
}

.icon-button:hover {
  color: var(--fluent-text-primary);
}

.grid-view {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 16px;
}

.fluent-card {
  display: flex;
  flex-direction: column;
  padding: 16px;
  transition: transform 0.2s, box-shadow 0.2s;
  height: 100%;
}

.fluent-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 16px rgba(0,0,0,0.1);
}

.card-header {
  display: flex;
  gap: 12px;
  margin-bottom: 12px;
}

.icon-placeholder {
  width: 48px;
  height: 48px;
  background-color: var(--fluent-accent-default);
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 20px;
  font-weight: 600;
  border-radius: 4px;
  flex-shrink: 0;
}

.header-text {
  display: flex;
  flex-direction: column;
  justify-content: center;
  overflow: hidden;
}

.plugin-title {
  font-size: 16px;
  margin: 0;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.caption {
  font-family: 'Consolas', monospace;
}

.card-body {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.description {
  font-size: 14px;
  color: var(--fluent-text-secondary);
  margin: 0;
  display: -webkit-box;
  -webkit-line-clamp: 3;
  -webkit-box-orient: vertical;
  overflow: hidden;
  line-height: 1.4;
}

.meta-row {
  margin-top: auto;
  padding-top: 12px;
}

.version-tag {
  background-color: #f3f3f3;
  padding: 2px 8px;
  border-radius: 4px;
  font-size: 12px;
  color: var(--fluent-text-secondary);
}

.card-footer {
  margin-top: 16px;
  text-align: right;
}

.fluent-link-button {
  display: inline-block;
  background-color: #f3f3f3;
  color: var(--fluent-text-primary);
  padding: 6px 20px;
  border-radius: 4px;
  font-size: 14px;
  font-weight: 600;
  transition: background-color 0.1s;
}

.fluent-link-button:hover {
  background-color: #e0e0e0;
  text-decoration: none;
}
</style>
