import {Injectable} from '@angular/core';
import {HttpClient, HttpHeaders} from '@angular/common/http';
import {map, Observable} from 'rxjs';
import {Pagination} from "./users.service";
import {BankAccount} from "../models/BankAccount";
import {BankAccountCreate} from "../models/BankAccountCreate";
import {Transaction} from "../models/Transaction";
import {User} from "../models/User";

@Injectable({
  providedIn: 'root'
})
export class IndexService {
  private apiUrl = 'http://localhost:5196/index';

  constructor(private http: HttpClient) {
  }

  getUserByToken(): Observable<User> {
    const headers = new HttpHeaders().set('token', localStorage.getItem('token') || '');
    return this.http.get<User>('http://localhost:5196/session/me', {headers});
  }

  getTotalBalanceByUserId(id: string): Observable<number> {
    const url = `${this.apiUrl}/user/${id}/totalbalance`;
    return this.http.get<number>(url);
  }

  getTransactionsByUserId(id: string): Observable<Transaction[]> {
    const url = `${this.apiUrl}/user/${id}/transactions`;
    return this.http.get<Transaction[]>(url);
  }

  getExpensesByUserId(id: string): Observable<Transaction[]> {
    const url = `${this.apiUrl}/user/${id}/expenses`;
    return this.http.get<Transaction[]>(url);
  }

  getIncomesByUserId(id: string): Observable<Transaction[]> {
    const url = `${this.apiUrl}/user/${id}/incomes`;
    return this.http.get<Transaction[]>(url);
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
