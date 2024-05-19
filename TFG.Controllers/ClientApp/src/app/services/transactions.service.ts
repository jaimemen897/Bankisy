import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {map, Observable} from 'rxjs';
import {Pagination} from "./users.service";
import {Transaction} from "../models/Transaction";
import {TransactionCreate} from "../models/TransactionCreate";
import {environment} from "../../environments/environment";

@Injectable({
  providedIn: 'root'
})
export class TransactionsService {
  private apiUrl = `${environment.apiUrl}/transactions`

  constructor(private http: HttpClient) {
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
      }))
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

  getMyIncomes(){
    return this.http.get<Transaction[]>(`${this.apiUrl}/myself/incomes`);
  }

  getMyExpenses(){
    return this.http.get<Transaction[]>(`${this.apiUrl}/myself/expenses`);
  }

  addTransaction(Transaction: TransactionCreate): Observable<Transaction> {
    return this.http.post<Transaction>(this.apiUrl, Transaction);
  }

  deleteTransaction(id: string): Observable<Transaction> {
    return this.http.delete<Transaction>(this.apiUrl + '/' + id);
  }
}
