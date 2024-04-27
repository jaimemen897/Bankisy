import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {catchError, map, Observable, throwError} from 'rxjs';
import {Pagination} from "./users.service";
import {Transaction} from "../models/Transaction";
import {Card} from '../models/Card';
import {CardCreate} from "../models/CardCreate";
import {MessageService} from "primeng/api";

@Injectable({
  providedIn: 'root'
})
export class CardService {
  private apiUrl = 'http://localhost:5196/card';

  constructor(private http: HttpClient, private messageService: MessageService) {
  }

  getCards(pageNumber: number, pageSize: number, orderBy?: string, descending?: boolean, search?: string, filter?: string, isDeleted?: boolean, isBlocked?: boolean): Observable<Pagination<Card>> {
    let url = `${this.apiUrl}?pageNumber=${pageNumber}&pageSize=${pageSize}`;
    if (descending) {
      url += `&descending=${descending}`;
    }
    if (orderBy) {
      url += `&orderBy=${orderBy}`;
    }
    if (search) {
      url += `&search=${search}`;
    }
    if (filter) {
      url += `&filter=${filter}`;
    }
    if (isDeleted !== undefined && isDeleted !== null) {
      url += `&isDeleted=${!isDeleted}`;
    }
    if (isBlocked !== undefined && isBlocked !== null) {
      url += `&isBlocked=${isBlocked}`;
    }
    return this.http.get<Pagination<Card>>(url).pipe(
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

  getCard(cardNumber: string): Observable<Card> {
    const url = `${this.apiUrl}/${cardNumber}`;
    return this.http.get<Card>(url);
  }

  addCard(card: CardCreate): Observable<Card> {
    return this.http.post<Card>(this.apiUrl, card).pipe(
      catchError(error => this.handleError(error))
    );
  }

  updateCard(card: CardCreate, cardNumber: string): Observable<Card> {
    return this.http.put<Card>(this.apiUrl + '/' + cardNumber, card).pipe(
      catchError(error => this.handleError(error))
    );
  }

  deleteCard(cardNumber: string): Observable<Card> {
    return this.http.delete<Card>(this.apiUrl + '/' + cardNumber);
  }

  renovateCard(cardNumber: string): Observable<Card> {
    return this.http.post<Card>(this.apiUrl + '/' + cardNumber + '/renovate', null).pipe(
      catchError(error => this.handleError(error))
    );
  }

  blockCard(cardNumber: string): Observable<Card> {
    return this.http.post<Card>(this.apiUrl + '/' + cardNumber + '/block', null).pipe(
      catchError(error => this.handleError(error))
    );
  }

  unblockCard(cardNumber: string): Observable<Card> {
    return this.http.post<Card>(this.apiUrl + '/' + cardNumber + '/unblock', null).pipe(
      catchError(error => this.handleError(error))
    );
  }

  activateCard(cardNumber: string): Observable<Card> {
    return this.http.post<Card>(this.apiUrl + '/' + cardNumber + '/activate', null).pipe(
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
