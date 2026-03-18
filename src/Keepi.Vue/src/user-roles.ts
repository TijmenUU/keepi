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

export function getUserRoleLabel(role: UserRole): string {
  switch (role) {
    case 'admin':
      return 'Beheerder'

    case 'user':
      return 'Gebruiker'

    case 'none':
      return 'Gedeactiveerd'

    default:
      console.error(`Missing role to label mapping for role ${role}`)
      return role
  }
}
