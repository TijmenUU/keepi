import {
  areDatesEqual,
  formatDateAsTwoLetterDayName,
  getDateOfIsoWeek,
  getWeekDaysForDate,
  getWeekNumber,
} from '@/date'
import { describe, expect, test } from 'vitest'

describe.concurrent('format', () => {
  test('getWeekDaysForDate', () => {
    expect(getWeekDaysForDate(new Date(2024, 0, 28))).toEqual({
      dates: [
        new Date(2024, 0, 22),
        new Date(2024, 0, 23),
        new Date(2024, 0, 24),
        new Date(2024, 0, 25),
        new Date(2024, 0, 26),
        new Date(2024, 0, 27),
        new Date(2024, 0, 28),
      ],
      weekNumber: 4,
      year: 2024,
    })

    expect(getWeekDaysForDate(new Date(2024, 0, 25))).toEqual({
      dates: [
        new Date(2024, 0, 22),
        new Date(2024, 0, 23),
        new Date(2024, 0, 24),
        new Date(2024, 0, 25),
        new Date(2024, 0, 26),
        new Date(2024, 0, 27),
        new Date(2024, 0, 28),
      ],
      weekNumber: 4,
      year: 2024,
    })

    expect(getWeekDaysForDate(new Date(2024, 0, 22))).toEqual({
      dates: [
        new Date(2024, 0, 22),
        new Date(2024, 0, 23),
        new Date(2024, 0, 24),
        new Date(2024, 0, 25),
        new Date(2024, 0, 26),
        new Date(2024, 0, 27),
        new Date(2024, 0, 28),
      ],
      weekNumber: 4,
      year: 2024,
    })

    expect(getWeekDaysForDate(new Date(2022, 0, 1))).toEqual({
      dates: [
        new Date(2021, 11, 27),
        new Date(2021, 11, 28),
        new Date(2021, 11, 29),
        new Date(2021, 11, 30),
        new Date(2021, 11, 31),
        new Date(2022, 0, 1),
        new Date(2022, 0, 2),
      ],
      weekNumber: 52,
      year: 2021,
    })
  })

  test('getWeekNumber', () => {
    expect(getWeekNumber(new Date(2021, 11, 31))).toBe(52)
    expect(getWeekNumber(new Date(2022, 0, 1))).toBe(52)

    expect(getWeekNumber(new Date(2023, 11, 31))).toBe(52)

    expect(getWeekNumber(new Date(2024, 0, 1))).toBe(1)
    expect(getWeekNumber(new Date(2024, 0, 7))).toBe(1)
    expect(getWeekNumber(new Date(2024, 0, 14))).toBe(2)
    expect(getWeekNumber(new Date(2024, 0, 21))).toBe(3)
    expect(getWeekNumber(new Date(2024, 0, 28))).toBe(4)
  })

  test('formatDateAsTwoLetterDayName', () => {
    expect(formatDateAsTwoLetterDayName(new Date('2025-04-07T00:00:00'))).toBe('ma')
    expect(formatDateAsTwoLetterDayName(new Date('2025-04-08T00:00:00'))).toBe('di')
    expect(formatDateAsTwoLetterDayName(new Date('2025-04-09T00:00:00'))).toBe('wo')
    expect(formatDateAsTwoLetterDayName(new Date('2025-04-10T00:00:00'))).toBe('do')
    expect(formatDateAsTwoLetterDayName(new Date('2025-04-11T00:00:00'))).toBe('vr')
    expect(formatDateAsTwoLetterDayName(new Date('2025-04-12T00:00:00'))).toBe('za')
    expect(formatDateAsTwoLetterDayName(new Date('2025-04-13T00:00:00'))).toBe('zo')
  })

  test('areDatesEqual', () => {
    expect(areDatesEqual(new Date('2025-04-07T00:00:00'), new Date('2025-04-07T00:00:00'))).toBe(
      true,
    )

    expect(areDatesEqual(new Date('2025-04-07T23:59:59'), new Date('2025-04-07T00:00:00'))).toBe(
      true,
    )

    expect(areDatesEqual(new Date('2025-04-08T00:00:00'), new Date('2025-04-07T00:00:00'))).toBe(
      false,
    )

    expect(areDatesEqual(new Date('2025-05-07T00:00:00'), new Date('2025-04-07T00:00:00'))).toBe(
      false,
    )

    expect(areDatesEqual(new Date('2024-04-07T00:00:00'), new Date('2025-04-07T00:00:00'))).toBe(
      false,
    )
  })

  test('getDateOfIsoWeek', () => {
    expect(formatAsIsoDate(getDateOfIsoWeek(53, 1976))).toBe('1976-12-27')
    expect(formatAsIsoDate(getDateOfIsoWeek(1, 1978))).toBe('1978-01-02')
    expect(formatAsIsoDate(getDateOfIsoWeek(1, 1980))).toBe('1979-12-31')
    expect(formatAsIsoDate(getDateOfIsoWeek(53, 2020))).toBe('2020-12-28')
    expect(formatAsIsoDate(getDateOfIsoWeek(1, 2021))).toBe('2021-01-04')
  })
})

function formatAsIsoDate(d: Date): string {
  return `${d.getFullYear().toString().padStart(4, '0')}-${(d.getMonth() + 1).toString().padStart(2, '0')}-${d.getDate().toString().padStart(2, '0')}`
}
