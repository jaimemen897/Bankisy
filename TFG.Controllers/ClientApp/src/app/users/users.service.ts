import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private apiUrl = 'http://localhost:5196/users';

  constructor(private http: HttpClient) { }

  getUsers(page: number, pageSize: number): Observable<any> {
    const url = `${this.apiUrl}?page=${page}&pageSize=${pageSize}`;
    return this.http.get(url);
  }

  getAllUsers(): Observable<any> {
    const url = `${this.apiUrl}/all`;
    return this.http.get(url);
  }


}
