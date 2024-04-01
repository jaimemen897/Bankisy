import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {catchError, map, Observable, throwError} from 'rxjs';
import {User} from '../models/User';
import {UserCreate} from "../models/UserCreate";
import {MessageService} from "primeng/api";

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
  private apiUrl = 'http://localhost:5196/users';

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

  uploadAvatar(avatar: FormData, id: string): Observable<any> {
    return this.http.put(this.apiUrl + '/' + id + '/avatar', avatar).pipe(
      catchError(error => this.handleError(error))
    );
  }

  deleteUser(id: string): Observable<User> {
    return this.http.delete<User>(this.apiUrl + '/' + id).pipe(
      catchError(error => this.handleError(error))
    );
  }

  private handleError(error: any) {
    if (error.status === 400) {
      if (error.error.title === 'Invalid orderBy parameter') {
        this.messageService.add({
          severity: 'error',
          summary: 'Error', closable: false, detail: 'Parámetro de ordenación inválido'
        });
      }
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
    } else if (error.status === 404) {
      if (error.error.title === 'User not found') {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          closable: false,
          detail: 'Usuario no encontrado'
        });
      } else {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          closable: false,
          detail: 'Usuario o contraseña incorrectos'
        });
      }
    } else {
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        closable: false,
        detail: 'Ha ocurrido un error inténtelo de nuevo más tarde'
      });
    }
    return throwError(() => error);
  }
}
