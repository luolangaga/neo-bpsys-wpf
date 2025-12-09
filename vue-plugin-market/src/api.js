import axios from 'axios'

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'https://bpapi.idvevent.cn',
})

export function setAdminKey(key) {
  localStorage.setItem('adminKey', key || '')
}

export function getAdminKey() {
  return localStorage.getItem('adminKey') || ''
}

api.interceptors.request.use((config) => {
  const key = getAdminKey()
  if (key) {
    config.headers['X-Admin-Key'] = key
  }
  return config
})

export const PluginsApi = {
  listApproved(q) {
    return api.get('/api/plugins', { params: { q } })
  },
  upload(formData) {
    return api.post('/api/plugins', formData, { headers: { 'Content-Type': 'multipart/form-data' } })
  },
  listPending() {
    return api.get('/api/plugins/pending')
  },
  approve(id) {
    return api.post(`/api/plugins/${id}/approve`)
  },
  reject(id, reason) {
    return api.post(`/api/plugins/${id}/reject`, { reason })
  },
}

export default api
