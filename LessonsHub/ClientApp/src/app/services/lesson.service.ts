import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Lesson } from '../models/lesson.model'; // Assuming Lesson interface is here or you can move it to a shared model file

@Injectable({
  providedIn: 'root'
})
export class LessonService {
  private apiUrl = 'api/lesson';

  constructor(private http: HttpClient) { }

  getLessonById(id: number): Observable<Lesson> {
    return this.http.get<Lesson>(`${this.apiUrl}/${id}`);
  }
}