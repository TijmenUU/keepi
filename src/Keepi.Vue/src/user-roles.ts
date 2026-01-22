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

export function getUserRole(user: {
  entriesPermission: UserPermission
  exportsPermission: UserPermission
  projectsPermission: UserPermission
  usersPermission: UserPermission
}): UserRole {
  return isAdmin(user) ? 'admin' : 'user'
}
