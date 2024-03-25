import {Injectable} from '@angular/core';
import {HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest} from '@angular/common/http';
import {Router} from '@angular/router';
import {catchError, Observable, throwError} from "rxjs";
import {MessageService} from "primeng/api";
import {Location} from "@angular/common";

@Injectable()
export class ErrorHttpInterceptor implements HttpInterceptor {
  constructor(private router: Router, private messageService: MessageService, private location: Location) {}

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(request).pipe(catchError(err => {
      if (err instanceof HttpErrorResponse && err.status === 401) {
        this.router.navigate(['login']);
      }
      if (err instanceof HttpErrorResponse && err.status === 403) {
        this.messageService.add({severity: 'error', summary: 'Error', detail: 'No tienes permisos para acceder a esta página'});
        this.location.back();
      }
      /*if (err instanceof HttpErrorResponse && err.status === 404) {
        this.messageService.add({severity: 'error', summary: 'Error', detail: 'No se ha encontrado la página'});
        this.location.back();
      }*/
      if (err instanceof HttpErrorResponse && err.status === 500) {
        this.messageService.add({severity: 'error', summary: 'Error', detail: 'Ha ocurrido un error'});
        this.location.back();
      }

      return throwError(err);
    }));
  }
}
