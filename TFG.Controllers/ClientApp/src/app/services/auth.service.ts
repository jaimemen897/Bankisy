import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {catchError, Observable, throwError} from 'rxjs';
import {MessageService} from "primeng/api";

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private loginUrl = 'http://localhost:5196/session/login';
  private registerUrl = 'http://localhost:5196/session/signup';

  constructor(private http: HttpClient, private messageService: MessageService) {
  }

  login(username: string, password: string): Observable<any> {
    return this.http.post(this.loginUrl, {username, password}).pipe(
      catchError(error => this.handleError(error))
    )
  }

  register(name: string, email: string, username: string, dni: string, gender: string, password: string): Observable<any> {
    return this.http.post(this.registerUrl, {name, email, username, dni, gender, password}).pipe(
      catchError(error => this.handleError(error))
    )
  }

  private handleError(error: any) {
    if (error.status === 400) {
      if (error.error.title === 'Username already exists') {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          closable: false,
          detail: 'El nombre de usuario ya existe'
        });
      }
      if (error.error.title === 'Email already exists') {
        this.messageService.add({severity: 'error', summary: 'Error', closable: false, detail: 'El email ya existe'});
      }
      if (error.error.title === 'DNI already exists') {
        this.messageService.add({severity: 'error', summary: 'Error', closable: false, detail: 'El DNI ya existe'});
      }
      if (error.error.title === 'Invalid gender. Valid values are: Male, Female, Other, PreferNotToSay') {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Género inválido. Los valores válidos son: Masculino, Femenino, Otro, Prefiero no decirlo'
        });
      }
      if (error.error.title === 'User not found') {
        this.messageService.add({severity: 'error', summary: 'Error', closable: false, detail: 'Usuario no encontrado'});
      } else {
        this.messageService.add({severity: 'error', summary: 'Error', closable: false, detail: 'Usuario o contraseña incorrectos'});
      }
    } else {
      this.messageService.add({severity: 'error', summary: 'Error', closable: false, detail: 'Error en el servidor'});
    }
    return throwError(() => error);
  }

}
