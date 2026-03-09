import { Routes } from '@angular/router';
import { LessonPlan } from './lesson-plan/lesson-plan';
import { LessonPlans } from './lesson-plans/lesson-plans';
import { LessonPlanDetail } from './lesson-plan-detail/lesson-plan-detail';
import { LessonDays } from './lesson-days/lesson-days';
import { TodaysLessons } from './todays-lessons/todays-lessons';
import { LessonDetail } from './lesson-detail/lesson-detail';
import { Login } from './login/login';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: 'login', component: Login },
  { path: '', redirectTo: '/today', pathMatch: 'full' },
  { path: 'today', component: TodaysLessons, canActivate: [authGuard] },
  { path: 'lesson/:id', component: LessonDetail, canActivate: [authGuard] },
  { path: 'lesson-plan', component: LessonPlan, canActivate: [authGuard] },
  { path: 'lesson-plans', component: LessonPlans, canActivate: [authGuard] },
  { path: 'lesson-plans/:id', component: LessonPlanDetail, canActivate: [authGuard] },
  { path: 'lesson-days', component: LessonDays, canActivate: [authGuard] }
];