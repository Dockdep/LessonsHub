import { Component, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { GeminiService } from '../services/gemini.service';
import { Message } from '../models/gemini.model';

@Component({
  selector: 'app-chat',
  imports: [CommonModule, FormsModule],
  templateUrl: './chat.html',
  styleUrl: './chat.css',
})
export class Chat {
  messages: Message[] = [];
  userInput: string = '';
  isLoading: boolean = false;
  error: string = '';

  constructor(
    private geminiService: GeminiService,
    private cdr: ChangeDetectorRef
  ) { }

  sendMessage(): void {
    if (!this.userInput.trim()) {
      return;
    }

    const userMessage: Message = {
      role: 'user',
      content: this.userInput
    };

    this.messages.push(userMessage);
    this.isLoading = true;
    this.error = '';

    this.geminiService.sendMessage({ messages: [userMessage] }).subscribe({
      next: (response) => {
        console.log('Received response:', response);
        const assistantMessage: Message = {
          role: 'assistant',
          content: response.content
        };
        // Create new array reference to trigger change detection
        this.messages = [...this.messages, assistantMessage];
        this.userInput = '';
        this.isLoading = false;
        console.log('Messages updated:', this.messages);
        console.log('Input cleared, isLoading:', this.isLoading);
        // Manually trigger change detection
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error:', error);
        this.error = 'Error communicating with Gemini: ' + (error.error?.error || error.message);
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  clearChat(): void {
    this.messages = [];
    this.error = '';
  }
}
