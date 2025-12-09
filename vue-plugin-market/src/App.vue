<template>
  <div class="app-layout">
    <header class="main-header">
      <div class="header-content container">
        <div class="brand">
          <h1>插件市场</h1>
        </div>
        <!-- Fluent Pivot / NavigationView Top -->
        <nav class="pivot-nav">
          <button 
            :class="['pivot-item', { active: tab === 'approved' }]" 
            @click="tab = 'approved'"
          >
            已审核插件
          </button>
          <button 
            :class="['pivot-item', { active: tab === 'upload' }]" 
            @click="tab = 'upload'"
          >
            上传插件
          </button>
          <button 
            :class="['pivot-item', { active: tab === 'admin' }]" 
            @click="tab = 'admin'"
          >
            管理员审核
          </button>
          <button 
            :class="['pivot-item', { active: tab === 'docs' }]" 
            @click="tab = 'docs'"
          >
            开发文档
          </button>
        </nav>
      </div>
    </header>
    
    <main class="main-content container">
      <transition name="fade" mode="out-in">
        <section v-if="tab === 'approved'" key="approved">
          <ApprovedList />
        </section>
        <section v-else-if="tab === 'upload'" key="upload">
          <UploadForm />
        </section>
        <section v-else-if="tab === 'admin'" key="admin">
          <AdminPanel />
        </section>
        <section v-else key="docs">
          <DocViewer />
        </section>
      </transition>
    </main>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import ApprovedList from './views/ApprovedList.vue'
import UploadForm from './views/UploadForm.vue'
import AdminPanel from './views/AdminPanel.vue'
import DocViewer from './views/DocViewer.vue'

const tab = ref('approved')
</script>

<style scoped>
.app-layout {
  min-height: 100vh;
  background-color: var(--fluent-bg-page);
}

.main-header {
  background-color: var(--fluent-bg-card);
  border-bottom: 1px solid var(--fluent-divider);
  position: sticky;
  top: 0;
  z-index: 100;
  padding-top: 12px;
}

.header-content {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding-bottom: 0; /* Pivot items sit on the bottom line */
  height: 60px;
}

.brand h1 {
  margin: 0;
  font-size: 20px;
  font-weight: 600;
}

.pivot-nav {
  display: flex;
  gap: 24px;
  height: 100%;
}

/* Fluent Pivot Item Style */
.pivot-item {
  background: transparent;
  border: none;
  border-radius: 4px; /* Slight radius for hover effect */
  border-bottom-left-radius: 0;
  border-bottom-right-radius: 0;
  padding: 0 8px;
  margin: 0;
  height: 100%;
  position: relative;
  color: var(--fluent-text-secondary);
  font-weight: 400;
  font-size: 14px;
  cursor: pointer;
  display: flex;
  align-items: center;
  transition: color 0.1s, background-color 0.1s;
}

.pivot-item:hover {
  background-color: rgba(0, 0, 0, 0.04);
  color: var(--fluent-text-primary);
  border-color: transparent; /* Override global button border */
}

/* Active indicator (Underline) */
.pivot-item::after {
  content: '';
  position: absolute;
  bottom: 0;
  left: 8px;
  right: 8px;
  height: 3px;
  background-color: var(--fluent-accent-default);
  border-radius: 3px 3px 0 0;
  transform: scaleX(0);
  transition: transform 0.2s cubic-bezier(0.1, 0.9, 0.2, 1);
}

.pivot-item.active {
  color: var(--fluent-text-primary);
  font-weight: 600;
  background-color: transparent;
}

.pivot-item.active::after {
  transform: scaleX(1);
}

.main-content {
  padding-top: 24px;
  padding-bottom: 48px;
}

/* Transitions */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
