import type { UserRole } from '@/types'

type UserContext = {
  id: number
  name: string
  authenticated: boolean
  role: UserRole
}

const context: UserContext = {
  id: -1,
  name: 'John Doe',
  authenticated: false,
  role: 'none',
}

export function getUserContext(): Readonly<UserContext> {
  return {
    id: context.id,
    name: context.name,
    authenticated: context.authenticated,
    role: context.role,
  }
}

export function setUserContext(id: number, name: string, role: UserRole) {
  context.id = id
  context.name = name
  context.role = role
  context.authenticated = true
}

export function clearUserContext() {
  context.id = -1
  context.name = 'John Doe'
  context.role = 'none'
  context.authenticated = false
}
