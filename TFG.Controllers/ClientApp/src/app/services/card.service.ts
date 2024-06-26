import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {catchError, map, Observable, throwError} from 'rxjs';
import {Pagination} from "./users.service";
import {Card} from '../models/Card';
import {CardCreate} from "../models/CardCreate";
import {MessageService} from "primeng/api";
import {environment} from "../../environments/environment";

@Injectable({
  providedIn: 'root'
})
export class CardService {
  private apiUrl = `${environment.apiUrl}/card`

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
      })),
      catchError(error => this.handleError(error))
    );
  }

  getCard(cardNumber: string): Observable<Card> {
    const url = `${this.apiUrl}/${cardNumber}`;
    return this.http.get<Card>(url).pipe(
      catchError(error => this.handleError(error))
    );
  }

  getMyCards(): Observable<Card[]> {
    return this.http.get<Card[]>(this.apiUrl + '/my-cards').pipe(
      catchError(error => this.handleError(error))
    );
  }

  createCard(card: CardCreate): Observable<Card> {
    return this.http.post<Card>(this.apiUrl, card).pipe(
      catchError(error => this.handleError(error))
    );
  }

  createCardForMySelf(card: CardCreate): Observable<Card> {
    return this.http.post<Card>(this.apiUrl + '/my-card', card).pipe(
      catchError(error => this.handleError(error))
    );
  }

  updateCard(card: CardCreate, cardNumber: string): Observable<Card> {
    return this.http.put<Card>(this.apiUrl + '/' + cardNumber, card).pipe(
      catchError(error => this.handleError(error))
    );
  }

  updateCardForMySelf(card: CardCreate, cardNumber: string): Observable<Card> {
    return this.http.put<Card>(this.apiUrl + '/my-card/' + cardNumber, card).pipe(
      catchError(error => this.handleError(error))
    );
  }

  deleteCard(cardNumber: string): Observable<Card> {
    return this.http.delete<Card>(this.apiUrl + '/' + cardNumber);
  }

  deleteMyCard(cardNumber: string): Observable<Card> {
    return this.http.delete<Card>(this.apiUrl + '/my-card/' + cardNumber).pipe(
      catchError(error => this.handleError(error))
    );
  }

  renovateCard(cardNumber: string): Observable<Card> {
    return this.http.post<Card>(this.apiUrl + '/' + cardNumber + '/renovate', null).pipe(
      catchError(error => this.handleError(error))
    );
  }

  renovateMyCard(cardNumber: string): Observable<Card> {
    return this.http.post<Card>(this.apiUrl + '/my-card/' + cardNumber + '/renovate', null).pipe(
      catchError(error => this.handleError(error))
    );
  }

  blockCard(cardNumber: string): Observable<Card> {
    return this.http.post<Card>(this.apiUrl + '/' + cardNumber + '/block', null).pipe(
      catchError(error => this.handleError(error))
    );
  }

  blockMyCard(cardNumber: string): Observable<Card> {
    return this.http.post<Card>(this.apiUrl + '/my-card/' + cardNumber + '/block', null).pipe(
      catchError(error => this.handleError(error))
    );
  }

  unblockCard(cardNumber: string): Observable<Card> {
    return this.http.post<Card>(this.apiUrl + '/' + cardNumber + '/unblock', null).pipe(
      catchError(error => this.handleError(error))
    );
  }

  unblockMyCard(cardNumber: string): Observable<Card> {
    return this.http.post<Card>(this.apiUrl + '/my-card/' + cardNumber + '/unblock', null).pipe(
      catchError(error => this.handleError(error))
    );
  }

  activateCard(cardNumber: string): Observable<Card> {
    return this.http.post<Card>(this.apiUrl + '/' + cardNumber + '/activate', null).pipe(
      catchError(error => this.handleError(error))
    );
  }

  private handleError(error: any) {
    const errorMessages: { [key: string]: string } = {
      'Invalid orderBy parameter': 'Parámetro de ordenación inválido',
      'Card not found': 'Tarjeta no encontrada',
      'Cards not found': 'Tarjetas no encontradas',
      'User not found': 'Usuario no encontrado',
      'You are not the owner of the card': 'No eres el propietario de la tarjeta',
      'Bank account not found': 'Cuenta bancaria no encontrada',
      'Bank account does not belong to the user': 'La cuenta bancaria no pertenece al usuario',
      'Bank account already has a card': 'La cuenta bancaria ya tiene una tarjeta',
      'Invalid card type. Valid values are: Debit, Visa, Credit, Prepaid, Virtual, Mastercard, AmericanExpress': 'Tipo de tarjeta inválido. Los valores válidos son: Débito, Visa, Crédito, Prepago, Virtual, Mastercard, AmericanExpress',
      'Card is already blocked': 'La tarjeta ya está bloqueada',
      'Card is not blocked': 'La tarjeta no está bloqueada',
      'Card is not expired': 'La tarjeta no está vencida',
      'Server error': 'Error en el servidor'
    };

    if (error.status === 400 || error.status === 404) {
      const message = errorMessages[error.error.title];
      if (message) {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          closable: false,
          detail: message
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
