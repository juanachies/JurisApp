const SEPARATOR = /\|(?:\s*:?-{3,}:?\s*\|)+/

/** Turns a one-line GFM table into one row per line so it can be rendered. */
export function normalizeMarkdownTables(markdown: string): string {
  const lines = markdown.replace(/\r\n/g, '\n').split('\n')
  const out: string[] = []
  let inFence = false

  for (const line of lines) {
    if (/^\s*(```|~~~)/.test(line)) {
      inFence = !inFence
      out.push(line)
      continue
    }

    out.push(...(inFence ? [line] : expandJammedTable(line)))
  }

  return out.join('\n')
}

function expandJammedTable(line: string): string[] {
  const match = SEPARATOR.exec(line)
  if (!match || match.index === undefined) return [line]

  const before = line.slice(0, match.index).trim()
  const separator = match[0].trim()
  const after = line.slice(match.index + match[0].length).trim()

  if (!before.startsWith('|')) return [line]

  const header = cellsOf(before)
  if (header.length < 2 || header.some((cell) => cell.length === 0)) return [line]

  const separatorRow = separator.startsWith('|') ? separator : `|${separator}`
  if (!after) return [formatRow(header), separatorRow]

  const body = rowsFromBody(after, header.length)
  if (!body) return [line]

  return [formatRow(header), separatorRow, ...body.map(formatRow)]
}

function cellsOf(fragment: string): string[] {
  let text = fragment.trim()
  if (text.startsWith('|')) text = text.slice(1)
  if (text.endsWith('|')) text = text.slice(0, -1)
  return text.split('|').map((cell) => cell.trim())
}

function rowsFromBody(after: string, columns: number): string[][] | null {
  const rows: string[][] = []
  let current: string[] = []

  for (const cell of cellsOf(after)) {
    if (cell.length === 0) {
      if (current.length === 0) continue
      if (current.length === columns) {
        rows.push(current)
        current = []
        continue
      }
      current.push(cell)
      continue
    }

    current.push(cell)
    if (current.length === columns) {
      rows.push(current)
      current = []
    }
  }

  if (current.length > 0 || rows.length === 0) return null
  return rows
}

function formatRow(cells: string[]): string {
  return `| ${cells.join(' | ')} |`
}
