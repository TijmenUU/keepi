import { tryParseDutchDate } from '@/format'
import { maxLength, required, withMessage } from '@regle/rules'

export const dateValidator = withMessage(isValidDate, 'Geen geldige datum')
export const requiredValidator = withMessage(required, 'Dit veld is verplicht')

export function isValidDate(value: unknown): boolean {
  if (value == null || typeof value !== 'string' || value === '') {
    return true
  }

  return tryParseDutchDate(value) != null
}

export function isValidString(value: unknown): boolean {
  return value !== null && typeof value === 'string'
}

export function hasMaxLength(length: number) {
  return withMessage(maxLength, `Maximale aantal karakters is ${length}`)(length)
}
