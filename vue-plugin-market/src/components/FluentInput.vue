<template>
  <div class="fluent-input-wrapper" :class="{ 'is-disabled': disabled, 'has-error': !!errorMessage, 'is-focused': isFocused }">
    <div class="fluent-label-wrapper" v-if="label">
      <label :for="inputId" class="fluent-label">
        {{ label }}
        <span v-if="required" class="required-mark">*</span>
      </label>
    </div>

    <div class="fluent-input-container">
      <div class="fluent-input-prefix" v-if="$slots.prefix">
        <slot name="prefix"></slot>
      </div>
      
      <input
        :id="inputId"
        ref="inputRef"
        :type="type"
        :value="modelValue"
        :placeholder="placeholder"
        :disabled="disabled"
        :readonly="readonly"
        class="fluent-input"
        @input="updateValue"
        @focus="handleFocus"
        @blur="handleBlur"
        v-bind="$attrs"
      />

      <div class="fluent-input-suffix" v-if="$slots.suffix">
        <slot name="suffix"></slot>
      </div>

      <!-- Focus border animation -->
      <div class="fluent-input-border-bottom"></div>
    </div>

    <div class="fluent-input-description" v-if="description && !errorMessage">
      {{ description }}
    </div>
    
    <div class="fluent-input-error" v-if="errorMessage">
      {{ errorMessage }}
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue';

const props = defineProps({
  modelValue: {
    type: [String, Number],
    default: ''
  },
  label: {
    type: String,
    default: ''
  },
  placeholder: {
    type: String,
    default: ''
  },
  type: {
    type: String,
    default: 'text'
  },
  description: {
    type: String,
    default: ''
  },
  errorMessage: {
    type: String,
    default: ''
  },
  disabled: {
    type: Boolean,
    default: false
  },
  readonly: {
    type: Boolean,
    default: false
  },
  required: {
    type: Boolean,
    default: false
  },
  id: {
    type: String,
    default: () => `fluent-input-${Math.random().toString(36).substr(2, 9)}`
  }
});

const emit = defineEmits(['update:modelValue', 'focus', 'blur']);

const inputRef = ref(null);
const isFocused = ref(false);
const inputId = computed(() => props.id);

const updateValue = (event) => {
  emit('update:modelValue', event.target.value);
};

const handleFocus = (event) => {
  isFocused.value = true;
  emit('focus', event);
};

const handleBlur = (event) => {
  isFocused.value = false;
  emit('blur', event);
};

defineExpose({
  focus: () => inputRef.value?.focus(),
  blur: () => inputRef.value?.blur()
});
</script>

<style scoped>
.fluent-input-wrapper {
  display: flex;
  flex-direction: column;
  margin-bottom: 16px;
  font-family: var(--fluent-font-family, "Segoe UI", sans-serif);
  position: relative;
}

/* Label Styles */
.fluent-label-wrapper {
  margin-bottom: 4px;
}

.fluent-label {
  font-size: 14px;
  font-weight: 600;
  color: var(--fluent-text-primary, #242424);
  cursor: pointer;
  display: block;
}

.required-mark {
  color: #a4262c;
  margin-left: 4px;
}

/* Input Container Styles */
.fluent-input-container {
  position: relative;
  display: flex;
  align-items: center;
  background-color: var(--fluent-bg-control, #ffffff);
  border: 1px solid var(--fluent-border-default, #8a8886); /* Standard border */
  border-bottom-color: var(--fluent-border-input, #8a8886); /* Slightly darker bottom for standard state */
  border-radius: var(--fluent-radius-control, 4px);
  height: 32px; /* Standard Fluent height */
  transition: background-color 0.1s ease, border-color 0.1s ease;
  box-sizing: border-box;
}

.fluent-input-container:hover {
  border-color: var(--fluent-border-input-hover, #323130);
  background-color: var(--fluent-bg-control-hover, #fcfcfc);
}

/* Input Field Styles */
.fluent-input {
  flex: 1;
  border: none;
  background: transparent;
  padding: 0 10px;
  height: 100%;
  font-family: inherit;
  font-size: 14px;
  color: var(--fluent-text-primary, #242424);
  outline: none;
  width: 100%;
}

.fluent-input::placeholder {
  color: var(--fluent-text-secondary, #605e5c);
  opacity: 1;
}

/* Prefix/Suffix Slots */
.fluent-input-prefix,
.fluent-input-suffix {
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--fluent-text-secondary, #605e5c);
  padding: 0 8px;
}

.fluent-input-prefix {
  padding-right: 0;
}

.fluent-input-suffix {
  padding-left: 0;
}

/* Focus State - Bottom Border Animation */
.fluent-input-border-bottom {
  position: absolute;
  bottom: -1px; /* Align with border */
  left: -1px;
  right: -1px;
  height: 2px;
  background-color: var(--fluent-accent-default, #0078d4);
  transform: scaleX(0);
  transition: transform 0.2s cubic-bezier(0.1, 0.9, 0.2, 1); /* Fluent motion */
  pointer-events: none;
  border-bottom-left-radius: var(--fluent-radius-control, 4px);
  border-bottom-right-radius: var(--fluent-radius-control, 4px);
  z-index: 1;
}

.is-focused .fluent-input-border-bottom {
  transform: scaleX(1);
}

.is-focused .fluent-input-container {
  background-color: var(--fluent-bg-control-active, #ffffff);
  /* Hide the default border color when focused to let the blue bar shine, 
     but keep the shape. Actually Fluent keeps the border but adds the blue bar. */
}

/* Error State */
.has-error .fluent-input-container {
  border-color: #a4262c;
}

.has-error .fluent-input-border-bottom {
  background-color: #a4262c;
}

.fluent-input-error {
  color: #a4262c;
  font-size: 12px;
  margin-top: 4px;
  animation: slideDown 0.2s ease-out forwards;
}

@keyframes slideDown {
  from { opacity: 0; transform: translateY(-5px); }
  to { opacity: 1; transform: translateY(0); }
}

/* Disabled State */
.is-disabled .fluent-label {
  color: var(--fluent-text-disabled, #a19f9d);
  cursor: default;
}

.is-disabled .fluent-input-container {
  background-color: #f3f2f1;
  border-color: #f3f2f1;
  pointer-events: none;
}

.is-disabled .fluent-input {
  color: var(--fluent-text-disabled, #a19f9d);
}

.is-disabled .fluent-input::placeholder {
  color: var(--fluent-text-disabled, #a19f9d);
}

/* Description Helper Text */
.fluent-input-description {
  font-size: 12px;
  color: var(--fluent-text-secondary, #605e5c);
  margin-top: 4px;
}
</style>