import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { FormsModule } from '@angular/forms';
import { MarkdownModule } from 'ngx-markdown';
import { LessonService } from '../services/lesson.service';
import { Lesson, Exercise, DIFFICULTIES } from '../models/lesson.model';

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
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    FormsModule,
    MarkdownModule
  ],
  templateUrl: './lesson-detail.html',
  styleUrl: './lesson-detail.css'
})
export class LessonDetail implements OnInit {
  lesson: Lesson | null = null;
  isLoading = true;
  error = '';
  selectedDifficulty = 'Average';
  difficulties = DIFFICULTIES;
  exerciseComment = '';
  isGeneratingExercise = false;
  answerTexts: { [exerciseId: number]: string } = {};
  submittingExerciseId: number | null = null;

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

  generateExercise(): void {
    if (!this.lesson) return;

    this.isGeneratingExercise = true;
    const comment = this.exerciseComment.trim() || undefined;
    this.lessonService.generateExercise(this.lesson.id, this.selectedDifficulty, comment).subscribe({
      next: (exercise) => {
        this.lesson!.exercises.push(exercise);
        this.exerciseComment = '';
        this.isGeneratingExercise = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error generating exercise', err);
        this.error = 'Failed to generate exercise: ' + (err.error?.message || err.message);
        this.isGeneratingExercise = false;
        this.cdr.detectChanges();
      }
    });
  }

  submitAnswer(exercise: Exercise): void {
    const answer = this.answerTexts[exercise.id]?.trim();
    if (!answer) return;

    this.submittingExerciseId = exercise.id;
    this.lessonService.submitExerciseAnswer(exercise.id, answer).subscribe({
      next: (result) => {
        exercise.answers.push(result);
        this.answerTexts[exercise.id] = '';
        this.submittingExerciseId = null;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error submitting answer', err);
        this.error = 'Failed to submit answer: ' + (err.error?.message || err.message);
        this.submittingExerciseId = null;
        this.cdr.detectChanges();
      }
    });
  }
}