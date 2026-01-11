import { Routes } from '@angular/router';
import { LessonPlan } from './lesson-plan/lesson-plan';
import { LessonDays } from './lesson-days/lesson-days';

export const routes: Routes = [
  { path: '', redirectTo: '/lesson-plan', pathMatch: 'full' },
  { path: 'lesson-plan', component: LessonPlan },
  { path: 'lesson-days', component: LessonDays }
];
