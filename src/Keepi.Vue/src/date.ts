export type DateRange = { weekNumber: number; year: number; dates: Date[] }

export function getWeekDaysForDate(d: Date): DateRange {
  const weekNumber = getWeekNumber(d)
  const dates: Date[] = []

  // Prior days in the week
  for (let i = 1; i < 7; ++i) {
    const candidate = new Date(d) // nice pun
    candidate.setDate(d.getDate() - i)
    if (getWeekNumber(candidate) === weekNumber) {
      dates.push(candidate)
    } else {
      break
    }
  }

  dates.reverse()
  dates.push(d)

  for (let i = 1; i < 7; ++i) {
    const candidate = new Date(d)
    candidate.setDate(d.getDate() + i)
    if (getWeekNumber(candidate) === weekNumber) {
      dates.push(candidate)
    } else {
      break
    }
  }

  if (dates.length !== 7) {
    throw new Error(`The calculated week must always be 7 but was ${dates.length}`)
  }

  return {
    dates: dates,
    weekNumber: weekNumber,
    year: dates.map((d) => d.getFullYear()).sort()[0],
  }
}

// Courtesy of https://stackoverflow.com/a/16591175
/**
 * Get the date from an ISO 8601 week and year
 *
 * https://en.wikipedia.org/wiki/ISO_week_date
 *
 * @param {number} week ISO 8601 week number
 * @param {number} year ISO year
 */
export function getDateOfIsoWeek(week: number, year: number) {
  if (week < 1 || week > 53) {
    throw new RangeError('ISO 8601 weeks are numbered from 1 to 53')
  } else if (!Number.isInteger(week)) {
    throw new TypeError('Week must be an integer')
  } else if (!Number.isInteger(year)) {
    throw new TypeError('Year must be an integer')
  }

  const simple = new Date(year, 0, 1 + (week - 1) * 7)
  const dayOfWeek = simple.getDay()
  const isoWeekStart = simple

  // Get the Monday past, and add a week if the day was
  // Friday, Saturday or Sunday.

  isoWeekStart.setDate(simple.getDate() - dayOfWeek + 1)
  if (dayOfWeek > 4) {
    isoWeekStart.setDate(isoWeekStart.getDate() + 7)
  }

  // The latest possible ISO week starts on December 28 of the current year.
  if (
    isoWeekStart.getFullYear() > year ||
    (isoWeekStart.getFullYear() == year &&
      isoWeekStart.getMonth() == 11 &&
      isoWeekStart.getDate() > 28)
  ) {
    throw new RangeError(`${year} has no ISO week ${week}`)
  }

  return isoWeekStart
}

// https://stackoverflow.com/a/6117889
export function getWeekNumber(d: Date): number {
  // Copy date so don't modify original
  d = new Date(Date.UTC(d.getFullYear(), d.getMonth(), d.getDate()))
  // Set to nearest Thursday: current date + 4 - current day number
  // Make Sunday's day number 7
  d.setUTCDate(d.getUTCDate() + 4 - (d.getUTCDay() || 7))
  // Get first day of year
  const yearStart = new Date(Date.UTC(d.getUTCFullYear(), 0, 1))
  // Calculate full weeks to nearest Thursday
  return Math.ceil(((d.getTime() - yearStart.getTime()) / 86400000 + 1) / 7)
}

export function formatDateAsTwoLetterDayName(date: Date): string {
  return date
    .toLocaleDateString('nl-NL', {
      weekday: 'long',
    })
    .substring(0, 2)
}

export function areDatesEqual(d1: Date, d2: Date): boolean {
  return (
    d1.getFullYear() === d2.getFullYear() &&
    d1.getMonth() === d2.getMonth() &&
    d1.getDate() === d2.getDate()
  )
}
