import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {catchError, Observable, throwError} from 'rxjs';
import {MessageService} from "primeng/api";
import {DepositCreate} from "../models/DepositCreate";

@Injectable({
  providedIn: 'root'
})
export class DepositService {
  private apiUrl = 'http://localhost:5196/stripe';

  constructor(private http: HttpClient, private messageService: MessageService) {
  }

  deposit(depositCreate: DepositCreate): Observable<DepositCreate> {
    return this.http.post<DepositCreate>(this.apiUrl + '/deposit', depositCreate).pipe(
      catchError(error => this.handleError(error))
    );
  }

  private handleError(error: any) {
    if (error.status === 400) {
      if (error.error.title === 'User not found') {
        this.messageService.add({
          severity: 'error',
          summary: 'Error', closable: false, detail: 'Usuario no encontrado'
        });
      }
      if (error.error.title === 'Bank account not found') {
        this.messageService.add({
          severity: 'error',
          summary: 'Error', closable: false, detail: 'Cuenta bancaria no encontrada'
        });
      }
      if (error.error.title === 'Bank account does not belong to the user') {
        this.messageService.add({
          severity: 'error',
          summary: 'Error', closable: false, detail: 'La cuenta bancaria no pertenece al usuario'
        });
      }
      if (error.error.title === 'Bank account already has a card') {
        this.messageService.add({
          severity: 'error',
          summary: 'Error', closable: false, detail: 'La cuenta bancaria ya tiene una tarjeta'
        });
      }
      if (error.error.title === 'Invalid card type. Valid values are: Debit, Visa, Credit, Prepaid, Virtual, Mastercard, AmericanExpress') {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          closable: false,
          detail: 'Tipo de tarjeta inválido. Los valores válidos son: Débito, Visa, Crédito, Prepago, Virtual, Mastercard, AmericanExpress'
        });
      }
      if (error.error.title === 'Card is already blocked') {
        this.messageService.add({
          severity: 'error',
          summary: 'Error', closable: false, detail: 'La tarjeta ya está bloqueada'
        });
      }
      if (error.error.title === 'Card is not blocked') {
        this.messageService.add({
          severity: 'error',
          summary: 'Error', closable: false, detail: 'La tarjeta no está bloqueada'
        });
      }
      if (error.error.title === 'Card is not expired') {
        this.messageService.add({
          severity: 'error',
          summary: 'Error', closable: false, detail: 'La tarjeta no está caducada'
        });
      }
    } else if (error.status === 404) {
      if (error.error.title === 'Account origin not found') {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          closable: false,
          detail: 'Cuenta de origen no encontrada'
        });
      }
      if (error.error.title === 'Account destination not found') {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          closable: false,
          detail: 'Cuenta de destino no encontrada'
        });
      }
    }
    return throwError(() => error);
  }
}
