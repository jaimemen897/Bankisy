import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {map, Observable} from 'rxjs';
import {BankAccount, BankAccountCreate} from "./bankaccounts.component";
import {Pagination} from "../users/users.service";

@Injectable({
  providedIn: 'root'
})
export class BankAccountService {
  private apiUrl = 'http://localhost:5196/bankaccounts';

  constructor(private http: HttpClient) {
  }

  getBankAccounts(pageNumber: number, pageSize: number, orderBy?: string, descending?: boolean, search?: string, filter?: string): Observable<Pagination<BankAccount>> {
    let url = `${this.apiUrl}?pageNumber=${pageNumber}&pageSize=${pageSize}`;
    console.log(url);
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

  getBankAccountById(id: string): Observable<BankAccount> {
    const url = `${this.apiUrl}/${id}`;
    return this.http.get<BankAccount>(url);
  }

  addBankAccount(BankAccount: BankAccountCreate): Observable<BankAccount> {
    return this.http.post<BankAccount>(this.apiUrl, BankAccount);
  }

  updateBankAccount(BankAccount: BankAccountCreate, id: string): Observable<BankAccount> {
    return this.http.put<BankAccount>(this.apiUrl + '/' + id, BankAccount);
  }

  deleteBankAccount(id: string): Observable<BankAccount> {
    return this.http.delete<BankAccount>(this.apiUrl + '/' + id);
  }
}
