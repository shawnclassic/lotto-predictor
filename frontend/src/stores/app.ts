import { ref } from 'vue'
import { defineStore } from 'pinia'

export interface ToastNotification {
  id: string
  type: 'success' | 'error' | 'info' | 'warning'
  title: string
  message: string
  duration?: number
}

export const useAppStore = defineStore('app', () => {
  // Loading state
  const isLoading = ref(false)
  
  // Toast notifications
  const notifications = ref<ToastNotification[]>([])
  
  // Set loading state
  function setLoading(loading: boolean) {
    isLoading.value = loading
  }
  
  // Add toast notification
  function addNotification(notification: Omit<ToastNotification, 'id'>) {
    const id = Date.now().toString()
    const newNotification: ToastNotification = {
      ...notification,
      id,
      duration: notification.duration || 5000
    }
    
    notifications.value.push(newNotification)
    
    // Auto remove after duration
    if (newNotification.duration && newNotification.duration > 0) {
      setTimeout(() => {
        removeNotification(id)
      }, newNotification.duration)
    }
  }
  
  // Remove toast notification
  function removeNotification(id: string) {
    const index = notifications.value.findIndex(n => n.id === id)
    if (index > -1) {
      notifications.value.splice(index, 1)
    }
  }
  
  // Clear all notifications
  function clearNotifications() {
    notifications.value = []
  }

  return {
    isLoading,
    notifications,
    setLoading,
    addNotification,
    removeNotification,
    clearNotifications
  }
})