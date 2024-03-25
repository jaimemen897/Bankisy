import {Component, OnInit} from '@angular/core';
import {IndexService} from "../services/index.service";
import {User} from "../models/User";
import {ButtonModule} from "primeng/button";
import {TooltipModule} from "primeng/tooltip";
import {UserService} from "../services/users.service";
import {BankAccountService} from "../services/bankaccounts.service";
import {CardModule} from "primeng/card";
import {Transaction} from "../models/Transaction";


@Component({
  selector: 'app-index',
  standalone: true,
  imports: [
    ButtonModule,
    TooltipModule,
    CardModule
  ],
  templateUrl: './index.component.html',
  styleUrl: './index.component.css'
})
export class IndexComponent implements OnInit {

  constructor(private indexService: IndexService, private userService: UserService) {
  }

  user!: User;

  totalBalance: number;
  totalIncomes: number;
  totalExpenses: number;
  transactions: Transaction[];
  incomes: Transaction[];
  expenses: Transaction[];

  ngOnInit(): void {
    this.indexService.getUserByToken().subscribe(user => {
      this.user = user;
      this.getBalanceByUserId(this.user.id);
      this.getIncomesByUserId(this.user.id);
      this.getExpensesByUserId(this.user.id);
    });
  }

  getBalanceByUserId(id: string) {
    this.indexService.getTotalBalanceByUserId(id).subscribe(balance => {
      this.totalBalance = balance;
    });
  }

  getTransactionsByUserId(id: string) {
    this.indexService.getTransactionsByUserId(id).subscribe(transactions => {
      this.transactions = transactions;
    });
  }

  getIncomesByUserId(id: string) {
    this.indexService.getIncomesByUserId(id).subscribe(incomes => {
      this.incomes = incomes;
      this.totalIncomes = incomes.reduce((acc, income) => acc + income.amount, 0);
    });
  }

  getExpensesByUserId(id: string) {
    this.indexService.getExpensesByUserId(id).subscribe(expenses => {
      this.expenses = expenses;
      this.totalExpenses = expenses.reduce((acc, expense) => acc + expense.amount, 0);
    });
  }
}
