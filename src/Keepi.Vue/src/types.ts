export const loggableDays: string[] = [
  'maandag',
  'dinsdag',
  'woensdag',
  'donderdag',
  'vrijdag',
  'zaterdag',
  'zondag',
] as const
export type LoggableDay = (typeof loggableDays)[number]

export type TimeTableEntry = {
  invoiceItemId: number
  dayIndex: number
  minutes: number
}

export class ApiError {
  type: ApiErrorType

  constructor(type: ApiErrorType) {
    this.type = type
  }
}

export type ApiErrorType = 'badrequest' | 'unauthorized' | 'forbidden' | 'unknown'

export type InvoiceItem = {
  id: number
  name: string
  ordinal: number
  color: string
  projectId: number
  projectName: string
}

export type UserRole = 'admin' | 'user'
