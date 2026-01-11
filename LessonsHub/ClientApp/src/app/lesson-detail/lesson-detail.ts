import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MarkdownModule } from 'ngx-markdown';
import { LessonService } from '../services/lesson.service';
import { Lesson } from '../models/lesson.model'; // Ensure this points to the new model file

@Component({
  selector: 'app-lesson-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
    MatExpansionModule,
    MatProgressSpinnerModule,
    MarkdownModule
  ],
  templateUrl: './lesson-detail.html',
  styleUrl: './lesson-detail.css'
})
export class LessonDetail implements OnInit {
  lesson: Lesson | null = null;
  isLoading = true;
  error = '';

  constructor(
    private route: ActivatedRoute,
    private lessonService: LessonService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const lessonId = Number(this.route.snapshot.paramMap.get('id'));
    if (lessonId) {
      this.loadLesson(lessonId);
    } else {
      this.error = 'Invalid Lesson ID';
      this.isLoading = false;
    }
  }

  loadLesson(id: number): void {
    this.lessonService.getLessonById(id).subscribe({
      next: (data) => {
        this.lesson = data;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error loading lesson', err);
        this.error = 'Failed to load lesson details.';
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }
}