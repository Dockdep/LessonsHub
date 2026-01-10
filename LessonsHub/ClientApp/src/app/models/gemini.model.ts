export interface Message {
  role: string;
  content: string;
}

export interface GeminiRequest {
  messages: Message[];
}

export interface GeminiResponse {
  content: string;
  model: string;
  tokensUsed: number;
}
