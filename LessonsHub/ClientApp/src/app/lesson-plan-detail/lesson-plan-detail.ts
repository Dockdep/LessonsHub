import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { LessonDayService } from '../services/lesson-day.service';
import { LessonPlanDetail as LessonPlanDetailModel } from '../models/lesson-day.model';
import { ConfirmDialog } from '../confirm-dialog/confirm-dialog';

@Component({
  selector: 'app-lesson-plan-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatProgressBarModule,
    MatDialogModule
  ],
  templateUrl: './lesson-plan-detail.html',
  styleUrl: './lesson-plan-detail.css'
})
export class LessonPlanDetail implements OnInit {
  plan: LessonPlanDetailModel | null = null;
  isLoading = true;
  isDeleting = false;
  error = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private lessonDayService: LessonDayService,
    private dialog: MatDialog,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) {
      this.loadPlan(id);
    } else {
      this.error = 'Invalid plan ID';
      this.isLoading = false;
    }
  }

  loadPlan(id: number): void {
    this.lessonDayService.getLessonPlanDetail(id).subscribe({
      next: (data) => {
        this.plan = data;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error loading plan detail', err);
        this.error = 'Failed to load lesson plan.';
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  confirmDelete(): void {
    if (!this.plan) return;

    const dialogRef = this.dialog.open(ConfirmDialog, {
      width: '420px',
      data: {
        title: 'Delete Plan',
        message: `Are you sure you want to delete "${this.plan.name}"? This will permanently delete all lessons, exercises, and answers associated with this plan.`,
        confirmText: 'Delete',
        cancelText: 'Cancel'
      }
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.deletePlan();
      }
    });
  }

  private deletePlan(): void {
    if (!this.plan) return;

    this.isDeleting = true;
    this.lessonDayService.deleteLessonPlan(this.plan.id).subscribe({
      next: () => {
        this.router.navigate(['/lesson-plans']);
      },
      error: (err) => {
        console.error('Error deleting plan', err);
        this.error = 'Failed to delete lesson plan.';
        this.isDeleting = false;
        this.cdr.detectChanges();
      }
    });
  }
}
