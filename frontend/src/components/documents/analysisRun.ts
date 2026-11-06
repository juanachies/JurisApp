import { useSyncExternalStore } from 'react'
import type { QueryClient } from '@tanstack/react-query'
import { documentsApi } from '@/api'
import { errorMessage } from '@/api/client'
import { queryKeys } from '@/api/queryKeys'
import type { DocumentAnalysisDto } from '@/types/api'

export type AnalysisRun = {
  status: 'queued' | 'running' | 'done' | 'error'
  error?: string
  analysis?: DocumentAnalysisDto
}

const runs = new Map<string, AnalysisRun>()
const listeners = new Set<() => void>()
const queue: { documentId: string; queryClient: QueryClient }[] = []
let active = 0
const MAX_PARALLEL = 2

function emit() {
  for (const listener of listeners) listener()
}

function subscribe(listener: () => void) {
  listeners.add(listener)
  return () => listeners.delete(listener)
}

export function readAnalysisRun(documentId: string) {
  return runs.get(documentId)
}

export function useAnalysisRun(documentId: string) {
  return useSyncExternalStore(
    subscribe,
    () => runs.get(documentId),
    () => undefined,
  )
}

function pump() {
  while (active < MAX_PARALLEL && queue.length > 0) {
    const job = queue.shift()
    if (!job) return
    const current = runs.get(job.documentId)
    if (!current || current.status === 'done' || current.status === 'error') continue

    active += 1
    runs.set(job.documentId, { status: 'running' })
    emit()

    documentsApi
      .analyze({ documentId: job.documentId })
      .then((analysis) => {
        runs.set(job.documentId, { status: 'done', analysis })
        job.queryClient.setQueryData(queryKeys.documentAnalysis(job.documentId), analysis)
        void job.queryClient.invalidateQueries({ queryKey: ['documents', 'chat'] })
        void job.queryClient.invalidateQueries({ queryKey: ['documents', 'folder'] })
      })
      .catch((err: unknown) => {
        runs.set(job.documentId, {
          status: 'error',
          error: errorMessage(err, 'No pudimos analizar el documento.'),
        })
      })
      .finally(() => {
        active -= 1
        emit()
        pump()
      })
  }
}

export function ensureCompleteAnalysis(documentId: string, queryClient: QueryClient) {
  const current = runs.get(documentId)
  if (current?.status === 'queued' || current?.status === 'running' || current?.status === 'done' || current?.status === 'error') {
    return
  }

  const cached = queryClient.getQueryData<DocumentAnalysisDto | null>(queryKeys.documentAnalysis(documentId))
  if (cached) {
    runs.set(documentId, { status: 'done', analysis: cached })
    emit()
    return
  }

  runs.set(documentId, { status: 'queued' })
  queue.push({ documentId, queryClient })
  emit()
  pump()
}

export function retryCompleteAnalysis(documentId: string, queryClient: QueryClient) {
  const current = runs.get(documentId)
  if (current?.status === 'queued' || current?.status === 'running') return
  runs.delete(documentId)
  ensureCompleteAnalysis(documentId, queryClient)
}
