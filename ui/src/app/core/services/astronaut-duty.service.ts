import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { BaseResponse } from '../../shared/models/base-response.model';
import { PersonAstronaut } from '../../shared/models/person.model';
import { AstronautDuty, CreateAstronautDutyRequest } from '../../shared/models/astronaut-duty.model';

export interface GetAstronautDutiesByNameResult extends BaseResponse {
  person: PersonAstronaut;
  astronautDuties: AstronautDuty[];
}

export interface CreateAstronautDutyResult extends BaseResponse {
  id: number | null;
}

@Injectable({ providedIn: 'root' })
export class AstronautDutyService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/astronautduty`;

  getDutiesByName(name: string): Observable<GetAstronautDutiesByNameResult> {
    return this.http.get<GetAstronautDutiesByNameResult>(`${this.base}/${encodeURIComponent(name)}`);
  }

  createDuty(payload: CreateAstronautDutyRequest): Observable<CreateAstronautDutyResult> {
    return this.http.post<CreateAstronautDutyResult>(this.base, payload);
  }
}
