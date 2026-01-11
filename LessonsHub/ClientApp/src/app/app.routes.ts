import { Routes } from '@angular/router';
import { LessonPlan } from './lesson-plan/lesson-plan';

export const routes: Routes = [
  { path: '', redirectTo: '/lesson-plan', pathMatch: 'full' },
  { path: 'lesson-plan', component: LessonPlan }
];
