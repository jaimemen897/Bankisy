import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {catchError, map, Observable, throwError} from 'rxjs';
import {Pagination} from "./users.service";
import {Transaction} from "../models/Transaction";
import {TransactionCreate} from "../models/TransactionCreate";
import {environment} from "../../environments/environment";
import {BizumCreate} from "../models/BizumCreate";
import {Bizum} from "../models/Bizum";
import {AccountData} from "../models/AccountData";
import {MessageService} from "primeng/api";

@Injectable({
  providedIn: 'root'
})
export class TransactionsService {
  private apiUrl = `${environment.apiUrl}/transactions`

  constructor(private http: HttpClient, private messageService: MessageService) {
  }

  getTransactions(pageNumber: number, pageSize: number, orderBy: string = 'Id', descending?: boolean, user?: any, search?: string, filter?: string): Observable<Pagination<Transaction>> {
    let url = `${this.apiUrl}?pageNumber=${pageNumber}&pageSize=${pageSize}&orderBy=${orderBy}`;
    if (descending) {
      url += `&descending=${descending ? 'true' : 'false'}`;
    }
    if (user) {
      url += `&user=${JSON.stringify(user)}`;
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
      })),
      catchError(error => this.handleError(error))
    );
  }

  getTransactionsByMyself(pageNumber: number, pageSize: number, orderBy: string = 'Id', descending?: boolean, search?: string, filter?: string): Observable<Pagination<Transaction>> {
    let url = `${this.apiUrl}/myself?pageNumber=${pageNumber}&pageSize=${pageSize}&orderBy=${orderBy}`;
    if (descending) {
      url += `&descending=${descending ? 'true' : 'false'}`;
    }
    if (search) {
      url += `&search=${search}`;
    }
    if (filter) {
      url += `&filter=${filter}`;
    }

    return this.http.get<Pagination<Transaction>>(url).pipe(
      //handle error
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

  getTransactionsByIban(iban: string): Observable<Transaction[]> {
    return this.http.get<Transaction[]>(`${this.apiUrl}/${iban}/transactions`).pipe(
      catchError(error => this.handleError(error))
    );
  }

  getMoneySummaryByUserId(): Observable<AccountData> {
    return this.http.get<AccountData>(`${this.apiUrl}/summary`).pipe(
      catchError(error => this.handleError(error))
    );
  }

  createTransaction(Transaction: TransactionCreate): Observable<Transaction> {
    return this.http.post<Transaction>(this.apiUrl, Transaction).pipe(
      catchError(error => this.handleError(error))
    );
  }

  createBizum(bizumCreate: BizumCreate): Observable<Bizum> {
    return this.http.post<Bizum>(`${this.apiUrl}/bizum`, bizumCreate).pipe(
      catchError(error => this.handleError(error))
    );
  }

  deleteTransaction(id: string): Observable<Transaction> {
    return this.http.delete<Transaction>(this.apiUrl + '/' + id).pipe(
      catchError(error => this.handleError(error))
    );
  }

  private handleError(error: any) {
    const errorMessages: { [key: string]: string } = {
      'User not found': 'Usuario no encontrado',
      'Account not found': 'Cuenta no encontrada',
      'Account origin not found or not accepting Bizum': 'Debes activar bizum en una de tus cuentas',
      'Transaction not found': 'Transacción no encontrada',
      'Account origin not found': 'Cuenta de origen no encontrada',
      'Account destination not found': 'Cuenta de destino no encontrada',
      'Account destination not found or not accepting Bizum' : 'El usuario introducido no tiene el servicio Bizum activado',
      'Origin and destination users cannot be the same': 'El usuario de origen y el de destino no pueden ser el mismo',
      'Insufficient funds in the origin account': 'Fondos insuficientes en la cuenta de origen',
      'Transaction amount must be greater than zero': 'El monto de la transacción debe ser mayor que cero',
      'Origin and destination accounts cannot be the same': 'Las cuentas de origen y destino no pueden ser las mismas',
      'You are not the owner of the account': 'No eres el propietario de la cuenta',
      'Server error': 'Error en el servidor'
    };

    if (error.status === 400 || error.status === 404) {
      const message = errorMessages[error.error.title];
      if (message) {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          closable: false,
          detail: message,
          life: 2000
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
