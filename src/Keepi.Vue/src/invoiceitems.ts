import type { IGetUserProjectsResponse } from '@/api-client'
import type { InvoiceItem } from '@/types'

export function getSortedInvoiceItems(
  projects: IGetUserProjectsResponse['projects'],
): InvoiceItem[] {
  return projects
    .flatMap((p) => {
      return p.invoiceItems.map((i) => ({
        id: i.id,
        name: i.name,
        ordinal: i.ordinal,
        color: i.color ?? 'none',
        projectId: p.id,
        projectName: p.name,
      }))
    })
    .sort((a, b) => {
      if (a.ordinal != b.ordinal) {
        return a.ordinal - b.ordinal
      }

      if (a.projectName != b.projectName) {
        return a.projectName.localeCompare(b.projectName)
      }

      return a.name.localeCompare(b.name)
    })
}
