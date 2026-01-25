import type { UserPermission } from '@/api-client'
import type { UserRole } from '@/types'

export function isAdmin(user: {
  entriesPermission: UserPermission
  exportsPermission: UserPermission
  projectsPermission: UserPermission
  usersPermission: UserPermission
}) {
  return (
    user.entriesPermission === 'readAndModify' &&
    user.exportsPermission === 'readAndModify' &&
    user.projectsPermission === 'readAndModify' &&
    user.usersPermission === 'readAndModify'
  )
}

export function isUser(user: {
  entriesPermission: UserPermission
  exportsPermission: UserPermission
  projectsPermission: UserPermission
  usersPermission: UserPermission
}) {
  return (
    user.entriesPermission === 'readAndModify' &&
    user.exportsPermission === 'none' &&
    user.projectsPermission === 'none' &&
    user.usersPermission === 'none'
  )
}

export function getUserRole(user: {
  entriesPermission: UserPermission
  exportsPermission: UserPermission
  projectsPermission: UserPermission
  usersPermission: UserPermission
}): UserRole {
  if (isAdmin(user)) {
    return 'admin'
  }
  if (isUser(user)) {
    return 'user'
  }
  return 'none'
}
