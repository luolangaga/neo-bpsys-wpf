<template>
  <div class="doc-viewer">
    <aside class="sidebar">
      <div class="sidebar-header">
        <h3>文档目录</h3>
      </div>
      <div class="sidebar-content">
        <div v-if="loadingManifest" class="loading">加载目录...</div>
        <div v-else-if="error" class="error">{{ error }}</div>
        <div v-else class="tree-root">
          <DocSidebarItem
            v-for="item in manifest"
            :key="item.path"
            :item="item"
            :current-path="currentPath"
            @select="loadDoc"
          />
        </div>
      </div>
    </aside>
    
    <main class="content-area">
      <div v-if="loadingContent" class="loading-content">
        <div class="spinner"></div>
        <span>加载文档...</span>
      </div>
      <div v-else-if="contentError" class="error-content">
        {{ contentError }}
      </div>
      <div v-else class="markdown-body" v-html="renderedContent"></div>
    </main>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import MarkdownIt from 'markdown-it'
import 'github-markdown-css/github-markdown.css'
import DocSidebarItem from '../components/DocSidebarItem.vue'

const md = new MarkdownIt({
  html: true,
  linkify: true,
  typographer: true
})

const manifest = ref([])
const loadingManifest = ref(true)
const error = ref(null)

const currentPath = ref('')
const renderedContent = ref('')
const loadingContent = ref(false)
const contentError = ref(null)

// Flatten helper to find first file
function findFirstFile(items) {
  for (const item of items) {
    if (item.type === 'file') return item;
    if (item.type === 'directory' && item.children) {
      const found = findFirstFile(item.children);
      if (found) return found;
    }
  }
  return null;
}

async function fetchManifest() {
  try {
    loadingManifest.value = true
    const baseUrl = import.meta.env.BASE_URL
    // Ensure baseUrl ends with / if it doesn't (Vite usually ensures this)
    const prefix = baseUrl.endsWith('/') ? baseUrl : baseUrl + '/'
    
    const response = await fetch(`${prefix}docs/manifest.json`)
    if (!response.ok) throw new Error('Failed to load manifest')
    manifest.value = await response.json()
    
    // Auto-select first file if available
    const firstFile = findFirstFile(manifest.value)
    if (firstFile) {
      loadDoc(firstFile.path)
    }
  } catch (err) {
    error.value = err.message
  } finally {
    loadingManifest.value = false
  }
}

async function loadDoc(path) {
  if (currentPath.value === path) return
  
  try {
    currentPath.value = path
    loadingContent.value = true
    contentError.value = null
    
    const baseUrl = import.meta.env.BASE_URL
    const prefix = baseUrl.endsWith('/') ? baseUrl : baseUrl + '/'
    
    const response = await fetch(`${prefix}docs/${path}`)
    if (!response.ok) throw new Error('Failed to load document')
    
    const text = await response.text()
    renderedContent.value = md.render(text)
  } catch (err) {
    contentError.value = '无法加载文档内容: ' + err.message
    renderedContent.value = ''
  } finally {
    loadingContent.value = false
  }
}

onMounted(() => {
  fetchManifest()
})
</script>

<style scoped>
.doc-viewer {
  display: flex;
  height: calc(100vh - 80px); /* Adjust based on header height */
  background-color: var(--fluent-bg-page);
  border: 1px solid var(--fluent-divider);
  border-radius: 8px;
  overflow: hidden;
}

.sidebar {
  width: 280px;
  flex-shrink: 0;
  border-right: 1px solid var(--fluent-divider);
  background-color: var(--fluent-bg-card);
  display: flex;
  flex-direction: column;
}

.sidebar-header {
  padding: 16px;
  border-bottom: 1px solid var(--fluent-divider);
}

.sidebar-header h3 {
  margin: 0;
  font-size: 16px;
  font-weight: 600;
}

.sidebar-content {
  flex: 1;
  overflow-y: auto;
  padding: 8px;
}

.content-area {
  flex: 1;
  overflow-y: auto;
  padding: 32px;
  background-color: white; /* Markdown usually looks best on white */
}

/* Dark mode adjustment for markdown body if needed */
@media (prefers-color-scheme: dark) {
  .content-area {
    background-color: #0d1117; /* GitHub dark bg */
  }
}

.loading, .error {
  padding: 16px;
  text-align: center;
  color: var(--fluent-text-secondary);
}

.error {
  color: #d13438;
}

.loading-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 100%;
  color: var(--fluent-text-secondary);
}

.spinner {
  width: 32px;
  height: 32px;
  border: 3px solid var(--fluent-divider);
  border-top-color: var(--fluent-accent-default);
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin-bottom: 16px;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* Customize markdown body to fit fluent design slightly if needed */
.markdown-body {
  box-sizing: border-box;
  min-width: 200px;
  max-width: 980px;
  margin: 0 auto;
  padding: 0;
}
</style>
