import { getUserRole, isAdmin, isUser } from '@/user-roles'
import { describe, expect, test } from 'vitest'

describe.concurrent('user-roles', () => {
  test('isAdmin', () => {
    expect(
      isAdmin({
        entriesPermission: 'readAndModify',
        exportsPermission: 'readAndModify',
        projectsPermission: 'readAndModify',
        usersPermission: 'readAndModify',
      }),
    ).toEqual(true)

    expect(
      isAdmin({
        entriesPermission: 'readAndModify',
        exportsPermission: 'none',
        projectsPermission: 'none',
        usersPermission: 'none',
      }),
    ).toEqual(false)

    expect(
      isAdmin({
        entriesPermission: 'none',
        exportsPermission: 'none',
        projectsPermission: 'none',
        usersPermission: 'none',
      }),
    ).toEqual(false)
  })

  test('isUser', () => {
    expect(
      isUser({
        entriesPermission: 'readAndModify',
        exportsPermission: 'none',
        projectsPermission: 'none',
        usersPermission: 'none',
      }),
    ).toEqual(true)

    expect(
      isUser({
        entriesPermission: 'readAndModify',
        exportsPermission: 'readAndModify',
        projectsPermission: 'readAndModify',
        usersPermission: 'readAndModify',
      }),
    ).toEqual(false)

    expect(
      isUser({
        entriesPermission: 'none',
        exportsPermission: 'none',
        projectsPermission: 'none',
        usersPermission: 'none',
      }),
    ).toEqual(false)
  })

  test('getUserRole', () => {
    expect(
      getUserRole({
        entriesPermission: 'readAndModify',
        exportsPermission: 'readAndModify',
        projectsPermission: 'readAndModify',
        usersPermission: 'readAndModify',
      }),
    ).toEqual('admin')

    expect(
      getUserRole({
        entriesPermission: 'readAndModify',
        exportsPermission: 'none',
        projectsPermission: 'none',
        usersPermission: 'none',
      }),
    ).toEqual('user')

    expect(
      getUserRole({
        entriesPermission: 'none',
        exportsPermission: 'none',
        projectsPermission: 'none',
        usersPermission: 'none',
      }),
    ).toEqual('none')

    // Unknown combinations should always return none
    expect(
      getUserRole({
        entriesPermission: 'readAndModify',
        exportsPermission: 'readAndModify',
        projectsPermission: 'readAndModify',
        usersPermission: 'none',
      }),
    ).toEqual('none')

    expect(
      getUserRole({
        entriesPermission: 'read',
        exportsPermission: 'read',
        projectsPermission: 'read',
        usersPermission: 'read',
      }),
    ).toEqual('none')
  })
})
