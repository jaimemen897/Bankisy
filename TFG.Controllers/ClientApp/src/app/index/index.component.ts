import {Component, OnInit, ViewChild} from '@angular/core';
import {IndexService} from "../services/index.service";
import {User} from "../models/User";
import {ButtonModule} from "primeng/button";
import {TooltipModule} from "primeng/tooltip";
import {CardModule} from "primeng/card";
import {Transaction} from "../models/Transaction";
import {BankAccount} from "../models/BankAccount";
import {PanelModule} from "primeng/panel";
import {AvatarModule} from "primeng/avatar";
import {MenuModule} from "primeng/menu";
import {ToastModule} from "primeng/toast";
import {TableModule} from "primeng/table";
import {TagModule} from "primeng/tag";
import {BankaccountCreateComponent} from "../bankaccounts/bankaccount-create/bankaccount-create.component";
import {DialogModule} from "primeng/dialog";
import {CreateTransactionComponent} from "../transactions/create/create-transaction.component";
import {CurrencyPipe, DatePipe, NgClass} from "@angular/common";

@Component({
  selector: 'app-index',
  standalone: true,
  imports: [
    ButtonModule,
    TooltipModule,
    CardModule,
    PanelModule,
    AvatarModule,
    MenuModule,
    ToastModule,
    TableModule,
    TagModule,
    BankaccountCreateComponent,
    DialogModule,
    CreateTransactionComponent,
    CurrencyPipe,
    DatePipe,
    NgClass
  ],
  templateUrl: './index.component.html',
  styleUrl: './index.component.css'
})
export class IndexComponent implements OnInit {

  constructor(private indexService: IndexService) {
  }

  @ViewChild(CreateTransactionComponent) transactionCreate!: CreateTransactionComponent
  @ViewChild(BankaccountCreateComponent) bankAccountCreate!: BankaccountCreateComponent

  user!: User;

  totalBalance: number;
  totalIncomes: number;
  totalExpenses: number;
  transactions: Transaction[];
  incomes: Transaction[];
  expenses: Transaction[];
  bankAccounts: BankAccount[];

  displayDialogBankAccount: boolean = false;
  displayDialogTransaction: boolean = false;
  updating: boolean = false;
  items: { label?: string; icon?: string; separator?: boolean, command?: () => void }[];

  ngOnInit(): void {
    this.indexService.getUserByToken().subscribe(user => {
      this.user = user;
      this.getBalanceByUserId(this.user.id);
      this.getIncomesByUserId(this.user.id);
      this.getExpensesByUserId(this.user.id);
      this.getTransactionsByUserId(this.user.id);
      this.getBankAccountsByUserId(this.user.id);
    });

    this.items = [
      {
        label: 'Recargar',
        icon: 'pi pi-refresh',
        command: () => {
          this.refresh();
        }
      },
      {
        label: 'Search',
        icon: 'pi pi-search'
      },
      {
        separator: true
      },
      {
        label: 'Delete',
        icon: 'pi pi-times'
      }
    ];
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

  getBankAccountsByUserId(id: string) {
    this.indexService.getBankAccountsByUserId(id).subscribe(bankAccounts => {
      this.bankAccounts = bankAccounts;
    });
  }

  getAccountName(accountType: string) {
    switch (accountType) {
      case 'Saving':
        return 'Ahorro';
      case 'Current':
        return 'Corriente';
      case 'FixedTerm':
        return 'Plazo fijo';
      case 'Payroll':
        return 'Nómina';
      case 'Student':
        return 'Estudiante';
      default:
        return 'Otro';
    }
  }

  getBalanceColor(balance: number) {
    if (balance > 1000) {
      return 'success';
    } else if (balance > 0) {
      return 'info'
    } else if (balance < 0) {
      return 'danger';
    } else {
      return 'info';
    }
  }

  refresh() {
    this.updating = true;
    this.ngOnInit()
    setTimeout(() => {
      this.updating = false;
    }, 200);
  }

  goToCreateBankAccount() {
    this.displayDialogBankAccount = true;
    this.bankAccountCreate.loadUser(this.user);
  }

  createBankAccount() {
    this.displayDialogBankAccount = false;
    this.ngOnInit();
  }

  createTransaction() {
    this.displayDialogTransaction = false;
    this.transactionCreate.loadUser();
    this.indexService.getBankAccountsByUserId(this.user.id).subscribe(bankAccounts => {
      this.bankAccounts = bankAccounts;
    });
    this.ngOnInit();
  }

  closeDialog() {
    this.displayDialogBankAccount = false;
    this.displayDialogTransaction = false;
  }

  protected readonly console = console;
  protected readonly BankAccount = BankAccount;
}
