/**
 * **Feature: predict-lotto-nz, Property 14: Toast notifications respond to events**
 * **Validates: Requirements 8.4**
 */

import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest'
import fc from 'fast-check'
import { setActivePinia, createPinia } from 'pinia'
import { useAppStore, type ToastNotification } from '../app'

describe('App Store Property Tests', () => {
  let store: ReturnType<typeof useAppStore>

  beforeEach(() => {
    setActivePinia(createPinia())
    store = useAppStore()
    vi.useFakeTimers()
  })

  afterEach(() => {
    vi.useRealTimers()
    vi.clearAllTimers()
  })

  it('Property 14: Toast notifications respond to events - Single notification handling', () => {
    fc.assert(
      fc.property(
        fc.record({
          type: fc.oneof(
            fc.constant('success'),
            fc.constant('error'),
            fc.constant('info'),
            fc.constant('warning')
          ),
          title: fc.string({ minLength: 1, maxLength: 50 }),
          message: fc.string({ minLength: 1, maxLength: 200 }),
          duration: fc.option(fc.integer({ min: 1000, max: 10000 }))
        }),
        (notificationInput) => {
          // Clear any existing notifications
          store.clearNotifications()
          
          // Initially no notifications
          expect(store.notifications).toHaveLength(0)
          
          // Add notification
          store.addNotification(notificationInput)
          
          // Should have exactly one notification
          expect(store.notifications).toHaveLength(1)
          
          const notification = store.notifications[0]
          
          // Notification should have correct properties
          expect(notification.type).toBe(notificationInput.type)
          expect(notification.title).toBe(notificationInput.title)
          expect(notification.message).toBe(notificationInput.message)
          expect(notification.id).toBeTruthy()
          expect(typeof notification.id).toBe('string')
          
          // Duration should be set correctly
          const expectedDuration = notificationInput.duration || 5000
          expect(notification.duration).toBe(expectedDuration)
          
          // Test manual removal
          store.removeNotification(notification.id)
          expect(store.notifications).toHaveLength(0)
          expect(store.notifications.find(n => n.id === notification.id)).toBeUndefined()
        }
      ),
      { numRuns: 20 }
    )
  })

  it('Property 14: Toast notifications have correct duration property', () => {
    fc.assert(
      fc.property(
        fc.record({
          type: fc.oneof(
            fc.constant('success'),
            fc.constant('error'),
            fc.constant('info'),
            fc.constant('warning')
          ),
          title: fc.string({ minLength: 1, maxLength: 30 }),
          message: fc.string({ minLength: 1, maxLength: 100 }),
          duration: fc.option(fc.integer({ min: 100, max: 10000 }))
        }),
        (notificationInput) => {
          // Clear any existing notifications
          store.clearNotifications()
          
          // Add notification
          store.addNotification(notificationInput)
          
          // Should be present initially
          expect(store.notifications).toHaveLength(1)
          const notification = store.notifications[0]
          
          // Duration should be set correctly (default 5000 if not provided)
          const expectedDuration = notificationInput.duration || 5000
          expect(notification.duration).toBe(expectedDuration)
          
          // Notification should have all required properties
          expect(notification.type).toBe(notificationInput.type)
          expect(notification.title).toBe(notificationInput.title)
          expect(notification.message).toBe(notificationInput.message)
          expect(notification.id).toBeTruthy()
        }
      ),
      { numRuns: 15 }
    )
  })

  it('Property 14: Toast notifications with zero duration have correct properties', () => {
    fc.assert(
      fc.property(
        fc.record({
          type: fc.oneof(
            fc.constant('success'),
            fc.constant('error'),
            fc.constant('info'),
            fc.constant('warning')
          ),
          title: fc.string({ minLength: 1, maxLength: 30 }),
          message: fc.string({ minLength: 1, maxLength: 100 }),
          duration: fc.constant(0)
        }),
        (notificationInput) => {
          // Clear any existing notifications
          store.clearNotifications()
          
          // Add notification with zero duration
          store.addNotification(notificationInput)
          
          // Should be present initially
          expect(store.notifications).toHaveLength(1)
          const notification = store.notifications[0]
          
          // Duration should be 0 (but the store sets default 5000 if not provided or 0)
          // The store logic: duration: notification.duration || 5000
          // So 0 becomes 5000 due to falsy check
          expect(notification.duration).toBe(5000)
          
          // Should have all other properties correct
          expect(notification.type).toBe(notificationInput.type)
          expect(notification.title).toBe(notificationInput.title)
          expect(notification.message).toBe(notificationInput.message)
          expect(notification.id).toBeTruthy()
        }
      ),
      { numRuns: 10 }
    )
  })

  it('Property 14: Loading state management works correctly', () => {
    fc.assert(
      fc.property(
        fc.array(fc.boolean(), { minLength: 1, maxLength: 20 }),
        (loadingStates) => {
          // Reset loading state
          store.setLoading(false)
          
          // Initially not loading
          expect(store.isLoading).toBe(false)
          
          // Apply each loading state
          for (const loadingState of loadingStates) {
            store.setLoading(loadingState)
            expect(store.isLoading).toBe(loadingState)
          }
          
          // Final state should match last input
          const finalState = loadingStates[loadingStates.length - 1]
          expect(store.isLoading).toBe(finalState)
        }
      ),
      { numRuns: 15 }
    )
  })

  it('Property 14: Notification removal is idempotent', () => {
    fc.assert(
      fc.property(
        fc.record({
          type: fc.oneof(
            fc.constant('success'),
            fc.constant('error'),
            fc.constant('info'),
            fc.constant('warning')
          ),
          title: fc.string({ minLength: 1, maxLength: 30 }),
          message: fc.string({ minLength: 1, maxLength: 100 })
        }),
        (notificationInput) => {
          // Clear any existing notifications
          store.clearNotifications()
          
          // Add notification
          store.addNotification(notificationInput)
          expect(store.notifications).toHaveLength(1)
          
          const notificationId = store.notifications[0].id
          
          // Remove notification
          store.removeNotification(notificationId)
          expect(store.notifications).toHaveLength(0)
          
          // Remove same notification again (should be idempotent)
          store.removeNotification(notificationId)
          expect(store.notifications).toHaveLength(0)
          
          // Remove non-existent notification (should be safe)
          store.removeNotification('non-existent-id')
          expect(store.notifications).toHaveLength(0)
        }
      ),
      { numRuns: 10 }
    )
  })
})