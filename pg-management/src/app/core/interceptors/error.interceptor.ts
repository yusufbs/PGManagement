import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { catchError, throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const snackBar = inject(MatSnackBar);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let message = 'An unexpected error occurred.';

      if (error.status === 401) {
        localStorage.removeItem('pg_token');
        message = 'Session expired. Please log in again.';
        router.navigate(['/auth/login']);
      } else if (error.status === 403) {
        message = 'Access Denied.';
      } else if (error.status === 429) {
        message = 'Too many requests. Please wait a moment and try again.';
      } else if (error.status === 400 && error.error?.detail) {
        message = error.error.detail;
      } else if (error.status >= 500) {
        const traceId = error.error?.traceId || error.headers.get('traceparent') || '';
        message = `Server Error. ${traceId ? 'Trace ID: ' + traceId : ''}`;
      } else if (error.error?.message) {
        message = error.error.message;
      }

      snackBar.open(message, 'Close', {
        duration: 5000,
        horizontalPosition: 'right',
        verticalPosition: 'top'
      });

      return throwError(() => error);
    })
  );
};
