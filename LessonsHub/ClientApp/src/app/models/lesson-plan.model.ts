export interface LessonPlanRequest {
  lessonType: string;
  planName: string;
  numberOfDays: number | null;
  topic: string;
  description: string;
}

export const LESSON_TYPES = ['Technical', 'Language', 'Default'];

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
