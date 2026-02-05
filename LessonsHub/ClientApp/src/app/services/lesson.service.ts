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

  generateExercise(lessonId: number, difficulty: string): Observable<Exercise> {
    return this.http.post<Exercise>(`${this.apiUrl}/${lessonId}/generate-exercise?difficulty=${difficulty}`, {});
  }

  submitExerciseAnswer(exerciseId: number, answer: string): Observable<ExerciseAnswer> {
    return this.http.post<ExerciseAnswer>(`${this.apiUrl}/exercise/${exerciseId}/check`, JSON.stringify(answer), {
      headers: { 'Content-Type': 'application/json' }
    });
  }
}