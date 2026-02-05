import { Component, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { LessonPlanService } from '../services/lesson-plan.service';
import { LessonPlanRequest, LessonPlanResponse, GeneratedLesson, LESSON_TYPES } from '../models/lesson-plan.model';

@Component({
  selector: 'app-lesson-plan',
  imports: [
    CommonModule,
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    MatIconModule,
    MatSelectModule
  ],
  templateUrl: './lesson-plan.html',
  styleUrl: './lesson-plan.css',
})
export class LessonPlan {
  lessonType: string = 'Default';
  lessonTypes = LESSON_TYPES;
  planName: string = '';
  numberOfDays: number | null = null;
  topic: string = '';
  description: string = '';
  isLoading: boolean = false;
  isSaving: boolean = false;
  error: string = '';
  saveSuccess: boolean = false;
  generatedPlan: LessonPlanResponse | null = null;

  constructor(
    private lessonPlanService: LessonPlanService,
    private cdr: ChangeDetectorRef
  ) { }

  generateLessonPlan(): void {
    if (!this.planName.trim() || !this.topic.trim()) {
      this.error = 'Please provide a plan name and topic.';
      return;
    }

    this.isLoading = true;
    this.error = '';
    this.generatedPlan = null;

    const request: LessonPlanRequest = {
      lessonType: this.lessonType,
      planName: this.planName,
      numberOfDays: this.numberOfDays,
      topic: this.topic,
      description: this.description
    };

    this.lessonPlanService.generateLessonPlan(request).subscribe({
      next: (response) => {
        console.log('Received response:', response);
        this.generatedPlan = response;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error:', error);
        this.error = 'Error generating lesson plan: ' + (error.error?.message || error.message);
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  saveLessonPlan(): void {
    if (!this.generatedPlan) return;

    this.isSaving = true;
    this.saveSuccess = false;
    this.error = '';

    this.lessonPlanService.saveLessonPlan(this.generatedPlan, this.description, this.lessonType).subscribe({
      next: (response) => {
        console.log('Save response:', response);
        this.saveSuccess = true;
        this.isSaving = false;
        this.cdr.detectChanges();

        // Hide success message after 3 seconds
        setTimeout(() => {
          this.saveSuccess = false;
          this.cdr.detectChanges();
        }, 3000);
      },
      error: (error) => {
        console.error('Save error:', error);
        this.error = 'Error saving lesson plan: ' + (error.error?.message || error.message);
        this.isSaving = false;
        this.cdr.detectChanges();
      }
    });
  }

  resetForm(): void {
    this.lessonType = 'Default';
    this.planName = '';
    this.numberOfDays = null;
    this.topic = '';
    this.description = '';
    this.error = '';
    this.saveSuccess = false;
    this.generatedPlan = null;
  }

  downloadJson(): void {
    if (!this.generatedPlan) return;

    const dataStr = JSON.stringify(this.generatedPlan, null, 2);
    const dataUri = 'data:application/json;charset=utf-8,' + encodeURIComponent(dataStr);

    const exportFileDefaultName = `${this.planName.replace(/\s+/g, '_')}_lesson_plan.json`;

    const linkElement = document.createElement('a');
    linkElement.setAttribute('href', dataUri);
    linkElement.setAttribute('download', exportFileDefaultName);
    linkElement.click();
  }
}
