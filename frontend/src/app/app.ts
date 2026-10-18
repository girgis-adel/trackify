import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthService } from './core/auth/auth.service';

@Component({
  imports: [RouterOutlet],
  selector: 'app-root',
  template: '<router-outlet />',
})
export class App implements OnInit {
  private readonly auth = inject(AuthService);
  ngOnInit(): void {
    void this.auth.restore().catch(() => undefined);
  }
}
