import { Injectable, inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

@Injectable({ providedIn: 'root' })
export class NotificationsService {
  private readonly snackBar = inject(MatSnackBar);

  success(message: string): void {
    this.show(message, 'success');
  }
  error(message: string): void {
    this.show(message, 'error');
  }
  warning(message: string): void {
    this.show(message, 'warning');
  }
  info(message: string): void {
    this.show(message, 'info');
  }

  private show(message: string, kind: string): void {
    this.snackBar.open(message, 'Dismiss', {
      duration: kind === 'error' ? 7000 : 4000,
      panelClass: [`toast-${kind}`],
      horizontalPosition: 'right',
      verticalPosition: 'top',
    });
  }
}
