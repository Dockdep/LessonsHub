import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Lesson, Exercise, ExerciseAnswer } from '../models/lesson.model';

@Injectable({
  providedIn: 'root'
})
export class LessonService {
  private apiUrl = 'api/lesson';

  constructor(private http: HttpClient) { }

  getLessonById(id: number): Observable<Lesson> {
    return this.http.get<Lesson>(`${this.apiUrl}/${id}`);
  }

  generateExercise(lessonId: number, difficulty: string, comment?: string): Observable<Exercise> {
    let url = `${this.apiUrl}/${lessonId}/generate-exercise?difficulty=${difficulty}`;
    if (comment) {
      url += `&comment=${encodeURIComponent(comment)}`;
    }
    return this.http.post<Exercise>(url, {});
  }

  submitExerciseAnswer(exerciseId: number, answer: string): Observable<ExerciseAnswer> {
    return this.http.post<ExerciseAnswer>(`${this.apiUrl}/exercise/${exerciseId}/check`, JSON.stringify(answer), {
      headers: { 'Content-Type': 'application/json' }
    });
  }
}