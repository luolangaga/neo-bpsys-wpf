<template>
  <div class="doc-sidebar-item">
    <div 
      class="item-header" 
      :class="{ active: isActive, 'is-folder': isFolder }"
      @click="handleClick"
    >
      <span v-if="isFolder" class="icon">{{ isOpen ? '📂' : '📁' }}</span>
      <span v-else class="icon">📄</span>
      <span class="label">{{ item.name }}</span>
    </div>
    
    <div v-if="isFolder && isOpen" class="children">
      <DocSidebarItem
        v-for="child in item.children"
        :key="child.path"
        :item="child"
        :current-path="currentPath"
        @select="$emit('select', $event)"
      />
    </div>
  </div>
</template>

<script setup>
import { computed, ref } from 'vue'

const props = defineProps({
  item: {
    type: Object,
    required: true
  },
  currentPath: {
    type: String,
    default: ''
  }
})

const emit = defineEmits(['select'])

const isOpen = ref(true) // Default open for better visibility

const isFolder = computed(() => props.item.type === 'directory')
const isActive = computed(() => props.item.path === props.currentPath)

function handleClick() {
  if (isFolder.value) {
    isOpen.value = !isOpen.value
  } else {
    emit('select', props.item.path)
  }
}
</script>

<style scoped>
.doc-sidebar-item {
  user-select: none;
}

.item-header {
  display: flex;
  align-items: center;
  padding: 4px 8px;
  cursor: pointer;
  border-radius: 4px;
  color: var(--fluent-text-primary);
}

.item-header:hover {
  background-color: var(--fluent-bg-subtle-hover);
}

.item-header.active {
  background-color: var(--fluent-bg-subtle-pressed);
  color: var(--fluent-accent-default);
  font-weight: 500;
}

.icon {
  margin-right: 8px;
  font-size: 14px;
}

.label {
  font-size: 14px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.children {
  padding-left: 16px;
  border-left: 1px solid var(--fluent-divider);
  margin-left: 7px; /* Align line with parent icon center approximately */
}
</style>
