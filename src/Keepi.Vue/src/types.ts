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

export type UserEntryCategory = {
  order: number
  enabled: boolean
  id: number
  name: string
}

export type TimeTableEntry = {
  userEntryCategoryId: number
  dayIndex: number
  initialMinutes: number
  inputMinutes: number
}
