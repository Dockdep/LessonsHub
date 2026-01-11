import { Component, signal } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';
import { ChatComponent } from './chat/chat.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, ChatComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('LessonsHub');
}
