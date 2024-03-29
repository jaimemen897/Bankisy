import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {map, Observable} from 'rxjs';
import {Pagination} from "./users.service";
import {Transaction} from "../models/Transaction";
import { Card } from '../models/Card';
import {CardCreate} from "../models/CardCreate";

@Injectable({
  providedIn: 'root'
})
export class CardService {
  private apiUrl = 'http://localhost:5196/card';

  constructor(private http: HttpClient) {
  }

  getCards(pageNumber: number, pageSize: number, orderBy?: string, descending?: boolean, search?: string, filter?: string): Observable<Pagination<Card>> {
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
    return this.http.post<Card>(this.apiUrl, card);
  }

  updateCard(card: CardCreate, cardNumber: string): Observable<Card> {
    return this.http.put<Card>(this.apiUrl + '/' + cardNumber, card);
  }

  deleteCard(cardNumber: string): Observable<Card> {
    return this.http.delete<Card>(this.apiUrl + '/' + cardNumber);
  }

  renovateCard(cardNumber: string): Observable<Card> {
    return this.http.put<Card>(this.apiUrl + cardNumber + '/renovate', null);
  }

  blockCard(cardNumber: string): Observable<Card> {
    return this.http.put<Card>(this.apiUrl + cardNumber + '/block', null);
  }

  unblockCard(cardNumber: string): Observable<Card> {
    return this.http.put<Card>(this.apiUrl + cardNumber + '/unblock', null);
  }

  activateCard(cardNumber: string): Observable<Card> {
    return this.http.put<Card>(this.apiUrl + cardNumber + '/activate', null);
  }
}
