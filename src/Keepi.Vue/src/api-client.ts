import { toShortIsoDate } from '@/date'
import { ApiError } from '@/types'
import { fromPromise, err, ok, ResultAsync, okAsync } from 'neverthrow'

export interface IApiClient {
  getUser(): ResultAsync<IGetUserResponse, ApiError>
  updateAntiForgeryToken(): ResultAsync<void, ApiError>
  getAllUsers(): ResultAsync<IGetAllUsersResponse, ApiError>
  updateUserPermissions(
    id: number,
    value: IUpdateUserPermissionsRequest,
  ): ResultAsync<void, ApiError>
  createProject(value: ICreateProjectRequest): ResultAsync<ICreateProjectResponse, ApiError>
  deleteProject(id: number): ResultAsync<void, ApiError>
  getAllProjects(): ResultAsync<IGetAllProjectsResponse, ApiError>
  updateProject(id: number, value: IUpdateProjectRequest): ResultAsync<void, ApiError>
  getUserProjects(): ResultAsync<IGetUserProjectsResponse, ApiError>
  getWeekUserEntries(
    year: number,
    weekNumber: number,
  ): ResultAsync<IGetWeekUserEntriesResponse, ApiError>
  updateWeekUserEntries(
    year: number,
    weekNumber: number,
    value: IPutUpdateWeekUserEntriesRequest,
  ): ResultAsync<void, ApiError>
  getUserEntriesExport(
    from: Date,
    to: Date,
  ): ResultAsync<{ blob: Blob; fileName: string }, ApiError>
  updateAllUserInvoiceItemCustomizations(
    value: IUpdateAllUserInvoiceItemCustomizationsRequest,
  ): ResultAsync<void, ApiError>
}

class ApiClient implements IApiClient {
  private antiForgeryHeaderName: string = ''
  private antiForgeryToken: string = ''

  public getUser() {
    const options = this.getBaseRequestOptions({
      method: 'GET',
    })

    return this.makeRequest('/user', options).andThen((result) => {
      return ApiClient.deserializeJson<IGetUserResponse>(result)
    })
  }

  public updateAntiForgeryToken(): ResultAsync<void, ApiError> {
    if (this.antiForgeryToken === '' || this.antiForgeryHeaderName === '') {
      return fromPromise(fetch('/antiforgerytoken'), (error) => {
        console.error('Failed to retrieve anti forgery token', error)
        return new ApiError('unknown')
      }).andThen((response) => {
        if (response.status === 200) {
          return fromPromise(response.json(), (error) => {
            console.error('Received malformed anti forgery response body', error)
            return new ApiError('unknown')
          }).andThen((model) => {
            if (
              model.headerFieldName != null &&
              model.token != null &&
              typeof model.headerFieldName === 'string' &&
              typeof model.token === 'string'
            ) {
              this.antiForgeryHeaderName = model.headerFieldName
              this.antiForgeryToken = model.token
              return ok()
            }

            console.error('Received malformed anti forgery response model', model)
            return err(new ApiError('unknown'))
          })
        }

        console.error('Failed to retrieve anti forgery token', response.status)
        return err(new ApiError('unknown'))
      })
    } else {
      return okAsync()
    }
  }

  public getAllUsers() {
    const options = this.getBaseRequestOptions({
      method: 'GET',
    })

    return this.makeRequest('/users', options).andThen((result) => {
      return ApiClient.deserializeJson<IGetAllUsersResponse>(result)
    })
  }

  public updateUserPermissions(id: number, value: IUpdateUserPermissionsRequest) {
    const options = this.getBaseRequestOptions({
      method: 'PUT',
      json: JSON.stringify(value),
    })

    return this.makeRequest(`/users/${id}/permissions`, options, [204]).andThen(() => {
      return ok()
    })
  }

  public createProject(value: ICreateProjectRequest) {
    const options = this.getBaseRequestOptions({
      method: 'POST',
      json: JSON.stringify(value),
    })

    return this.makeRequest('/projects', options, [200]).andThen((result) => {
      return ApiClient.deserializeJson<ICreateProjectResponse>(result)
    })
  }

  public deleteProject(id: number) {
    const options = this.getBaseRequestOptions({
      method: 'DELETE',
    })

    return this.makeRequest(`/projects/${id}`, options, [204]).andThen(() => {
      return ok()
    })
  }

