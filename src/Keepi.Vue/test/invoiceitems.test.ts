import { describe, expect, test } from 'vitest'
import { getSortedInvoiceItems } from '@/invoiceitems'

describe.concurrent('getSortedInvoiceItems', () => {
  test('empty', () => {
    expect(getSortedInvoiceItems([])).toEqual([])
  })

  test('sorted order', () => {
    expect(
      getSortedInvoiceItems([
        {
          id: 1,
          name: 'Alfa',
          enabled: true,
          invoiceItems: [
            { id: 10, name: 'Item 1', ordinal: 1, color: '#eab308' },
            { id: 11, name: 'Item 2', ordinal: 2, color: '#f97316' },
            { id: 12, name: 'Item 3', ordinal: 3, color: '#3b82f6' },
          ],
        },
      ]),
    ).toEqual([
      {
        id: 10,
        name: 'Item 1',
        ordinal: 1,
        color: '#eab308',
        projectId: 1,
        projectName: 'Alfa',
      },
      {
        id: 11,
        name: 'Item 2',
        ordinal: 2,
        color: '#f97316',
        projectId: 1,
        projectName: 'Alfa',
      },
      {
        id: 12,
        name: 'Item 3',
        ordinal: 3,
        color: '#3b82f6',
        projectId: 1,
        projectName: 'Alfa',
      },
    ])
  })

  test('inverse order', () => {
    expect(
      getSortedInvoiceItems([
        {
          id: 1,
          name: 'Alfa',
          enabled: true,
          invoiceItems: [
            { id: 10, name: 'Item 1', ordinal: 3, color: '#eab308' },
            { id: 11, name: 'Item 2', ordinal: 2, color: '#f97316' },
            { id: 12, name: 'Item 3', ordinal: 1, color: '#3b82f6' },
          ],
        },
      ]),
    ).toEqual([
      {
        id: 12,
        name: 'Item 3',
        ordinal: 1,
        color: '#3b82f6',
        projectId: 1,
        projectName: 'Alfa',
      },
      {
        id: 11,
        name: 'Item 2',
        ordinal: 2,
        color: '#f97316',
        projectId: 1,
        projectName: 'Alfa',
      },
      {
        id: 10,
        name: 'Item 1',
        ordinal: 3,
        color: '#eab308',
        projectId: 1,
        projectName: 'Alfa',
      },
    ])
  })

  test('random order', () => {
    expect(
      getSortedInvoiceItems([
        {
          id: 1,
          name: 'Alfa',
          enabled: true,
          invoiceItems: [
            { id: 10, name: 'Item 1', ordinal: 1, color: '#eab308' },
            { id: 11, name: 'Item 2', ordinal: 3, color: '#f97316' },
            { id: 12, name: 'Item 3', ordinal: 2, color: '#3b82f6' },
          ],
        },
      ]),
    ).toEqual([
      {
        id: 10,
        name: 'Item 1',
        ordinal: 1,
        color: '#eab308',
        projectId: 1,
        projectName: 'Alfa',
      },
      {
        id: 12,
        name: 'Item 3',
        ordinal: 2,
        color: '#3b82f6',
        projectId: 1,
        projectName: 'Alfa',
      },
      {
        id: 11,
        name: 'Item 2',
        ordinal: 3,
        color: '#f97316',
        projectId: 1,
        projectName: 'Alfa',
      },
    ])
  })

  test('multiple projects', () => {
    expect(
      getSortedInvoiceItems([
        {
          id: 1,
          name: 'Alfa',
          enabled: true,
          invoiceItems: [
            { id: 10, name: 'Item 1', ordinal: 1, color: '#eab308' },
            { id: 11, name: 'Item 2', ordinal: 3, color: '#f97316' },
          ],
        },
        {
          id: 2,
          name: 'Beta',
          enabled: true,
          invoiceItems: [
            { id: 12, name: 'Item 3', ordinal: 4, color: '#3b82f6' },
            { id: 13, name: 'Item 4', ordinal: 2, color: '#22c55e' },
          ],
        },
      ]),
    ).toEqual([
      {
        id: 10,
        name: 'Item 1',
        ordinal: 1,
        color: '#eab308',
        projectId: 1,
        projectName: 'Alfa',
      },
      {
        id: 13,
        name: 'Item 4',
        ordinal: 2,
        color: '#22c55e',
        projectId: 2,
        projectName: 'Beta',
      },
      {
        id: 11,
        name: 'Item 2',
        ordinal: 3,
        color: '#f97316',
        projectId: 1,
        projectName: 'Alfa',
      },
      {
        id: 12,
        name: 'Item 3',
        ordinal: 4,
        color: '#3b82f6',
        projectId: 2,
        projectName: 'Beta',
      },
    ])
  })

  test('fallback to project name', () => {
    expect(
      getSortedInvoiceItems([
        {
          id: 2,
          name: 'Beta',
          enabled: true,
          invoiceItems: [{ id: 11, name: 'Item 2', ordinal: 1, color: '#f97316' }],
        },
        {
          id: 1,
          name: 'Alfa',
          enabled: true,
          invoiceItems: [{ id: 10, name: 'Item 1', ordinal: 1, color: '#eab308' }],
        },
      ]),
    ).toEqual([
      {
        id: 10,
        name: 'Item 1',
        ordinal: 1,
        color: '#eab308',
        projectId: 1,
        projectName: 'Alfa',
      },
      {
        id: 11,
        name: 'Item 2',
        ordinal: 1,
        color: '#f97316',
        projectId: 2,
        projectName: 'Beta',
      },
    ])
  })

  test('fallback to name', () => {
    expect(
      getSortedInvoiceItems([
        {
          id: 1,
          name: 'Alfa',
          enabled: true,
          invoiceItems: [
            { id: 11, name: 'Item 2', ordinal: 1, color: '#eab308' },
            { id: 10, name: 'Item 1', ordinal: 1, color: '#f97316' },
          ],
        },
      ]),
    ).toEqual([
      {
        id: 10,
        name: 'Item 1',
        ordinal: 1,
        color: '#f97316',
        projectId: 1,
        projectName: 'Alfa',
      },
      {
        id: 11,
        name: 'Item 2',
        ordinal: 1,
        color: '#eab308',
        projectId: 1,
        projectName: 'Alfa',
      },
    ])
  })
})
