import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { LessonPlanRequest, LessonPlanResponse } from '../models/lesson-plan.model';

@Injectable({
  providedIn: 'root'
})
export class LessonPlanService {
  private apiUrl = '/api/lessonplan';

  constructor(private http: HttpClient) { }

  generateLessonPlan(request: LessonPlanRequest): Observable<LessonPlanResponse> {
    return this.http.post<LessonPlanResponse>(`${this.apiUrl}/generate`, request);
  }

  saveLessonPlan(lessonPlan: LessonPlanResponse, description: string, lessonType: string): Observable<any> {
    const request = {
      lessonPlan: lessonPlan,
      description: description,
      lessonType: lessonType
    };
    return this.http.post(`${this.apiUrl}/save`, request);
  }
}
