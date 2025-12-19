<template>
  <main>
    <div class="predictions">
      <h1>Predictions</h1>
      <p>Generate and view lottery number predictions</p>
      
      <div class="prediction-controls">
        <div class="control-group">
          <label for="predictionCount">Number of Predictions:</label>
          <select id="predictionCount" v-model="predictionCount" class="prediction-select">
            <option v-for="n in 10" :key="n" :value="n">{{ n }}</option>
          </select>
        </div>
        
        <div class="button-group">
          <button @click="generatePredictions" :disabled="isLoading" class="generate-button primary">
            <span v-if="isLoading">Generating...</span>
            <span v-else>Generate New</span>
          </button>
          
          <button @click="loadStoredPredictions" :disabled="isLoading" class="generate-button secondary">
            <span v-if="isLoading">Loading...</span>
            <span v-else>Load Stored (Simple)</span>
          </button>
          
          <button @click="() => loadPaginatedPredictions(1)" :disabled="isLoading" class="generate-button secondary">
            <span v-if="isLoading">Loading...</span>
            <span v-else>Browse All Stored</span>
          </button>
        </div>
      </div>
      
      <div v-if="predictions.length > 0" class="predictions-table">
        <h3>Generated Predictions</h3>
        <table>
          <thead>
            <tr>
              <th>#</th>
              <th>Numbers</th>
              <th>Source</th>
              <th>Score</th>
              <th>Created</th>
              <th v-if="showTargetDate">Target Draw</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(prediction, index) in displayedPredictions" :key="prediction.id || index" class="prediction-row">
              <td>{{ getDisplayIndex(index) }}</td>
              <td class="numbers-cell">
                <span v-for="number in prediction.numbers" :key="number" class="number-ball">
                  {{ number }}
                </span>
              </td>
              <td class="source-cell">{{ prediction.source }}</td>
              <td class="score-cell">{{ prediction.score?.toFixed(2) || 'N/A' }}</td>
              <td class="date-cell">{{ formatDate(prediction.createdAt) }}</td>
              <td v-if="showTargetDate" class="date-cell">
                {{ prediction.targetDrawDate ? formatDate(prediction.targetDrawDate) : 'N/A' }}
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      
      <!-- Pagination Controls -->
      <div v-if="paginatedMode && paginationData" class="pagination-controls">
        <div class="pagination-info">
          <span>Showing {{ paginationData.firstItemIndex }} - {{ paginationData.lastItemIndex }} of {{ paginationData.totalItems }} predictions</span>
        </div>
        
        <div class="pagination-buttons">
          <button 
            @click="goToPage(1)" 
            :disabled="!paginationData.hasPreviousPage || isLoading"
            class="pagination-button"
          >
            First
          </button>
          
          <button 
            @click="goToPage(currentPage - 1)" 
            :disabled="!paginationData.hasPreviousPage || isLoading"
            class="pagination-button"
          >
            Previous
          </button>
          
          <span class="page-info">
            Page {{ currentPage }} of {{ paginationData.totalPages }}
          </span>
          
          <button 
            @click="goToPage(currentPage + 1)" 
            :disabled="!paginationData.hasNextPage || isLoading"
            class="pagination-button"
          >
            Next
          </button>
          
          <button 
            @click="goToPage(paginationData.totalPages)" 
            :disabled="!paginationData.hasNextPage || isLoading"
            class="pagination-button"
          >
            Last
          </button>
        </div>
        
        <div class="page-size-control">
          <label for="pageSize">Per page:</label>
          <select id="pageSize" v-model="pageSize" @change="() => loadPaginatedPredictions(1)" class="prediction-select">
            <option value="10">10</option>
            <option value="25">25</option>
            <option value="50">50</option>
            <option value="100">100</option>
          </select>
        </div>
      </div>

      <div v-if="predictions.length === 0 && !isLoading" class="no-predictions">
        <p>No predictions loaded. Click "Generate New" to create fresh predictions, "Load Stored (Simple)" for recent ones, or "Browse All Stored" to see all with pagination.</p>
      </div>
      
      <div v-if="isLoading" class="loading-state">
        <div class="loading-spinner"></div>
        <p>Generating predictions...</p>
      </div>
    </div>
  </main>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import api from '@/services/api'
import { useAppStore } from '@/stores/app'

interface PredictionResult {
  id?: number
  numbers: number[]
  source: string
  score?: number
  createdAt: string
  targetDrawDate?: string
  confidenceScore?: number
  reasoningExplanation?: string
}

interface PaginationData {
  page: number
  pageSize: number
  totalItems: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
  firstItemIndex: number
  lastItemIndex: number
}

