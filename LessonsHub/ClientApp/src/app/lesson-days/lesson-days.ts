import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatListModule } from '@angular/material/list';
import { MatChipsModule } from '@angular/material/chips';
import { LessonDayService } from '../services/lesson-day.service';
import { LessonDay, LessonPlanSummary, AvailableLesson } from '../models/lesson-day.model';

@Component({
  selector: 'app-lesson-days',
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatSelectModule,
    MatFormFieldModule,
    MatInputModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatDialogModule,
    MatListModule,
    MatChipsModule
  ],
  templateUrl: './lesson-days.html',
  styleUrl: './lesson-days.css',
})
export class LessonDays implements OnInit {
  lessonPlans: LessonPlanSummary[] = [];
  selectedLessonPlan: number | null = null;
  availableLessons: AvailableLesson[] = [];
  selectedDate: Date = new Date();
  lessonDays: LessonDay[] = [];
  selectedLesson: AvailableLesson | null = null;

  dayName: string = '';
  dayDescription: string = '';

  isLoading: boolean = false;
  isAssigning: boolean = false;
  error: string = '';
  successMessage: string = '';

  currentMonth: number = new Date().getMonth();
  currentYear: number = new Date().getFullYear();

  constructor(
    private lessonDayService: LessonDayService,
    private dialog: MatDialog,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadLessonPlans();
    this.loadLessonDays();
  }

  loadLessonPlans(): void {
    this.isLoading = true;
    this.cdr.detectChanges();
    this.lessonDayService.getLessonPlans().subscribe({
      next: (plans) => {
        this.lessonPlans = plans;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        this.error = 'Failed to load lesson plans';
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  onLessonPlanChange(): void {
    if (this.selectedLessonPlan) {
      this.loadAvailableLessons(this.selectedLessonPlan);
    } else {
      this.availableLessons = [];
    }
  }

  loadAvailableLessons(planId: number): void {
    this.lessonDayService.getAvailableLessons(planId).subscribe({
      next: (lessons) => {
        this.availableLessons = lessons;
        this.cdr.detectChanges();
      },
      error: (error) => {
        this.error = 'Failed to load lessons';
        this.cdr.detectChanges();
      }
    });
  }

  loadLessonDays(): void {
    this.lessonDayService.getLessonDaysByMonth(this.currentYear, this.currentMonth + 1).subscribe({
      next: (days) => {
        this.lessonDays = days;
        this.populateFieldsForSelectedDate();
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Failed to load lesson days', error);
        this.cdr.detectChanges();
      }
    });
  }

  populateFieldsForSelectedDate(): void {
    const existingDay = this.getLessonDayForDate(this.selectedDate);
    if (existingDay) {
      this.dayName = existingDay.name;
      this.dayDescription = existingDay.shortDescription;
    }
  }

  onDateChange(date: Date): void {
    this.selectedDate = date;
    const existingDay = this.getLessonDayForDate(date);
    if (existingDay) {
      this.dayName = existingDay.name;
      this.dayDescription = existingDay.shortDescription;
    } else {
      this.dayName = '';
      this.dayDescription = '';
    }
  }

  assignLesson(): void {
    if (!this.selectedLesson || !this.selectedDate) {
      this.error = 'Please select a lesson and a date';
      this.cdr.detectChanges();
      return;
    }

    this.isAssigning = true;
    this.error = '';
    this.cdr.detectChanges();

    const request = {
      lessonId: this.selectedLesson.id,
      date: this.formatDateToYYYYMMDD(this.selectedDate),
      dayName: this.dayName || `Lesson Day - ${this.formatDate(this.selectedDate)}`,
      dayDescription: this.dayDescription || this.selectedLesson.shortDescription
    };

    this.lessonDayService.assignLesson(request).subscribe({
      next: () => {
        this.successMessage = 'Lesson assigned successfully!';
        this.isAssigning = false;
        this.loadLessonDays();
        this.resetForm();
        this.cdr.detectChanges();

        setTimeout(() => {
          this.successMessage = '';
          this.cdr.detectChanges();
        }, 3000);
      },
      error: (error) => {
        this.error = 'Failed to assign lesson';
        this.isAssigning = false;
        this.cdr.detectChanges();
      }
    });
  }

  unassignLesson(lessonId: number): void {
    this.lessonDayService.unassignLesson(lessonId).subscribe({
      next: () => {
        this.successMessage = 'Lesson unassigned successfully!';
        this.loadLessonDays();
        if (this.selectedLessonPlan) {
          this.loadAvailableLessons(this.selectedLessonPlan);
        }
        this.cdr.detectChanges();

        setTimeout(() => {
          this.successMessage = '';
          this.cdr.detectChanges();
        }, 3000);
      },
      error: (error) => {
        this.error = 'Failed to unassign lesson';
        this.cdr.detectChanges();
      }
    });
  }

  resetForm(): void {
    this.selectedLesson = null;
    this.dayName = '';
    this.dayDescription = '';
  }

  formatDate(date: Date): string {
    return date.toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' });
  }

  formatDateToYYYYMMDD(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  getLessonDayForDate(date: Date): LessonDay | undefined {
    const dateStr = this.formatDateToYYYYMMDD(date);
    return this.lessonDays.find(day => day.date.startsWith(dateStr));
  }

  previousMonth(): void {
    if (this.currentMonth === 0) {
      this.currentMonth = 11;
      this.currentYear--;
    } else {
      this.currentMonth--;
    }
    this.loadLessonDays();
  }

  nextMonth(): void {
    if (this.currentMonth === 11) {
      this.currentMonth = 0;
      this.currentYear++;
    } else {
      this.currentMonth++;
    }
    this.loadLessonDays();
  }

  get currentMonthName(): string {
    return new Date(this.currentYear, this.currentMonth).toLocaleDateString('en-US', { month: 'long', year: 'numeric' });
  }
}
