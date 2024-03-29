import {Injectable} from '@angular/core';
import {HttpClient, HttpHeaders} from '@angular/common/http';
import {catchError, map, Observable, throwError} from 'rxjs';
import {BankAccount} from "../models/BankAccount";
import {BankAccountCreate} from "../models/BankAccountCreate";
import {Transaction} from "../models/Transaction";
import {User} from "../models/User";
import {TransactionCreate} from "../models/TransactionCreate";
import {MessageService} from "primeng/api";
import {Pagination} from "./users.service";
import {Card} from "../models/Card";
import {CardCreate} from "../models/CardCreate";

@Injectable({
  providedIn: 'root'
})
export class IndexService {
  private apiUrl = 'http://localhost:5196/index';

  constructor(private http: HttpClient, private messageService: MessageService) {
  }

  getUserByToken(): Observable<User> {
    const headers = new HttpHeaders().set('Authorization', 'Bearer ' + localStorage.getItem('token'));
    return this.http.get<User>('http://localhost:5196/session/me', {headers});
  }

  getTotalBalanceByUserId(): Observable<number> {
    const url = `${this.apiUrl}/totalbalance`;
    return this.http.get<number>(url);
  }

  getTransactionsByUserId(pageNumber: number, pageSize: number, orderBy?: string, descending?: boolean, search?: string): Observable<Pagination<Transaction>> {
    let url = `${this.apiUrl}/transactions?pageNumber=${pageNumber}&pageSize=${pageSize}`;
    if (orderBy) {
      url += `&orderBy=${orderBy}`;
    }
    if (descending) {
      url += `&descending=${descending}`;
    }
    if (search) {
      url += `&search=${search}`;
    }
    return this.http.get<Pagination<Transaction>>(url).pipe(
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

  getExpensesByUserId(): Observable<Transaction[]> {
    const url = `${this.apiUrl}/expenses`;
    return this.http.get<Transaction[]>(url);
  }

  getIncomesByUserId(): Observable<Transaction[]> {
    const url = `${this.apiUrl}/incomes`;
    return this.http.get<Transaction[]>(url);
  }

  getBankAccountsByUserId(): Observable<BankAccount[]> {
    const url = `${this.apiUrl}/bankaccounts`;
    return this.http.get<BankAccount[]>(url);
  }

  addBankAccount(BankAccount: BankAccountCreate): Observable<BankAccount> {
    return this.http.post<BankAccount>(this.apiUrl + '/bankaccount', BankAccount).pipe(
      catchError(error => this.handleError(error))
    );
  }

  addTransaction(Transaction: TransactionCreate): Observable<Transaction> {
    return this.http.post<Transaction>(this.apiUrl + '/transaction', Transaction).pipe(
      catchError(error => this.handleError(error))
    );
  }

  getTransactionsByIban(iban: string): Observable<Transaction[]> {
    return this.http.get<Transaction[]>(`${this.apiUrl}/${iban}/transactions`);
  }

  getCardByCardNumber(cardNumber: string): Observable<Card> {
    return this.http.get<Card>(`${this.apiUrl}/card/${cardNumber}`);
  }

  createCard(CardCreate: CardCreate): Observable<Card> {
    return this.http.post<Card>(`${this.apiUrl}/card`, CardCreate).pipe(
      catchError(error => this.handleError(error))
    );
  }

  updateCard(cardNumber: string, cardUpdateDto: CardCreate): Observable<Card> {
    return this.http.put<Card>(`${this.apiUrl}/card/${cardNumber}`, cardUpdateDto).pipe(
      catchError(error => this.handleError(error))
    );
  }

  deleteCard(cardNumber: string): Observable<{}> {
    return this.http.delete(`${this.apiUrl}/card/${cardNumber}`).pipe(
      catchError(error => this.handleError(error))
    );
  }

  renovateCard(cardNumber: string): Observable<Card> {
    return this.http.post<Card>(`${this.apiUrl}/card/${cardNumber}/renovate`, {}).pipe(
      catchError(error => this.handleError(error))
    );
  }

  blockCard(cardNumber: string): Observable<{}> {
    return this.http.post(`${this.apiUrl}/card/${cardNumber}/block`, {}).pipe(
      catchError(error => this.handleError(error))
    );
  }

  unblockCard(cardNumber: string): Observable<{}> {
    return this.http.post(`${this.apiUrl}/card/${cardNumber}/unblock`, {}).pipe(
      catchError(error => this.handleError(error))
    );
  }

  activateCard(cardNumber: string): Observable<{}> {
    return this.http.post(`${this.apiUrl}/card/${cardNumber}/activate`, {}).pipe(
      catchError(error => this.handleError(error))
    );
  }

  getCardsByUserId(): Observable<Card[]> {
    return this.http.get<Card[]>(`${this.apiUrl}/cards/user`);
  }

  getCardsByIban(iban: string): Observable<Card[]> {
    return this.http.get<Card[]>(`${this.apiUrl}/cards/bankaccount/${iban}`);
  }

  private handleError(error: any) {
    if (error.status === 400) {
      if (error.error.title === 'Users not found') {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Uno o varios de los usuarios no existen'
        });
      }
      if (error.error.title === 'Invalid account type. Valid values are: Saving, Current, FixedTerm, Payroll, Student') {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Tipo de cuenta inválido. Los valores válidos son: Ahorro, Corriente, PlazoFijo, Nómina, Estudiante'
        });
      }
      if (error.error.title === 'Insufficient funds in the origin account') {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Fondos insuficientes en la cuenta de origen'
        });
      }
      if (error.error.title === 'Origin and destination accounts cannot be the same') {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Las cuentas de origen y destino no pueden ser iguales'
        });
      }
      if (error.error.title === 'Transaction amount must be greater than zero') {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'La cantidad de la transacción debe ser mayor que cero'
        });
      }
    } else if (error.status === 404) {
      if (error.error.title === 'Account origin not found') {
        this.messageService.add({severity: 'error', summary: 'Error', detail: 'Cuenta de origen no encontrada'});
      }
      if (error.error.title === 'Account destination not found') {
        this.messageService.add({severity: 'error', summary: 'Error', detail: 'Cuenta de destino no encontrada'});
      }
    } else {
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Ha ocurrido un error inténtelo de nuevo más tarde'
      });
    }
    return throwError(() => error);
  }
}