const appStore = useAppStore()

// Reactive state
const predictionCount = ref(10)
const predictions = ref<PredictionResult[]>([])
const isLoading = ref(false)
const paginatedMode = ref(false)
const currentPage = ref(1)
const pageSize = ref(50)
const paginationData = ref<PaginationData | null>(null)

// Computed properties
const displayedPredictions = computed(() => {
  return paginatedMode.value ? predictions.value : predictions.value
})

const showTargetDate = computed(() => {
  return predictions.value.some(p => p.targetDrawDate)
})

// Generate predictions
const generatePredictions = async () => {
  isLoading.value = true
  appStore.setLoading(true)
  paginatedMode.value = false
  paginationData.value = null
  
  try {
    // Use the dedicated predictions endpoint for generating new predictions
    const response = await api.get(`/predictions/generate?count=${predictionCount.value}`)
    predictions.value = response.data
    
    appStore.addNotification({
      type: 'success',
      title: 'Predictions Generated',
      message: `Generated ${predictions.value.length} predictions successfully`
    })
  } catch (error: any) {
    const errorMessage = error.response?.data?.error || error.response?.data?.message || error.message || 'Failed to generate predictions'
    
    appStore.addNotification({
      type: 'error',
      title: 'Prediction Failed',
      message: errorMessage
    })
    
    console.error('Failed to generate predictions:', error)
  } finally {
    isLoading.value = false
    appStore.setLoading(false)
  }
}

// Load stored predictions (simple)
const loadStoredPredictions = async () => {
  isLoading.value = true
  appStore.setLoading(true)
  paginatedMode.value = false
  paginationData.value = null
  
  try {
    const response = await api.get(`/predictions/stored?count=${predictionCount.value}`)
    predictions.value = response.data
    
    if (predictions.value.length > 0) {
      appStore.addNotification({
        type: 'info',
        title: 'Stored Predictions Loaded',
        message: `Loaded ${predictions.value.length} stored predictions`
      })
    }
  } catch (error: any) {
    // If no stored predictions, that's okay - just show empty state
    if (error.response?.status !== 404) {
      const errorMessage = error.response?.data?.error || error.response?.data?.message || error.message || 'Failed to load stored predictions'
      
      appStore.addNotification({
        type: 'error',
        title: 'Load Failed',
        message: errorMessage
      })
      
      console.error('Failed to load stored predictions:', error)
    }
  } finally {
    isLoading.value = false
    appStore.setLoading(false)
  }
}

// Load paginated predictions
const loadPaginatedPredictions = async (page: number = 1) => {
  isLoading.value = true
  appStore.setLoading(true)
  paginatedMode.value = true
  currentPage.value = page
  
  try {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.value.toString(),
      sortBy: 'createdAt',
      sortDirection: 'desc'
    })
    
    const response = await api.get(`/predictions/stored/paginated?${params}`)
    const data = response.data
    
    predictions.value = data.items
    paginationData.value = data.metadata || {
      page: data.page,
      pageSize: data.pageSize,
      totalItems: data.totalItems,
      totalPages: data.totalPages,
      hasPreviousPage: data.hasPreviousPage,
      hasNextPage: data.hasNextPage,
      firstItemIndex: data.firstItemIndex,
      lastItemIndex: data.lastItemIndex
    }
    
    if (predictions.value.length > 0) {
      appStore.addNotification({
        type: 'info',
        title: 'Paginated Predictions Loaded',
        message: `Loaded page ${page} of ${paginationData.value.totalPages} (${paginationData.value.totalItems} total predictions)`
      })
    }
  } catch (error: any) {
    const errorMessage = error.response?.data?.error || error.response?.data?.message || error.message || 'Failed to load paginated predictions'
    
    appStore.addNotification({
      type: 'error',
      title: 'Load Failed',
      message: errorMessage
    })
    
    console.error('Failed to load paginated predictions:', error)
    
    // Reset pagination state on error
    paginatedMode.value = false
    paginationData.value = null
    predictions.value = []
  } finally {
    isLoading.value = false
    appStore.setLoading(false)
  }
}

// Pagination navigation
const goToPage = (page: number) => {
  if (page >= 1 && paginationData.value && page <= paginationData.value.totalPages) {
    loadPaginatedPredictions(page)
  }
}

// Helper functions
const getDisplayIndex = (index: number): number => {
  if (paginatedMode.value && paginationData.value) {
    return paginationData.value.firstItemIndex + index - 1
  }
  return index + 1
}

