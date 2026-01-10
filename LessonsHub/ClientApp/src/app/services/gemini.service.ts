import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { GeminiRequest, GeminiResponse } from '../models/gemini.model';

@Injectable({
  providedIn: 'root'
})
export class GeminiService {
  private apiUrl = '/api/gemini/chat';

  constructor(private http: HttpClient) { }

  sendMessage(request: GeminiRequest): Observable<GeminiResponse> {
    return this.http.post<GeminiResponse>(this.apiUrl, request);
  }
}
