<template>
  <div class="view-container">
    <div class="header-section">
      <h2>上传新插件</h2>
      <p class="subtitle">填写以下信息以提交插件审核</p>
    </div>
    
    <div class="card form-card">
      <form @submit.prevent="submit" class="fluent-form">
        <div class="form-section">
          <h3>基本信息</h3>
          
          <FluentInput
            v-model="slug"
            required
            label="标识符 (Slug)"
            placeholder="例如: bpsys.sample"
            description="推荐使用反向域名格式，唯一且不可更改。"
          />
          
          <div class="form-row">
            <FluentInput
              v-model="name"
              required
              label="名称"
              placeholder="插件显示名称"
              class="flex-1"
            />
            <FluentInput
              v-model="version"
              required
              label="版本号"
              placeholder="1.0.0"
              class="flex-1"
            />
          </div>
          
          <div class="form-group">
            <label class="fluent-label">描述</label>
            <textarea v-model="description" rows="4" placeholder="简要描述插件的功能..." class="fluent-textarea"></textarea>
          </div>
        </div>

        <div class="separator"></div>
        
        <div class="form-section">
          <h3>配置与文件</h3>
          
          <div class="form-group checkbox-wrapper">
            <label class="fluent-checkbox">
              <input type="checkbox" v-model="requiresRestart" />
              <span>安装后需要重启应用</span>
            </label>
          </div>
          
          <div class="form-group">
            <label class="fluent-label">插件包文件</label>
            <div 
              class="file-drop-zone" 
              :class="{ 'has-file': file }"
              @click="triggerFileSelect"
            >
              <input 
                type="file" 
                ref="fileInput"
                @change="onFile" 
                required 
                hidden
              />
              <div class="drop-content">
                <span class="file-icon" v-if="!file">📂</span>
                <span class="file-name" v-else>{{ file.name }}</span>
                <span class="file-hint" v-if="!file">点击选择文件</span>
                <span class="file-hint change-text" v-else>点击更换</span>
              </div>
            </div>
          </div>
        </div>
        
        <div class="form-actions">
          <button type="submit" :disabled="uploading" class="primary big-button">
            {{ uploading ? '正在上传...' : '提交审核' }}
          </button>
        </div>
      </form>
    </div>
    
    <!-- InfoBar / Message -->
    <div v-if="message" :class="['info-bar', { error: isError, success: !isError }]">
      <span class="status-icon">{{ isError ? '❌' : '✅' }}</span>
      <span class="status-text">{{ message }}</span>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { PluginsApi } from '../api.js'
import FluentInput from '../components/FluentInput.vue'

const slug = ref('')
const name = ref('')
const version = ref('')
const description = ref('')
const requiresRestart = ref(false)
const file = ref(null)
const message = ref('')
const isError = ref(false)
const uploading = ref(false)
const fileInput = ref(null)

function triggerFileSelect() {
  fileInput.value.click()
}

function onFile(e) {
  file.value = e.target.files?.[0] ?? null
}

async function submit() {
  if (!file.value) { 
    message.value = '请选择文件'
    isError.value = true
    return 
  }
  
  uploading.value = true
  message.value = ''
  
  const fd = new FormData()
  fd.append('Slug', slug.value)
  fd.append('Name', name.value)
  fd.append('Version', version.value)
  fd.append('Description', description.value)
  fd.append('RequiresRestart', requiresRestart.value ? 'true' : 'false')
  fd.append('package', file.value)
  
  try {
    await PluginsApi.upload(fd)
    message.value = '上传成功，请等待管理员审核'
    isError.value = false
    
    // Reset form
    slug.value = name.value = version.value = description.value = ''
    requiresRestart.value = false
    file.value = null
  } catch (e) {
    message.value = '上传失败：' + (e?.response?.data || e.message)
    isError.value = true
  } finally {
    uploading.value = false
  }
}
</script>

<style scoped>
.view-container {
  max-width: 720px;
  margin: 0 auto;
}

.header-section {
  margin-bottom: 24px;
}

.form-card {
  padding: 32px;
}

.fluent-form {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.form-section h3 {
  font-size: 18px;
  margin-bottom: 16px;
  color: var(--fluent-text-primary);
}

.form-group {
  margin-bottom: 16px;
}

.form-row {
  display: flex;
  gap: 16px;
}

.flex-1 {
  flex: 1;
}

/* Add styles for manual labels/textarea to match component */
.fluent-label {
  font-size: 14px;
  font-weight: 600;
  color: var(--fluent-text-primary, #242424);
  margin-bottom: 4px;
  display: block;
}

.fluent-textarea {
  width: 100%;
  border: 1px solid var(--fluent-border-default, #8a8886);
  border-radius: var(--fluent-radius-control, 4px);
  padding: 8px 10px;
  font-family: inherit;
  font-size: 14px;
  color: var(--fluent-text-primary);
  background-color: var(--fluent-bg-control);
  resize: vertical;
  outline: none;
  transition: all 0.1s;
}

.fluent-textarea:focus {
  border-color: var(--fluent-accent-default);
}

.fluent-textarea:hover {
  background-color: var(--fluent-bg-control-hover);
}

.separator {
  height: 1px;
  background-color: var(--fluent-divider);
  margin: 8px 0;
}

.fluent-checkbox {
  display: flex;
  align-items: center;
  cursor: pointer;
  user-select: none;
}

.file-drop-zone {
  border: 1px dashed var(--fluent-border-input);
  background-color: var(--fluent-bg-control);
  border-radius: var(--fluent-radius-card);
  padding: 32px;
  text-align: center;
  cursor: pointer;
  transition: all 0.2s;
}

.file-drop-zone:hover {
  background-color: var(--fluent-bg-control-hover);
  border-color: var(--fluent-accent-default);
}

.file-drop-zone.has-file {
  border-style: solid;
  background-color: #f0f9ff; /* Very light blue */
  border-color: var(--fluent-accent-default);
}

.drop-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
}

.file-icon {
  font-size: 24px;
}

.file-name {
  font-weight: 600;
  color: var(--fluent-accent-default);
}

.file-hint {
  color: var(--fluent-text-secondary);
  font-size: 14px;
}

.change-text {
  font-size: 12px;
  color: var(--fluent-text-secondary);
}

.form-actions {
  margin-top: 16px;
  display: flex;
  justify-content: flex-end;
}

.big-button {
  padding: 8px 32px;
  font-size: 16px;
}

.info-bar {
  margin-top: 24px;
  padding: 12px 16px;
  border-radius: 4px;
  display: flex;
  align-items: center;
  gap: 12px;
  font-size: 14px;
}

.info-bar.success {
  background-color: #dff6dd;
  color: #107c10;
  border: 1px solid #d2e9d0;
}

.info-bar.error {
  background-color: #fde7e9;
  color: #a80000;
  border: 1px solid #f6d1d4;
}
</style>