  public getAllProjects() {
    const options = this.getBaseRequestOptions({
      method: 'GET',
    })

    return this.makeRequest('/projects', options, [200]).andThen((result) => {
      return ApiClient.deserializeJson<IGetAllProjectsResponse>(result)
    })
  }

  public updateProject(id: number, value: IUpdateProjectRequest) {
    const options = this.getBaseRequestOptions({
      method: 'PUT',
      json: JSON.stringify(value),
    })

    return this.makeRequest(`/projects/${id}`, options, [204]).andThen(() => {
      return ok()
    })
  }

  public getUserProjects() {
    const options = this.getBaseRequestOptions({
      method: 'GET',
    })

    return this.makeRequest('/user/projects', options, [200]).andThen((result) => {
      return ApiClient.deserializeJson<IGetUserProjectsResponse>(result)
    })
  }

  public getWeekUserEntries(year: number, weekNumber: number) {
    const options = this.getBaseRequestOptions({
      method: 'GET',
    })

    return this.makeRequest(
      `/user/entries/year/${year}/week/${weekNumber}`,
      options,
      [200],
    ).andThen((result) => {
      return ApiClient.deserializeJson<IGetWeekUserEntriesResponse>(result)
    })
  }

  public updateWeekUserEntries(
    year: number,
    weekNumber: number,
    value: IPutUpdateWeekUserEntriesRequest,
  ) {
    const options = this.getBaseRequestOptions({
      method: 'PUT',
      json: JSON.stringify(value),
    })

    return this.makeRequest(
      `/user/entries/year/${year}/week/${weekNumber}`,
      options,
      [201],
    ).andThen(() => {
      return ok()
    })
  }

  public getUserEntriesExport(
    from: Date,
    to: Date,
  ): ResultAsync<{ blob: Blob; fileName: string }, ApiError> {
    const options = this.getBaseRequestOptions({
      method: 'POST',
      json: JSON.stringify({
        start: toShortIsoDate(from),
        stop: toShortIsoDate(to),
      }),
    })

    return this.makeRequest(`/export/userentries`, options, [200]).andThen((result) =>
      fromPromise(result.blob(), (reason) => {
        console.error('Failed to retrieve blob from response body', reason)
        return new ApiError('unknown')
      }).map((blob) => ({
        blob,
        fileName: ApiClient.getFileNameFromHeaderOrFallback(result.headers, 'export.csv'),
      })),
    )
  }

  private static getFileNameFromHeaderOrFallback(headers: Headers, fallback: string) {
    // Example expected header value:
    // attachment; filename=export_2025-10-20_2025-10-26.csv; filename*=UTF-8''export_2025-10-20_2025-10-26.csv
    const contentDisposition = headers.get('Content-Disposition')
    if (contentDisposition == null) {
      console.warn('Missing content disposition header')
      return fallback
    }

    const contentDispositionParts = contentDisposition.split(';').map((p) => p.trim())
    const fileNamePartKey = 'filename='
    const fileNamePart = contentDispositionParts.find((p) => p.startsWith(fileNamePartKey))
    if (fileNamePart == null) {
      console.warn('Missing file name part in content disposition header', contentDisposition)
      return fallback
    }

    return fileNamePart.substring(fileNamePartKey.length)
  }

  public updateAllUserInvoiceItemCustomizations(
    value: IUpdateAllUserInvoiceItemCustomizationsRequest,
  ) {
    const options = this.getBaseRequestOptions({
      method: 'PUT',
      json: JSON.stringify(value),
    })

    return this.makeRequest('/user/invoiceitemcustomizations', options, [204]).andThen(() => {
      return ok()
    })
  }

  private makeRequest(
    subpath: string,
    options?: RequestInit,
    supportedResponseCodes?: number[],
  ): ResultAsync<Response, ApiError> {
    const path = subpath.startsWith('/') ? `/api${subpath}` : `/api/${subpath}`

    let antiForgeryTokenPromise: ResultAsync<void, ApiError> = okAsync()
    if (options?.method == null || options.method !== 'GET') {
      antiForgeryTokenPromise = this.updateAntiForgeryToken()
    }

    return antiForgeryTokenPromise.andThen(() => {
      if (this.antiForgeryToken != '') {
        options ??= {}
        const headers = new Headers(options.headers)
        headers.set(this.antiForgeryHeaderName, this.antiForgeryToken)
        options.headers = headers
      }

      return fromPromise(fetch(path, options), (error) => {
        console.error(`Unexpected error whilst making fetch request to ${path}`, error)
        return new ApiError('unknown')
      }).andThen((result) => {
        if (supportedResponseCodes != null && supportedResponseCodes.includes(result.status)) {
          return ok(result)
        } else if (result.status === 200) {
          return ok(result)
        }

        console.error(`Unexpected response status code ${result.status} for path ${path}`)
        if (result.status === 400) {
          return err(new ApiError('badrequest'))
        } else if (result.status === 401) {
          return err(new ApiError('unauthorized'))
        } else if (result.status === 403) {
          return err(new ApiError('forbidden'))
        }

        return err(new ApiError('unknown'))
      })
    })
  }

