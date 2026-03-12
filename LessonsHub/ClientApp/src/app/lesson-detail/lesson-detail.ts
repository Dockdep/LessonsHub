import { Component, OnInit, ChangeDetectorRef, ViewChildren, QueryList } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatExpansionModule, MatExpansionPanel } from '@angular/material/expansion';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { FormsModule } from '@angular/forms';
import { MarkdownModule } from 'ngx-markdown';
import { LessonService } from '../services/lesson.service';
import { Lesson, Exercise } from '../models/lesson.model';
import { GenerateExerciseDialog, GenerateExerciseDialogResult } from '../generate-exercise-dialog/generate-exercise-dialog';

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
    MatDialogModule,
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
  isGeneratingExercise = false;
  isTogglingComplete = false;
  prevLessonId: number | null = null;
  nextLessonId: number | null = null;
  answerTexts: { [exerciseId: number]: string } = {};
  submittingExerciseId: number | null = null;
  @ViewChildren(MatExpansionPanel) exercisePanels!: QueryList<MatExpansionPanel>;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private lessonService: LessonService,
    private dialog: MatDialog,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const lessonId = Number(params.get('id'));
      if (lessonId) {
        this.isLoading = true;
        this.lesson = null;
        this.prevLessonId = null;
        this.nextLessonId = null;
        this.error = '';
        this.answerTexts = {};
        window.scrollTo({ top: 0 });
        this.loadLesson(lessonId);
      } else {
        this.error = 'Invalid Lesson ID';
        this.isLoading = false;
      }
    });
  }

  loadLesson(id: number): void {
    this.lessonService.getLessonById(id).subscribe({
      next: (data) => {
        this.lesson = data;
        this.isLoading = false;
        this.cdr.detectChanges();
        this.lessonService.getSiblingLessonIds(id).subscribe({
          next: (res) => {
            this.prevLessonId = res.prevLessonId;
            this.nextLessonId = res.nextLessonId;
            this.cdr.detectChanges();
          }
        });
      },
      error: (err) => {
        console.error('Error loading lesson', err);
        this.error = 'Failed to load lesson details.';
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  openGenerateDialog(review?: string): void {
    const dialogRef = this.dialog.open(GenerateExerciseDialog, {
      width: '480px',
      data: { review }
    });

    dialogRef.afterClosed().subscribe((result: GenerateExerciseDialogResult | null) => {
      if (result) {
        if (result.review) {
          this.retryExercise(result);
        } else {
          this.generateExercise(result);
        }
      }
    });
  }

  private generateExercise(params: GenerateExerciseDialogResult): void {
    if (!this.lesson) return;

    this.isGeneratingExercise = true;
    this.cdr.detectChanges();
    this.lessonService.generateExercise(this.lesson.id, params.difficulty, params.comment).subscribe({
      next: (exercise) => {
        this.lesson!.exercises.push(exercise);
        this.isGeneratingExercise = false;
        this.cdr.detectChanges();
        this.openNewPanel();
      },
      error: (err) => {
        console.error('Error generating exercise', err);
        this.error = 'Failed to generate exercise: ' + (err.error?.message || err.message);
        this.isGeneratingExercise = false;
        this.cdr.detectChanges();
      }
    });
  }

  private retryExercise(params: GenerateExerciseDialogResult): void {
    if (!this.lesson || !params.review) return;

    this.isGeneratingExercise = true;
    this.cdr.detectChanges();
    this.lessonService.retryExercise(this.lesson.id, params.difficulty, params.review, params.comment).subscribe({
      next: (exercise) => {
        this.lesson!.exercises.push(exercise);
        this.isGeneratingExercise = false;
        this.cdr.detectChanges();
        this.openNewPanel();
      },
      error: (err) => {
        console.error('Error retrying exercise', err);
        this.error = 'Failed to generate exercise: ' + (err.error?.message || err.message);
        this.isGeneratingExercise = false;
        this.cdr.detectChanges();
      }
    });
  }

  private openNewPanel(): void {
    this.exercisePanels.forEach(p => p.close());
    if (this.exercisePanels.last) {
      this.exercisePanels.last.open();
    }
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

  toggleComplete(): void {
    if (!this.lesson) return;

    this.isTogglingComplete = true;
    this.lessonService.completeLesson(this.lesson.id).subscribe({
      next: (updated) => {
        this.lesson!.isCompleted = updated.isCompleted;
        this.lesson!.completedAt = updated.completedAt;
        this.isTogglingComplete = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error toggling lesson completion', err);
        this.error = 'Failed to update lesson status.';
        this.isTogglingComplete = false;
        this.cdr.detectChanges();
      }
    });
  }
}