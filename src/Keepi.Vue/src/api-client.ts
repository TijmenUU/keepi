import { toShortIsoDate } from '@/format'
import { fromPromise, err, ok } from 'neverthrow'

export default class ApiClient {
  public getUser() {
    const options = this.getBaseRequestOptions({
      method: 'GET',
    })

    return this.makeRequest('/user', options).andThen((result) => {
      return ApiClient.deserializeJson<IGetUserResponse>(result)
    })
  }

  public ensureUserIsRegistered() {
    const options = this.getBaseRequestOptions({
      method: 'POST',
    })

    return this.makeRequest('/registeruser', options, [200, 201]).andThen((result) => {
      return ApiClient.deserializeJson<IPostRegisterUserResponse>(result).andThen((result) => {
        if (result.result === 'created' || result.result === 'userAlreadyExists') {
          return ok()
        } else {
          console.error(`Unexpected result value ${result.result}`)
          return err('unknown')
        }
      })
    })
  }

  public updateAllUserEntryCategories(value: IPutUpdateUserEntryCategoriesRequest) {
    const options = this.getBaseRequestOptions({
      method: 'PUT',
      json: JSON.stringify(value),
    })

    return this.makeRequest('/user/entrycategories', options, [204]).andThen(() => {
      return ok()
    })
  }

  public getAllUserEntryCategories() {
    const options = this.getBaseRequestOptions({
      method: 'GET',
    })

    return this.makeRequest('/user/entrycategories', options, [200]).andThen((result) => {
      return ApiClient.deserializeJson<IGetUserEntryCategoriesResponse>(result)
    })
  }

  public getWeekUserEntries(year: number, weekNumber: number) {
    const options = this.getBaseRequestOptions({
      method: 'GET',
    })

    return this.makeRequest(`/user/entries/year/${year}/week/${weekNumber}`, options, [
      200,
    ]).andThen((result) => {
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

    return this.makeRequest(`/user/entries/year/${year}/week/${weekNumber}`, options, [
      201,
    ]).andThen(() => {
      return ok()
    })
  }

  public getUserEntriesExport(from: Date, to: Date) {
    const options = this.getBaseRequestOptions({
      method: 'POST',
      json: JSON.stringify({
        start: toShortIsoDate(from),
        stop: toShortIsoDate(to),
      }),
    })

    return this.makeRequest(`/user/entries/export`, options, [200]).andThen((result) =>
      fromPromise(
        result.blob().then((blob) => ({
          blob,
          fileName: ApiClient.getFileNameFromHeaderOrFallback(result.headers, 'export.csv'),
        })),
        () => err('unknown'),
      ),
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

  private makeRequest(subpath: string, options?: RequestInit, supportedResponseCodes?: number[]) {
    const path = subpath.startsWith('/') ? `/api${subpath}` : `/api/${subpath}`

    return fromPromise<Response, 'unknown'>(fetch(path, options), (error) => {
      console.error(`Unexpected error whilst making fetch request to ${path}`, error)
      return 'unknown'
    }).andThen((result) => {
      if (supportedResponseCodes != null && supportedResponseCodes.includes(result.status)) {
        return ok(result)
      } else if (result.status === 200) {
        return ok(result)
      }

      console.error(`Unexpected response status code ${result.status} for path ${path}`)
      if (result.status === 400) {
        return err('badrequest')
      } else if (result.status === 401) {
        return err('unauthorized')
      } else if (result.status === 403) {
        return err('forbidden')
      }

      return err('unknown')
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

  private static deserializeJson<TBody>(response: Response) {
    // TODO add some kind of schema validation?
    return fromPromise<TBody, 'unknown'>(response.json(), (error) => {
      console.error(`Unexpected error whilst deserializing JSON`, error)
      return 'unknown'
    })
  }
}

export interface IGetUserResponse {
  name: string
  registered: boolean
}

export interface IPostRegisterUserResponse {
  result: 'created' | 'userAlreadyExists'
}

export interface IGetUserEntryCategoriesResponse {
  categories: {
    id: number
    name: string
    ordinal: number
    enabled: boolean
    activeFrom: string | null // yyyy-MM-dd
    activeTo: string | null // yyyy-MM-dd
  }[]
}

export interface IPutUpdateUserEntryCategoriesRequest {
  userEntryCategories: {
    id?: number
    name: string
    ordinal: number
    enabled: boolean
    activeFrom?: string // yyyy-MM-dd
    activeTo?: string // yyyy-MM-dd
  }[]
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
    entryCategoryId: number
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
    entryCategoryId: number
    minutes: number
    remark: string | null
  }[]
}