const formatDate = (dateString: string): string => {
  if (!dateString) return 'N/A'
  
  try {
    const date = new Date(dateString)
    return date.toLocaleDateString('en-NZ', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    })
  } catch {
    return 'Invalid Date'
  }
}

// Component mounted - no automatic loading
onMounted(() => {
  // Don't automatically load predictions - wait for user action
  // User can click "Generate New" or "Load Stored" buttons
})
</script>

<style scoped>
.predictions {
  padding: 2rem;
  max-width: 1000px;
  margin: 0 auto;
}

h1 {
  font-size: 2.5rem;
  margin-bottom: 1rem;
  color: var(--color-heading);
}

.prediction-controls {
  display: flex;
  align-items: center;
  gap: 2rem;
  margin: 2rem 0;
  padding: 1.5rem;
  background: var(--color-background-soft);
  border-radius: 8px;
  flex-wrap: wrap;
}

@media (max-width: 768px) {
  .prediction-controls {
    flex-direction: column;
    align-items: stretch;
    gap: 1rem;
  }
  
  .control-group {
    justify-content: center;
  }
  
  .button-group {
    justify-content: center;
  }
}

.control-group {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.control-group label {
  font-weight: 500;
  color: var(--color-heading);
}

.prediction-select {
  padding: 0.5rem;
  border: 1px solid var(--color-border);
  border-radius: 4px;
  background: var(--color-background);
  color: var(--color-text);
  font-size: 1rem;
}

.button-group {
  display: flex;
  gap: 1rem;
}

.generate-button {
  border: none;
  padding: 0.75rem 1.5rem;
  border-radius: 6px;
  cursor: pointer;
  font-size: 1rem;
  transition: all 0.3s ease;
  font-weight: 500;
}

.generate-button.primary {
  background: #42b883;
  color: white;
}

.generate-button.primary:hover:not(:disabled) {
  background: #369870;
}

.generate-button.secondary {
  background: var(--color-background-soft);
  color: var(--color-text);
  border: 1px solid var(--color-border);
}

.generate-button.secondary:hover:not(:disabled) {
  background: var(--color-background-mute);
}

.generate-button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.predictions-table {
  margin-top: 2rem;
}

.predictions-table h3 {
  color: var(--color-heading);
  margin-bottom: 1rem;
}

table {
  width: 100%;
  border-collapse: collapse;
  background: var(--color-background);
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

th, td {
  padding: 1rem;
  text-align: left;
  border-bottom: 1px solid var(--color-border);
}

th {
  background: var(--color-background-soft);
  font-weight: 600;
  color: var(--color-heading);
}

.prediction-row:hover {
  background: var(--color-background-soft);
}

.numbers-cell {
  display: flex;
  gap: 0.5rem;
  flex-wrap: wrap;
}

.number-ball {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 2rem;
  height: 2rem;
  background: #42b883;
  color: white;
  border-radius: 50%;
  font-weight: bold;
  font-size: 0.9rem;
}

.source-cell {
  font-weight: 500;
  color: var(--color-text);
}

.score-cell {
  font-family: monospace;
  color: var(--color-text);
}

.date-cell {
  font-size: 0.9rem;
  color: var(--color-text);
  white-space: nowrap;
}

/* Pagination Controls */
.pagination-controls {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin: 2rem 0;
  padding: 1rem;
  background: var(--color-background-soft);
  border-radius: 8px;
  flex-wrap: wrap;
  gap: 1rem;
}

@media (max-width: 768px) {
  .pagination-controls {
    flex-direction: column;
    align-items: stretch;
  }
  
  .pagination-buttons {
    justify-content: center;
  }
  
  .pagination-info,
  .page-size-control {
    text-align: center;
  }
}

.pagination-info {
  font-size: 0.9rem;
  color: var(--color-text);
}

.pagination-buttons {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.pagination-button {
  padding: 0.5rem 1rem;
  border: 1px solid var(--color-border);
  background: var(--color-background);
  color: var(--color-text);
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.9rem;
  transition: all 0.3s ease;
}

.pagination-button:hover:not(:disabled) {
  background: var(--color-background-soft);
  border-color: #42b883;
}

.pagination-button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.page-info {
  margin: 0 1rem;
  font-weight: 500;
  color: var(--color-heading);
}

.page-size-control {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.page-size-control label {
  font-size: 0.9rem;
  color: var(--color-text);
}

.no-predictions {
  text-align: center;
  padding: 3rem;
  color: var(--color-text);
}

.loading-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 3rem;
  gap: 1rem;
}

.loading-spinner {
  width: 2rem;
  height: 2rem;
  border: 3px solid var(--color-border);
  border-top: 3px solid #42b883;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}
</style>