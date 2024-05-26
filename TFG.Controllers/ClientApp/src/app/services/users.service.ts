import {Injectable} from '@angular/core';
import {HttpClient, HttpHeaders} from '@angular/common/http';
import {BehaviorSubject, catchError, map, Observable, throwError} from 'rxjs';
import {User} from '../models/User';
import {UserCreate} from "../models/UserCreate";
import {MessageService} from "primeng/api";
import {environment} from "../../environments/environment";

export interface Pagination<T> {
  currentPage: number;
  totalPages: number;
  pageSize: number;
  totalCount: number;
  totalRecords: number;
  items: T[];
}

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private apiUrl = `${environment.apiUrl}/users`

  private userSubject = new BehaviorSubject<User>(new User());
  user$ = this.userSubject.asObservable();

  constructor(private http: HttpClient, private messageService: MessageService) {
  }

  getUsers(pageNumber: number, pageSize: number, orderBy?: string, descending?: boolean, search?: string): Observable<Pagination<User>> {
    let url = `${this.apiUrl}?pageNumber=${pageNumber}&pageSize=${pageSize}`;
    if (orderBy) {
      url += `&orderBy=${orderBy}`;
    }
    if (descending) {
      url += `&descending=${descending}`;
    }
    if (search) {
      url += `&search=${search}`;
    }
    return this.http.get<Pagination<User>>(url).pipe(
      map(response => ({
        currentPage: response.currentPage,
        totalPages: response.totalPages,
        pageSize: response.pageSize,
        totalCount: response.totalCount,
        totalRecords: response.totalRecords,
        items: response.items
      }))
    );
  }

  getAllUsers(): Observable<User[]> {
    return this.http.get<User[]>(this.apiUrl + '/all').pipe(
      catchError(error => this.handleError(error))
    );
  }

  getUserById(id: string): Observable<User> {
    const url = `${this.apiUrl}/${id}`;
    return this.http.get<User>(url).pipe(
      catchError(error => this.handleError(error))
    );
  }

  addUser(user: UserCreate): Observable<User> {
    return this.http.post<User>(this.apiUrl, user).pipe(
      catchError(error => this.handleError(error))
    );
  }

  updateUser(user: UserCreate, id: string): Observable<User> {
    return this.http.put<User>(this.apiUrl + '/' + id, user).pipe(
      catchError(error => this.handleError(error))
    );
  }

  updateProfile(user: UserCreate): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/profile`, user).pipe(
      catchError(error => this.handleError(error))
    );
  }

  deleteUser(id: string): Observable<User> {
    return this.http.delete<User>(this.apiUrl + '/' + id).pipe(
      catchError(error => this.handleError(error))
    );
  }

  deleteMyAvatar(): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/avatar`).pipe(
      catchError(error => this.handleError(error))
    );
  }

  setUser() {
    const headers = new HttpHeaders().set('Authorization', 'Bearer ' + localStorage.getItem('token'));
    this.http.get<User>(`${environment.apiUrl}/session/me`, {headers}).subscribe(user => {
      this.userSubject.next(user);
    });
  }

  private handleError(error: any) {
    const errorMessages: { [key: string]: string } = {
      'Invalid orderBy parameter': 'Parámetro de ordenación inválido',
      'User not found': 'Usuario no encontrado',
      'Invalid gender. Valid values are: Male, Female, Other, PreferNotToSay': 'Género inválido. Los valores válidos son: Masculino, Femenino, Otro, Prefiero no decirlo',
      'Username, Email or DNI already exists': 'El nombre de usuario, correo electrónico o DNI ya existen',
      'Invalid file type. Only images are allowed': 'Tipo de archivo inválido. Solo se permiten imágenes',
      'Invalid password hash': 'Hash de contraseña inválido',
      'Invalid credentials': 'Credenciales inválidas',
      'Server error': 'Error en el servidor'
    };

    if (error.status === 400 || error.status === 404) {
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
