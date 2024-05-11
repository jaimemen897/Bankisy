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
import {UserCreate} from "../models/UserCreate";
import {BizumCreate} from "../models/BizumCreate";
import {Bizum} from "../models/Bizum";
import {environment} from "../../environments/environment";

@Injectable({
  providedIn: 'root'
})
export class IndexService {
  private apiUrl = `${environment.apiUrl}/index`

  constructor(private http: HttpClient, private messageService: MessageService) {
  }

  getUserByToken(): Observable<User> {
    const headers = new HttpHeaders().set('Authorization', 'Bearer ' + localStorage.getItem('token'));
    return this.http.get<User>(`${environment.apiUrl}/session/me`, {headers});
  }

  getTotalBalanceByUserId(): Observable<number> {
    const url = `${this.apiUrl}/totalbalance`;
    return this.http.get<number>(url);
  }

  getTransactionsByUserId(pageNumber: number, pageSize: number, orderBy?: string, descending?: boolean, search?: string, filter?: string): Observable<Pagination<Transaction>> {
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
    if (filter) {
      url += `&filter=${filter}`;
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

  //PROFILE
  updateProfile(user: UserCreate): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/profile`, user).pipe(
      catchError(error => this.handleError(error))
    );
  }

  deleteAvatar(): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/avatar`).pipe(
      catchError(error => this.handleError(error))
    );
  }

  activeBizum(iban: string): Observable<{}> {
    return this.http.post(`${this.apiUrl}/bankaccount/${iban}/active-bizum`, {}).pipe(
      catchError(error => this.handleError(error))
    );
  }

  createBizum(bizumCreate: BizumCreate): Observable<Bizum> {
    return this.http.post<Bizum>(`${this.apiUrl}/transaction/bizum`, bizumCreate).pipe(
      catchError(error => this.handleError(error))
    );
  }

  private handleError(error: any) {
    const errorMessages: { [key: string]: string } = {
      'User not found': 'Usuario no encontrado',
      'Bank account not found': 'Cuenta bancaria no encontrada',
      'Bank account does not belong to the user': 'La cuenta bancaria no pertenece al usuario',
      'Bank account already has a card': 'La cuenta bancaria ya tiene una tarjeta',
      'Invalid card type. Valid values are: Debit, Visa, Credit, Prepaid, Virtual, Mastercard, AmericanExpress': 'Tipo de tarjeta inválido. Los valores válidos son: Débito, Visa, Crédito, Prepago, Virtual, Mastercard, AmericanExpress',
      'Card is already blocked': 'La tarjeta ya está bloqueada',
      'Card is not blocked': 'La tarjeta no está bloqueada',
      'Users not found': 'Uno o varios de los usuarios no existen',
      'Invalid account type. Valid values are: Saving, Current, FixedTerm, Payroll, Student': 'Tipo de cuenta inválido. Los valores válidos son: Ahorro, Corriente, PlazoFijo, Nómina, Estudiante',
      'You are not the owner of the card': 'No eres el propietario de la tarjeta',
      'Insufficient funds in the origin account': 'Fondos insuficientes en la cuenta de origen',
      'Origin and destination accounts cannot be the same': 'Las cuentas de origen y destino no pueden ser iguales',
      'Transaction amount must be greater than zero': 'La cantidad de la transacción debe ser mayor que cero',
      'Account origin not found or not accepting Bizum': 'Cuenta de origen no encontrada o no acepta Bizum',
      'User destination not found': 'Usuario de destino no encontrado',
      'Account destination not found or not accepting Bizum': 'Cuenta de destino no encontrada o no acepta Bizum',
      'You are not the owner of the account': 'No eres el propietario de la cuenta',
      'Card is not expired': 'La tarjeta no está caducada',
      'Account origin not found': 'Cuenta de origen no encontrada',
      'Account destination not found': 'Cuenta de destino no encontrada'
    };

    if (error.status === 400 || error.status === 404) {
      const message = errorMessages[error.error.title];
      if (message) {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          closable: false,
          life: 2000,
          detail: message
        });
      }
    }

    return throwError(() => error);
  }
}
