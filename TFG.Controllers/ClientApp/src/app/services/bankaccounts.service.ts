import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {catchError, map, Observable, throwError} from 'rxjs';
import {Pagination} from "./users.service";
import {BankAccount} from "../models/BankAccount";
import {BankAccountCreate} from "../models/BankAccountCreate";
import {Transaction} from "../models/Transaction";
import {environment} from "../../environments/environment";
import {MessageService} from "primeng/api";

@Injectable({
  providedIn: 'root'
})
export class BankAccountService {
  private apiUrl = `${environment.apiUrl}/bankaccounts`

  constructor(private http: HttpClient, private messageService: MessageService){
  }

  getBankAccounts(pageNumber: number, pageSize: number, orderBy?: string, descending?: boolean, search?: string, filter?: string, isDeleted?: boolean): Observable<Pagination<BankAccount>> {
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
    return this.http.get<Pagination<BankAccount>>(url).pipe(
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

  getBankAccountById(iban: string): Observable<BankAccount> {
    const url = `${this.apiUrl}/${iban}`;
    return this.http.get<BankAccount>(url).pipe(
      catchError(error => this.handleError(error))
    );
  }

  getBankAccountsByUserId(userId: string): Observable<BankAccount[]> {
    return this.http.get<BankAccount[]>(`${this.apiUrl}/user/${userId}`).pipe(
      catchError(error => this.handleError(error))
    );
  }

  getBankAccountsByMySelf(): Observable<BankAccount[]> {
    return this.http.get<BankAccount[]>(`${this.apiUrl}/my-self/user`).pipe(
      catchError(error => this.handleError(error))
    );
  }

  addBankAccount(BankAccount: BankAccountCreate): Observable<BankAccount> {
    return this.http.post<BankAccount>(this.apiUrl, BankAccount).pipe(
      catchError(error => this.handleError(error))
    );
  }

  addBankAccountMySelf(BankAccount: BankAccountCreate): Observable<BankAccount> {
    return this.http.post<BankAccount>(`${this.apiUrl}/my-self`, BankAccount).pipe(
      catchError(error => this.handleError(error))
    );
  }

  updateBankAccount(BankAccount: BankAccountCreate, iban: string): Observable<BankAccount> {
    return this.http.put<BankAccount>(this.apiUrl + '/' + iban, BankAccount).pipe(
      catchError(error => this.handleError(error))
    );
  }

  deleteBankAccount(iban: string): Observable<BankAccount> {
    return this.http.delete<BankAccount>(this.apiUrl + '/' + iban).pipe(
      catchError(error => this.handleError(error))
    );
  }

  //TRANSACTIONS FOR BANK ACCOUNT
  getTransactionsByIban(iban: string): Observable<Transaction[]> {
    return this.http.get<Transaction[]>(`${this.apiUrl}/${iban}/transactions`).pipe(
      catchError(error => this.handleError(error))
    );
  }

  activateBankAccount(iban: string): Observable<BankAccount> {
    return this.http.put<BankAccount>(`${this.apiUrl}/${iban}/active`, {}).pipe(
      catchError(error => this.handleError(error))
    );
  }

  activeBizumMySelf(iban: string): Observable<BankAccount> {
    return this.http.put<BankAccount>(`${this.apiUrl}/my-self/${iban}/active-bizum`, {}).pipe(
      catchError(error => this.handleError(error))
    );
  }

  private handleError(error: any) {
    const errorMessages: { [key: string]: string } = {
      'BankAccount not found': 'Cuenta bancaria no encontrada',
      'Users not found': 'Usuarios no encontrados',
      'Invalid account type. Valid values are: Saving, Current, FixedTerm, Payroll, Student' :'Tipo de cuenta inválido. Los valores válidos son: Ahorro, Corriente, Plazo fijo, Nómina, Estudiante',
      'You can\'t create a bank account for another user': 'No puedes crear una cuenta bancaria para otro usuario',
      'You can\'t delete a bank account with balance': 'No puedes eliminar una cuenta bancaria con saldo',
      'User not found in bank account': 'Usuario no encontrado en la cuenta bancaria',
      'Server error': 'Error en el servidor',
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
