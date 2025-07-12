import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class DataService {
  private controller: string = 'Data';
  private apiUrl = environment.apiUrl;
  constructor(private _http: HttpClient) { }

  getData() {
    return this._http.get<string[]>(`${this.apiUrl}${this.controller}/get-data`, { withCredentials: true })
  }
}
