import {Injectable} from '@angular/core';
import {HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest} from '@angular/common/http';
import {Router} from '@angular/router';
import {catchError, Observable, throwError} from "rxjs";
import {MessageService} from "primeng/api";

@Injectable()
export class ErrorHttpInterceptor implements HttpInterceptor {
  constructor(private router: Router, private messageService: MessageService) {}

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(request).pipe(catchError(err => {
      if (err instanceof HttpErrorResponse && err.status === 401) {
        this.router.navigate(['login']);
      }
      if (err instanceof HttpErrorResponse && err.status === 403) {
        this.messageService.add({severity: 'error', summary: 'Error', detail: 'No tienes permisos para acceder a esta página'});
      }

      return throwError(err);
    }));
  }
}
