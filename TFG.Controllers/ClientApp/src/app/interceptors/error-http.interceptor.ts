import {Injectable} from '@angular/core';
import {HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest} from '@angular/common/http';
import {Router} from '@angular/router';
import {catchError, Observable, throwError} from "rxjs";
import {MessageService} from "primeng/api";
import {Location} from "@angular/common";

@Injectable()
export class ErrorHttpInterceptor implements HttpInterceptor {
  constructor(private router: Router, private messageService: MessageService, private location: Location) {
  }

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const errorActions: { [key: number]: () => void } = {
      401: () => this.router.navigate(['login']),
      403: () => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'No tienes permisos para acceder a esta página'
        });
        this.location.back();
      },
      500: () => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          closable: false,
          detail: 'Ha ocurrido un error',
          life: 1000
        });
        this.router.navigate(['/']);
      }
    };

    return next.handle(request).pipe(catchError(err => {
      if (err instanceof HttpErrorResponse) {
        const action = errorActions[err.status];
        if (action) {
          action();
        }
      }
      return throwError(err);
    }));
  }
}
