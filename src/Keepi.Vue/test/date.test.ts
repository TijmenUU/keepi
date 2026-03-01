import {
  areDatesEqual,
  formatDateAsTwoLetterDayName,
  getDateOfIsoWeek,
  getWeekDaysForDate,
  getWeekNumber,
  toShortDutchDate,
  toShortIsoDate,
  tryParseDutchDate,
  tryParseIsoDate,
} from '@/date'
import { describe, expect, test } from 'vitest'

describe.concurrent('date', () => {
  test('toShortIsoDate', () => {
    expect(toShortIsoDate(new Date(2020, 2, 28, 4))).toBe('2020-03-28')
    expect(toShortIsoDate(new Date(2020, 11, 31, 4))).toBe('2020-12-31')
  })

  test('toShortDutchDate', () => {
    expect(toShortDutchDate(new Date(2020, 1, 28))).toBe('28-02-2020')
    expect(toShortDutchDate(new Date(2020, 11, 31))).toBe('31-12-2020')
  })

  test('tryParseDutchDate', () => {
    expect(tryParseDutchDate('')).toBe(null)
    expect(tryParseDutchDate('1')).toBe(null)
    expect(tryParseDutchDate('1-1')).toBe(null)
    expect(tryParseDutchDate('1-1-abc')).toBe(null)
    expect(tryParseDutchDate('1-abc-2000')).toBe(null)
    expect(tryParseDutchDate('abc-1-2000')).toBe(null)
    expect(tryParseDutchDate('1a-1-2000')).toBe(null)
    expect(tryParseDutchDate('1-1a-2000')).toBe(null)
    expect(tryParseDutchDate('1-1-2000a')).toBe(null)
    expect(tryParseDutchDate('29-2-2025')).toBe(null)

    expectLocalDatesToBeEqual(tryParseDutchDate('1-1-2000'), new Date(2000, 0, 1))
    expectLocalDatesToBeEqual(tryParseDutchDate('01-01-2000'), new Date(2000, 0, 1))
    expectLocalDatesToBeEqual(tryParseDutchDate('31-12-2025'), new Date(2025, 11, 31))
    expectLocalDatesToBeEqual(tryParseDutchDate('29-2-2024'), new Date(2024, 1, 29))
    expectLocalDatesToBeEqual(tryParseDutchDate('29-02-2024'), new Date(2024, 1, 29))
  })

  test('tryParseIsoDate', () => {
    expect(tryParseIsoDate('')).toBe(null)
    expect(tryParseIsoDate('1')).toBe(null)
    expect(tryParseIsoDate('1-1')).toBe(null)
    expect(tryParseIsoDate('1-1-abc')).toBe(null)
    expect(tryParseIsoDate('1-abc-2000')).toBe(null)
    expect(tryParseIsoDate('abc-1-2000')).toBe(null)
    expect(tryParseIsoDate('1a-1-2000')).toBe(null)
    expect(tryParseIsoDate('1-1a-2000')).toBe(null)
    expect(tryParseIsoDate('1-1-2000a')).toBe(null)
    expect(tryParseIsoDate('28-2-2025')).toBe(null)
    expect(tryParseIsoDate('28-2-2025')).toBe(null)
    expect(tryParseIsoDate('2025-2-29')).toBe(null)

    expectLocalDatesToBeEqual(tryParseIsoDate('2000-1-1'), new Date(2000, 0, 1))
    expectLocalDatesToBeEqual(tryParseIsoDate('2000-01-01'), new Date(2000, 0, 1))
    expectLocalDatesToBeEqual(tryParseIsoDate('2025-12-31'), new Date(2025, 11, 31))
    expectLocalDatesToBeEqual(tryParseIsoDate('2024-2-29'), new Date(2024, 1, 29))
    expectLocalDatesToBeEqual(tryParseIsoDate('2024-02-29'), new Date(2024, 1, 29))
  })

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

function expectLocalDatesToBeEqual(actual: Date | null, expected: Date) {
  expect(actual).not.toBeNull()

  expect(actual?.getDate()).toBe(expected.getDate())
  expect(actual?.getMonth()).toBe(expected.getMonth())
  expect(actual?.getFullYear()).toBe(expected.getFullYear())
}

function formatAsIsoDate(d: Date): string {
  return `${d.getFullYear().toString().padStart(4, '0')}-${(d.getMonth() + 1).toString().padStart(2, '0')}-${d.getDate().toString().padStart(2, '0')}`
}
