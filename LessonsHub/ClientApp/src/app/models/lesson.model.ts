export interface Lesson {
  id: number;
  lessonNumber: number;
  name: string;
  shortDescription: string;
  content: string;
  geminiPrompt: string;
  lessonPlanId: number;
  lessonPlanName?: string; // Optional, usually populated manually or via DTO
  lessonDayId?: number;
  exercises: Exercise[];
  chatHistory: ChatMessage[];
}

export interface Exercise {
  id: number;
  exerciseText: string;
  lessonId: number;
  answers: ExerciseAnswer[];
  chatHistory: ChatMessage[];
}

export interface ExerciseAnswer {
  id: number;
  content: string;
  isCorrect: boolean;
}

export interface ChatMessage {
  id: number;
  role: string; // 'User' or 'Model'
  content: string;
  timestamp: Date;
}