import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AppConfig } from '../models/app-config';

@Injectable({
  providedIn: 'root',
})
export class AppConfigService {
  private appConfig: AppConfig;

  constructor(private http: HttpClient) {}

  loadAppConfig() {
    return this.http
      .get<AppConfig>('/assets/data/appConfig.json')
      .toPromise()
      .then((data: AppConfig) => {
        this.appConfig = data;
      });
  }

  getConfig(): AppConfig {
    return this.appConfig;
  }
}
