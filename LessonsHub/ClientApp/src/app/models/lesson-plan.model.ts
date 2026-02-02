export interface LessonPlanRequest {
  planName: string;
  numberOfDays: number | null;
  topic: string;
  description: string;
}

export interface GeneratedLesson {
  lessonNumber: number;
  name: string;
  shortDescription: string;
  topic: string;
}

export interface LessonPlanResponse {
  planName: string;
  topic: string;
  lessons: GeneratedLesson[];
}
