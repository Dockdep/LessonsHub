export interface Lesson {
  id: number;
  lessonNumber: number;
  name: string;
  shortDescription: string;
  content: string;
  lessonType: string;
  lessonTopic: string;
  keyPoints: string[];
  lessonPlanId: number;
  lessonDayId?: number;
  exercises: Exercise[];
  chatHistory: ChatMessage[];
  videos: Video[];
  books: Book[];
  documentation: Documentation[];
}

export interface Exercise {
  id: number;
  exerciseText: string;
  difficulty: string;
  lessonId: number;
  answers: ExerciseAnswer[];
  chatHistory: ChatMessage[];
}

export interface ExerciseAnswer {
  id: number;
  userResponse: string;
  submittedAt: string;
  accuracyLevel?: number;
  reviewText?: string;
  exerciseId: number;
}

export interface ChatMessage {
  id: number;
  role: string;
  text: string;
  createdAt: string;
}

export interface Video {
  id: number;
  title: string;
  channel: string;
  description: string;
  url: string;
  lessonId: number;
}

export interface Book {
  id: number;
  author: string;
  bookName: string;
  chapterNumber?: number;
  chapterName?: string;
  description: string;
  lessonId: number;
}

export interface Documentation {
  id: number;
  name: string;
  section?: string;
  description: string;
  url: string;
  lessonId: number;
}

export const DIFFICULTIES = ['Easy', 'Average', 'Hard', 'Very hard'];