  private getBaseRequestOptions(options: {
    method: 'GET' | 'PUT' | 'POST' | 'DELETE'
    json?: string
  }): RequestInit {
    const result: RequestInit = {
      method: options.method,
      credentials: 'include',
    }
    if (options.json) {
      result.body = options.json
      result.headers = {
        'Content-Type': 'application/json',
      }
    }
    return result
  }

  private static deserializeJson<TBody>(response: Response): ResultAsync<TBody, ApiError> {
    // TODO add some kind of schema validation? Maybe look at Zod.
    return fromPromise(response.json(), (error) => {
      console.error(`Unexpected error whilst deserializing JSON`, error)
      return new ApiError('unknown')
    })
  }
}

export type UserPermission = 'none' | 'read' | 'readAndModify'

export interface IGetUserResponse {
  id: number
  name: string
  emailAddress: string
  entriesPermission: UserPermission
  exportsPermission: UserPermission
  projectsPermission: UserPermission
  usersPermission: UserPermission
}

export interface IGetAllUsersResponse {
  users: {
    id: number
    name: string
    emailAddress: string
    entriesPermission: UserPermission
    exportsPermission: UserPermission
    projectsPermission: UserPermission
    usersPermission: UserPermission
  }[]
}

export interface IUpdateUserPermissionsRequest {
  entriesPermission: UserPermission
  exportsPermission: UserPermission
  projectsPermission: UserPermission
  usersPermission: UserPermission
}

export interface ICreateProjectRequest {
  name: string
  enabled: boolean
  userIds: number[]
  invoiceItemNames: string[]
}

export interface ICreateProjectResponse {
  id: number
}

export interface IGetAllProjectsResponse {
  projects: {
    id: number
    name: string
    enabled: boolean
    users: { id: number; name: string }[]
    invoiceItems: { id: number; name: string }[]
  }[]
}

export interface IUpdateProjectRequest {
  name: string
  enabled: boolean
  userIds: number[]
  invoiceItems: { id?: number; name: string }[]
}

export interface IPutUpdateWeekUserEntriesRequest {
  monday: IPutUpdateWeekUserEntriesRequestDay
  tuesday: IPutUpdateWeekUserEntriesRequestDay
  wednesday: IPutUpdateWeekUserEntriesRequestDay
  thursday: IPutUpdateWeekUserEntriesRequestDay
  friday: IPutUpdateWeekUserEntriesRequestDay
  saturday: IPutUpdateWeekUserEntriesRequestDay
  sunday: IPutUpdateWeekUserEntriesRequestDay
}

export interface IPutUpdateWeekUserEntriesRequestDay {
  entries: {
    invoiceItemId: number
    minutes: number
    remark?: string
  }[]
}

export interface IGetWeekUserEntriesResponse {
  monday: IGetWeekUserEntriesResponseDay
  tuesday: IGetWeekUserEntriesResponseDay
  wednesday: IGetWeekUserEntriesResponseDay
  thursday: IGetWeekUserEntriesResponseDay
  friday: IGetWeekUserEntriesResponseDay
  saturday: IGetWeekUserEntriesResponseDay
  sunday: IGetWeekUserEntriesResponseDay
}

export interface IGetWeekUserEntriesResponseDay {
  entries: {
    invoiceItemId: number
    minutes: number
    remark: string | null
  }[]
}

export interface IUpdateAllUserInvoiceItemCustomizationsRequest {
  invoiceItems: {
    id: number
    ordinal: number
    color: string | null
  }[]
}

export interface IGetUserProjectsResponse {
  projects: {
    id: number
    name: string
    enabled: boolean
    invoiceItems: {
      id: number
      name: string
      ordinal: number
      color: string | null
    }[]
  }[]
}

// Used as a singleton in order to use the anti forgery token properly
const apiClient = new ApiClient()
export function useApiClient(): IApiClient {
  return apiClient
}
