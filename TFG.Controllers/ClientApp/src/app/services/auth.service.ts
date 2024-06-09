import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {catchError, Observable, throwError} from 'rxjs';
import {MessageService} from "primeng/api";
import {UserCreate} from "../models/UserCreate";
import {environment} from "../../environments/environment";

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private loginUrl = `${environment.apiUrl}/session/login`
  private registerUrl = `${environment.apiUrl}/session/signup`

  constructor(private http: HttpClient, private messageService: MessageService) {
  }

  login(username: string, password: string): Observable<any> {
    return this.http.post(this.loginUrl, {username, password}).pipe(
      catchError(error => this.handleError(error))
    )
  }

  register(userRegister: UserCreate): Observable<any> {
    return this.http.post(this.registerUrl, userRegister).pipe(
      catchError(error => this.handleError(error))
    )
  }

  private handleError(error: any) {
    const errorMessages: { [key: string]: string } = {
      'Username, Email or DNI already exists': 'Nombre de usuario, email o DNI ya existen',
      'Invalid gender. Valid values are: Male, Female, Other, PreferNotToSay': 'Género inválido. Los valores válidos son: Masculino, Femenino, Otro, Prefiero no decirlo',
      'User not found': 'Usuario no encontrado',
      'User or password incorrect': 'Usuario o contraseña incorrectos',
      'Server error': 'Error en el servidor'
    };

    if (error.status === 400) {
      const message = errorMessages[error.error.title];
      if (message) {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          closable: false,
          detail: message,
          life: 2000
        });
      } else {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          closable: false,
          detail: errorMessages['User or password incorrect'],
          life: 2000
        });
      }
    } else if (error.status === 500) {
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        closable: false,
        detail: errorMessages['Server error'],
        life: 2000
      });
    }

    return throwError(() => error);
  }

}
