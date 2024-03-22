import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {map, Observable} from 'rxjs';
import {User} from '../models/User';
import {UserCreate} from "../models/UserCreate";

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

  constructor(private http: HttpClient) {
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

  getUserById(id: string): Observable<User> {
    const url = `${this.apiUrl}/${id}`;
    return this.http.get<User>(url);
  }

  addUser(user: UserCreate): Observable<User> {
    return this.http.post<User>(this.apiUrl, user);
  }

  updateUser(user: UserCreate, id: string): Observable<User> {
    return this.http.put<User>(this.apiUrl + '/' + id, user);
  }

  deleteUser(id: string): Observable<User> {
    return this.http.delete<User>(this.apiUrl + '/' + id);
  }
}
