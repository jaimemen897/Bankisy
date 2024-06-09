import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {Observable} from "rxjs";
import {environment} from "../../environments/environment";

@Injectable({
  providedIn: 'root'
})
export class CheckoutService {

  private apiUrl = `${environment.apiUrl}/create-checkout-session`

  constructor(private http: HttpClient) {
  }

  createCheckoutSession(amount: number, iban: string): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${this.apiUrl}`, {amount: amount, iban: iban});
  }
}
