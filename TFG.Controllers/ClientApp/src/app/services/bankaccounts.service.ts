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
export class BankAccountService {
  private apiUrl = 'http://localhost:5196/bankaccounts';

  constructor(private http: HttpClient) {
  }

  getBankAccounts(pageNumber: number, pageSize: number, orderBy?: string, descending?: boolean, search?: string, filter?: string): Observable<Pagination<BankAccount>> {
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
    return this.http.get<Pagination<BankAccount>>(url).pipe(
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

  getBankAccountById(iban: string): Observable<BankAccount> {
    const url = `${this.apiUrl}/${iban}`;
    return this.http.get<BankAccount>(url);
  }

  addBankAccount(BankAccount: BankAccountCreate): Observable<BankAccount> {
    return this.http.post<BankAccount>(this.apiUrl, BankAccount);
  }

  updateBankAccount(BankAccount: BankAccountCreate, iban: string): Observable<BankAccount> {
    return this.http.put<BankAccount>(this.apiUrl + '/' + iban, BankAccount);
  }

  deleteBankAccount(iban: string): Observable<BankAccount> {
    return this.http.delete<BankAccount>(this.apiUrl + '/' + iban);
  }

  //TRANSACTIONS FOR BANK ACCOUNT
  getTransactionsByIban(iban: string): Observable<Transaction[]>{
    return this.http.get<Transaction[]>(`${this.apiUrl}/${iban}/transactions`);
  }
}
