import { Component, OnInit, AfterViewInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { MatIconModule } from '@angular/material/icon';

declare const google: {
  accounts: {
    id: {
      initialize(config: { client_id: string; callback: (response: { credential: string }) => void }): void;
      renderButton(parent: HTMLElement, options: object): void;
    };
  };
};

@Component({
  selector: 'app-login',
  imports: [MatIconModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login implements OnInit, AfterViewInit {
  private auth = inject(AuthService);
  private router = inject(Router);

  error = '';
  loading = false;

  ngOnInit(): void {
    if (this.auth.isLoggedIn()) {
      this.router.navigate(['/today']);
    }
  }

  ngAfterViewInit(): void {
    if (!this.auth.isLoggedIn()) {
      this.waitForGsi();
    }
  }

  private waitForGsi(): void {
    if (typeof (window as any)['google'] !== 'undefined') {
      this.initGoogle();
    } else {
      const script = document.querySelector('script[src*="accounts.google.com/gsi"]');
      if (script) {
        script.addEventListener('load', () => this.initGoogle());
      } else {
        let attempts = 0;
        const interval = setInterval(() => {
          attempts++;
          if (typeof (window as any)['google'] !== 'undefined') {
            clearInterval(interval);
            this.initGoogle();
          } else if (attempts > 50) {
            clearInterval(interval);
            this.error = 'Google Sign-In failed to load. Check your internet connection.';
          }
        }, 100);
      }
    }
  }

  private initGoogle(): void {
    const clientId = (window as { GOOGLE_CLIENT_ID?: string }).GOOGLE_CLIENT_ID
      ?? document.querySelector<HTMLMetaElement>('meta[name="google-client-id"]')?.content
      ?? '';

    (window as any)['google'].accounts.id.initialize({
      client_id: clientId,
      callback: async (response: { credential: string }) => {
        this.loading = true;
        this.error = '';
        try {
          await this.auth.loginWithGoogle(response.credential);
          this.router.navigate(['/today']);
        } catch (err) {
          console.error('Sign-in error:', err);
          this.error = 'Sign-in failed. Please try again.';
        } finally {
          this.loading = false;
        }
      }
    });

    const btn = document.getElementById('google-btn');
    if (btn) {
      (window as any)['google'].accounts.id.renderButton(btn, {
        type: 'standard',
        shape: 'rectangular',
        theme: 'outline',
        size: 'large',
        text: 'signin_with',
        width: 280
      });
    }
  }
}
