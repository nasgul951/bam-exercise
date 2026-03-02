import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { BaseResponse } from '../../shared/models/base-response.model';
import { PagedResult } from '../../shared/models/paged-result.model';
import { LogEntry } from '../../shared/models/log-entry.model';

export interface GetLogsResult extends BaseResponse {
  logs: PagedResult<LogEntry>;
}

@Injectable({ providedIn: 'root' })
export class LogsService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/logs`;

  getLogs(pageNumber: number, pageSize: number): Observable<GetLogsResult> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);
    return this.http.get<GetLogsResult>(this.base, { params });
  }
}
