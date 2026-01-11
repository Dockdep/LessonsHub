import { Routes } from '@angular/router';
import { LessonPlan } from './lesson-plan/lesson-plan';
import { LessonDays } from './lesson-days/lesson-days';
import { TodaysLessons } from './todays-lessons/todays-lessons'; // Import the new component
import { LessonDetail } from './lesson-detail/lesson-detail';

export const routes: Routes = [
  { path: '', redirectTo: '/today', pathMatch: 'full' }, // Optional: Make Today the home page
  { path: 'today', component: TodaysLessons },
  { path: 'lesson/:id', component: LessonDetail },
  { path: 'lesson-plan', component: LessonPlan },
  { path: 'lesson-days', component: LessonDays }
];