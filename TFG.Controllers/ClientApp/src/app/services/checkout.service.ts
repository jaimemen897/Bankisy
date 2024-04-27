import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import {Observable} from "rxjs";

@Injectable({
  providedIn: 'root'
})
export class CheckoutService {

  private apiUrl = 'http://localhost:5196/create-checkout-session';

  constructor(private http: HttpClient) { }

  createCheckoutSession(amount: number): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${this.apiUrl}`, { amount: amount });
  }
}
