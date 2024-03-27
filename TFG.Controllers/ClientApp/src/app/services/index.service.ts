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

@Injectable({
  providedIn: 'root'
})
export class IndexService {
  private apiUrl = 'http://localhost:5196/index';

  constructor(private http: HttpClient, private messageService: MessageService) {
  }

  getUserByToken(): Observable<User> {
    const headers = new HttpHeaders().set('token', localStorage.getItem('token') || '');
    return this.http.get<User>('http://localhost:5196/session/me', {headers});
  }

  getTotalBalanceByUserId(id: string): Observable<number> {
    const url = `${this.apiUrl}/${id}/totalbalance`;
    return this.http.get<number>(url);
  }

  /*getTransactionsByUserId(id: string): Observable<Transaction[]> {
    const url = `${this.apiUrl}/${id}/transactions`;
    return this.http.get<Transaction[]>(url);
  }*/

  getTransactionsByUserId(id:string,pageNumber: number, pageSize: number, orderBy?: string, descending?: boolean, search?: string): Observable<Pagination<Transaction>> {
    let url = `${this.apiUrl}/transactions/${id}?pageNumber=${pageNumber}&pageSize=${pageSize}`;
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

  getExpensesByUserId(id: string): Observable<Transaction[]> {
    const url = `${this.apiUrl}/${id}/expenses`;
    return this.http.get<Transaction[]>(url);
  }

  getIncomesByUserId(id: string): Observable<Transaction[]> {
    const url = `${this.apiUrl}/${id}/incomes`;
    return this.http.get<Transaction[]>(url);
  }

  getBankAccountsByUserId(id: string): Observable<BankAccount[]> {
    const url = `${this.apiUrl}/${id}/bankaccounts`;
    return this.http.get<BankAccount[]>(url);
  }

  addBankAccount(BankAccount: BankAccountCreate): Observable<BankAccount> {
    return this.http.post<BankAccount>(this.apiUrl + '/bankaccount', BankAccount).pipe(
      catchError(error => {
        /*Users not found*/
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
        } else {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Ha ocurrido un error inténtelo de nuevo más tarde'
          });
        }
        return throwError(() => error);
      })
    );
  }

  addTransaction(Transaction: TransactionCreate): Observable<Transaction> {
    return this.http.post<Transaction>(this.apiUrl + '/transaction', Transaction).pipe(
      catchError(error => {
        if (error.status === 400) {
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
      })
    );
  }

  /*getBalanceByIban(iban: string): Observable<number> {
    const url = `${this.apiUrl}/me/bankaccounts/${iban}/balance`;
    return this.http.get<number>(url);
  }

  //BANK ACCOUNTS
  getBankAccounts(): Observable<BankAccount[]> {
    const url = `${this.apiUrl}/me/bankaccounts`;
    return this.http.get<BankAccount[]>(url);
  }

  getBankAccountById(iban: string): Observable<BankAccount> {
    const url = `${this.apiUrl}/me/bankaccounts/${iban}`;
    return this.http.get<BankAccount>(url);
  }

  addBankAccount(BankAccount: BankAccountCreate): Observable<BankAccount> {
    return this.http.post<BankAccount>(`${this.apiUrl}/me/bankaccounts`, BankAccount);
  }

  updateBankAccount(BankAccount: BankAccountCreate, iban: string): Observable<BankAccount> {
    return this.http.put<BankAccount>(`${this.apiUrl}/me/bankaccounts/${iban}`, BankAccount);
  }

  deleteBankAccount(iban: string): Observable<BankAccount> {
    return this.http.delete<BankAccount>(`${this.apiUrl}/me/bankaccounts/${iban}`);
  }

  //TRANSACTIONS
  getTransactions(): Observable<Transaction[]> {
    const url = `${this.apiUrl}/me/transactions`;
    return this.http.get<Transaction[]>(url);
  }

  getTransactionById(id: string): Observable<Transaction> {
    const url = `${this.apiUrl}/me/transactions/${id}`;
    return this.http.get<Transaction>(url);
  }

  addTransaction(Transaction: Transaction): Observable<Transaction> {
    return this.http.post<Transaction>(`${this.apiUrl}/me/transactions`, Transaction);
  }*/
}
