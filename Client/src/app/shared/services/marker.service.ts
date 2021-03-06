import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MarkerInfo } from '../models/marker-info';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AppConfigService } from './app-config.service';

@Injectable({
  providedIn: 'root',
})
export class MarkerService {
  constructor(private http: HttpClient, private appConfigService: AppConfigService) {}

  updateMarkers(floorId: string, markers: MarkerInfo[]): Observable<boolean> {
    return this.http
      .put<boolean>(`${this.appConfigService.getConfig().BASE_API_URL}marker/${floorId}`, markers)
      .pipe(catchError(() => of(false)));
  }

  getMarkers(floorId: string): Observable<MarkerInfo[]> {
    return this.http.get<MarkerInfo[]>(`${this.appConfigService.getConfig().BASE_API_URL}marker/${floorId}`).pipe(catchError(() => of([])));
  }
}
