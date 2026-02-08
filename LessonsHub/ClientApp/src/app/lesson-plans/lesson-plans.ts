import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { LessonDayService } from '../services/lesson-day.service';
import { LessonPlanSummary } from '../models/lesson-day.model';

@Component({
  selector: 'app-lesson-plans',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatProgressBarModule
  ],
  templateUrl: './lesson-plans.html',
  styleUrl: './lesson-plans.css'
})
export class LessonPlans implements OnInit {
  plans: LessonPlanSummary[] = [];
  isLoading = true;
  error = '';

  constructor(
    private lessonDayService: LessonDayService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadPlans();
  }

  loadPlans(): void {
    this.lessonDayService.getLessonPlans().subscribe({
      next: (data) => {
        this.plans = data;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error loading plans', err);
        this.error = 'Failed to load lesson plans.';
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }
}
