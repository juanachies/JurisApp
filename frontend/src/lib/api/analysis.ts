import { apiClient } from './client'
import type { AnalyzeSegmentedRequest, ApiError, SegmentedDocumentAnalysisDto } from './types'

export const analysisApi = {
  analyzeSegmented: (data: AnalyzeSegmentedRequest) =>
    apiClient<SegmentedDocumentAnalysisDto>('/api/analysis/segmented', {
      method: 'POST',
      body: JSON.stringify(data),
    }),

  getByDocument: async (documentId: string): Promise<SegmentedDocumentAnalysisDto | null> => {
    try {
      return await apiClient<SegmentedDocumentAnalysisDto>(
        `/api/documents/${documentId}/analysis`,
      )
    } catch (error) {
      if ((error as ApiError).status === 404) return null
      throw error
    }
  },
}
