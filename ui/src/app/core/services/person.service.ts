import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { BaseResponse } from '../../shared/models/base-response.model';
import { PagedResult } from '../../shared/models/paged-result.model';
import { PersonAstronaut } from '../../shared/models/person.model';

export interface GetPeopleResult extends BaseResponse {
  people: PagedResult<PersonAstronaut>;
}

export interface GetPersonByNameResult extends BaseResponse {
  person: PersonAstronaut | null;
}

export interface CreatePersonResult extends BaseResponse {
  id: number;
}

@Injectable({ providedIn: 'root' })
export class PersonService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/person`;

  getPersons(pageNumber: number, pageSize: number): Observable<GetPeopleResult> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);
    return this.http.get<GetPeopleResult>(this.base, { params });
  }

  getPersonByName(name: string): Observable<GetPersonByNameResult> {
    return this.http.get<GetPersonByNameResult>(`${this.base}/${encodeURIComponent(name)}`);
  }

  createPerson(name: string): Observable<CreatePersonResult> {
    return this.http.post<CreatePersonResult>(this.base, JSON.stringify(name), {
      headers: { 'Content-Type': 'application/json' }
    });
  }
}
