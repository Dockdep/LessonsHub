export interface LessonDay {
  id?: number;
  date: string;
  name: string;
  shortDescription: string;
  lessons: AssignedLesson[];
}

export interface AssignedLesson {
  id: number;
  lessonNumber: number;
  name: string;
  shortDescription: string;
  lessonPlanId: number;
  lessonPlanName: string;
}

export interface LessonPlanSummary {
  id: number;
  name: string;
  topic: string;
  description: string;
  createdDate: string;
  lessonsCount: number;
}

export interface AvailableLesson {
  id: number;
  lessonNumber: number;
  name: string;
  shortDescription: string;
  lessonPlanId: number;
  lessonPlanName: string;
  isAssigned: boolean;
}

export interface AssignLessonRequest {
  lessonId: number;
  date: string;
  dayName: string;
  dayDescription: string;
}
