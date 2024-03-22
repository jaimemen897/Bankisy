import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {catchError, Observable} from 'rxjs';
import {MessageService} from "primeng/api";

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private loginUrl = 'http://localhost:5196/session/login';
  private registerUrl = 'http://localhost:5196/session/signup';

  constructor(private http: HttpClient, private messageService: MessageService) {
  }

  login(username: string, password: string): Observable<any>{
    return this.http.post(this.loginUrl, {username, password}).pipe(
      catchError(error => {
        if (error.status === 400) {
          this.messageService.add({severity: 'error', summary: 'Error', detail: 'Usuario o contraseña incorrectos'});
        }
        return error;
      })
    )
  }

  register(name: string, email: string, username: string, dni: string, gender: string, password: string): Observable<any> {
    return this.http.post(this.registerUrl, {name, email, username, dni, gender, password});
  }
}
