import { Routes } from '@angular/router';
import { LessonPlan } from './lesson-plan/lesson-plan';
import { LessonPlans } from './lesson-plans/lesson-plans';
import { LessonPlanDetail } from './lesson-plan-detail/lesson-plan-detail';
import { LessonDays } from './lesson-days/lesson-days';
import { TodaysLessons } from './todays-lessons/todays-lessons';
import { LessonDetail } from './lesson-detail/lesson-detail';

export const routes: Routes = [
  { path: '', redirectTo: '/today', pathMatch: 'full' },
  { path: 'today', component: TodaysLessons },
  { path: 'lesson/:id', component: LessonDetail },
  { path: 'lesson-plan', component: LessonPlan },
  { path: 'lesson-plans', component: LessonPlans },
  { path: 'lesson-plans/:id', component: LessonPlanDetail },
  { path: 'lesson-days', component: LessonDays }
];