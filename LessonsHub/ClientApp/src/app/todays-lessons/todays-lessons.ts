import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatChipsModule } from '@angular/material/chips';
import { RouterModule } from '@angular/router';
import { LessonDayService } from '../services/lesson-day.service';
import { LessonDay } from '../models/lesson-day.model';

@Component({
  selector: 'app-todays-lessons',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatListModule,
    MatProgressBarModule,
    MatChipsModule,
    RouterModule
  ],
  templateUrl: './todays-lessons.html',
  styleUrl: './todays-lessons.css'
})
export class TodaysLessons implements OnInit {
  today: Date = new Date();
  lessonDay: LessonDay | null = null;
  isLoading: boolean = false;
  error: string = '';

  constructor(
    private lessonDayService: LessonDayService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadTodaysLessons();
  }

  loadTodaysLessons(): void {
    this.isLoading = true;
    this.error = '';
    
    // Format date as YYYY-MM-DD
    const dateStr = this.today.toISOString().split('T')[0];

    this.lessonDayService.getLessonDayByDate(dateStr).subscribe({
      next: (data) => {
        this.lessonDay = data;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Failed to load today\'s lessons', err);
        this.error = 'Unable to load your schedule. Please try again later.';
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  // Helper to get a greeting based on time of day
  get greeting(): string {
    const hour = this.today.getHours();
    if (hour < 12) return 'Good morning';
    if (hour < 18) return 'Good afternoon';
    return 'Good evening';
  }
}