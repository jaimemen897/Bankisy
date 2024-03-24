import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {map, Observable} from 'rxjs';
import {Pagination} from "./users.service";
import {BankAccount} from "../models/BankAccount";
import {BankAccountCreate} from "../models/BankAccountCreate";
import {Transaction} from "../models/Transaction";

@Injectable({
  providedIn: 'root'
})
export class IndexService {
  private apiUrl = 'http://localhost:5196';

  constructor(private http: HttpClient) {
  }

  getBalanceByIban(iban: string): Observable<number> {
    const url = `${this.apiUrl}/me/bankaccounts/${iban}/balance`;
    return this.http.get<number>(url);
  }

  //TRANSACTIONS, EXPENSES, INCOMES FOR ALL ACCOUNTS OF ONE USER
  getTransactionsByUserId(id: string): Observable<Transaction[]> {
    const url = `${this.apiUrl}/me/bankaccounts/${id}/transactions`;
    return this.http.get<Transaction[]>(url);
  }

  getExpensesByUserId(id: string): Observable<Transaction[]> {
    const url = `${this.apiUrl}/me/bankaccounts/${id}/expenses`;
    return this.http.get<Transaction[]>(url);
  }

  getIncomesByUserId(id: string): Observable<Transaction[]> {
    const url = `${this.apiUrl}/me/bankaccounts/${id}/incomes`;
    return this.http.get<Transaction[]>(url);
  }

  //TRANSACTIONS, EXPENSES, INCOMES FOR ONE ACCOUNT
  getBankAccountTransactions(iban: string): Observable<Transaction[]> {
    return this.http.get<Transaction[]>(`${this.apiUrl}/me/bankaccounts/${iban}/transactions`);
  }

  getBankAccountExpenses(iban: string): Observable<Transaction[]> {
    return this.http.get<Transaction[]>(`${this.apiUrl}/me/bankaccounts/${iban}/expenses`);
  }

  getBankAccountIncomes(iban: string): Observable<Transaction[]> {
    return this.http.get<Transaction[]>(`${this.apiUrl}/me/bankaccounts/${iban}/incomes`);
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
  }
}
