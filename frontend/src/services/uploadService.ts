import api from './api'

export interface UploadProgress {
  loaded: number
  total: number
  percentage: number
}

export interface UploadResult {
  fileName: string
  success: boolean
  recordsAdded?: number
  recordsSkipped?: number
  error?: string
}

export interface DuplicateCheckResult {
  isDuplicate: boolean
  existingDrawNumber?: number
  message?: string
}

export class UploadService {
  private activeUploads = new Map<string, XMLHttpRequest>()

  /**
   * Validate file type, size, and format requirements
   */
  validateFile(file: File): string | null {
    const allowedTypes = ['.csv', '.txt', '.pdf']
    const fileExtension = '.' + file.name.split('.').pop()?.toLowerCase()
    
    if (!allowedTypes.includes(fileExtension)) {
      return `Invalid file type. Only CSV, TXT, and PDF files are allowed.`
    }
    
    if (file.size > 50 * 1024 * 1024) { // 50MB limit
      return `File size too large. Maximum size is 50MB.`
    }
    
    if (file.size === 0) {
      return `File is empty.`
    }
    
    return null
  }

  /**
   * Check for duplicates by analyzing the first CSV row
   */
  async checkForDuplicates(file: File): Promise<DuplicateCheckResult> {
    if (!file.name.toLowerCase().endsWith('.csv')) {
      return { isDuplicate: false }
    }

    try {
      // Read first few lines of CSV to extract draw number
      const text = await this.readFileFirstLines(file, 5)
      const lines = text.split('\n').filter(line => line.trim())
      
      if (lines.length < 2) {
        return { isDuplicate: false, message: 'CSV file appears to be empty or invalid' }
      }

      // Skip header row, get first data row
      const firstDataRow = lines[1]
      const columns = firstDataRow.split(',').map(col => col.trim().replace(/"/g, ''))
      
      // Assume first column is draw number
      const drawNumber = parseInt(columns[0])
      
      if (isNaN(drawNumber)) {
        return { isDuplicate: false, message: 'Could not extract draw number from CSV' }
      }

      // Check if draw exists in backend
      const response = await api.get(`/lotto/exists/${drawNumber}`)
      const exists = response.data === true || response.data.exists === true
      
      return {
        isDuplicate: exists,
        existingDrawNumber: drawNumber,
        message: exists ? `Draw ${drawNumber} already exists in database` : undefined
      }
    } catch (error: any) {
      console.warn('Duplicate check failed:', error.message)
      return { isDuplicate: false, message: 'Could not check for duplicates' }
    }
  }

  /**
   * Read first N lines of a file
   */
  private readFileFirstLines(file: File, maxLines: number): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader()
      const chunkSize = Math.min(file.size, 2048) // Read first 2KB
      const blob = file.slice(0, chunkSize)
      
      reader.onload = (e) => {
        const text = e.target?.result as string
        const lines = text.split('\n').slice(0, maxLines)
        resolve(lines.join('\n'))
      }
      
      reader.onerror = () => reject(new Error('Failed to read file'))
      reader.readAsText(blob)
    })
  }

  /**
   * Upload file with XMLHttpRequest for better progress monitoring
   */
  uploadFile(
    file: File,
    onProgress: (progress: UploadProgress) => void,
    onComplete: (result: UploadResult) => void,
    onError: (error: string) => void
  ): string {
    const uploadId = `upload_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`
    
    const xhr = new XMLHttpRequest()
    this.activeUploads.set(uploadId, xhr)

    // Determine endpoint based on file type
    const endpoint = file.name.toLowerCase().endsWith('.csv') ? '/lotto/upload' : '/combinations/upload'
    const url = `${import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api'}${endpoint}`

    // Create form data
    const formData = new FormData()
    formData.append('file', file)

    // Configure upload progress tracking
    xhr.upload.addEventListener('progress', (event) => {
      if (event.lengthComputable) {
        const progress: UploadProgress = {
          loaded: event.loaded,
          total: event.total,
          percentage: Math.round((event.loaded * 100) / event.total)
        }
        onProgress(progress)
      }
    })

    // Handle upload completion
    xhr.addEventListener('load', () => {
      this.activeUploads.delete(uploadId)
      
      if (xhr.status >= 200 && xhr.status < 300) {
        try {
          const response = JSON.parse(xhr.responseText)
          const result: UploadResult = {
            fileName: file.name,
            success: true,
            recordsAdded: response.recordsAdded || 0,
            recordsSkipped: response.recordsSkipped || 0
          }
          onComplete(result)
        } catch (error) {
          onError('Invalid response from server')
        }
      } else {
        try {
          const errorResponse = JSON.parse(xhr.responseText)
          onError(errorResponse.message || `Upload failed with status ${xhr.status}`)
        } catch {
          onError(`Upload failed with status ${xhr.status}`)
        }
      }
    })

    // Handle upload errors
    xhr.addEventListener('error', () => {
      this.activeUploads.delete(uploadId)
      onError('Network error occurred during upload')
    })

    // Handle upload abort
    xhr.addEventListener('abort', () => {
      this.activeUploads.delete(uploadId)
      onError('Upload was cancelled')
    })

    // Handle upload timeout
    xhr.addEventListener('timeout', () => {
      this.activeUploads.delete(uploadId)
      onError('Upload timed out')
    })

    // Configure and start upload
    xhr.open('POST', url)
    xhr.timeout = 300000 // 5 minutes timeout
    xhr.send(formData)

    return uploadId
  }

  /**
   * Cancel an active upload
   */
  cancelUpload(uploadId: string): boolean {
    const xhr = this.activeUploads.get(uploadId)
    if (xhr) {
      xhr.abort()
      this.activeUploads.delete(uploadId)
      return true
    }
    return false
  }

  /**
   * Retry a failed upload
   */
  retryUpload(
    file: File,
    onProgress: (progress: UploadProgress) => void,
    onComplete: (result: UploadResult) => void,
    onError: (error: string) => void
  ): string {
    // Simply create a new upload
    return this.uploadFile(file, onProgress, onComplete, onError)
  }

  /**
   * Get active upload count
   */
  getActiveUploadCount(): number {
    return this.activeUploads.size
  }

  /**
   * Cancel all active uploads
   */
  cancelAllUploads(): void {
    for (const [uploadId, xhr] of this.activeUploads) {
      xhr.abort()
    }
    this.activeUploads.clear()
  }
}

// Export singleton instance
export const uploadService = new UploadService()