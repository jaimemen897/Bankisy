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
import {OverlayPanelModule} from "primeng/overlaypanel";
import {MenuItem, MessageService} from "primeng/api";
import {ScrollPanelModule} from "primeng/scrollpanel";


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
    NgClass,
    OverlayPanelModule,
    ScrollPanelModule
  ],
  templateUrl: './index.component.html',
  styleUrl: './index.component.css'
})
export class IndexComponent implements OnInit {

  constructor(private indexService: IndexService, private messageService: MessageService) {
  }

  @ViewChild(CreateTransactionComponent) transactionCreate!: CreateTransactionComponent
  @ViewChild(BankaccountCreateComponent) bankAccountCreate!: BankaccountCreateComponent
  @ViewChild('transactionPanel') transactionPanel!: any;

  user!: User;

  rows: number = 5;
  totalRecords: number = 0;
  search: string;
  filter!: string;
  sortField!: string;
  sortOrder!: number;

  totalBalance: number;
  totalIncomes: number;
  totalExpenses: number;
  transactions: Transaction[];
  transactionsByBankAccount: Transaction[];
  incomes: Transaction[];
  expenses: Transaction[];
  bankAccounts: BankAccount[];

  displayDialogBankAccount: boolean = false;
  displayDialogTransaction: boolean = false;
  updating: boolean = false;
  items: MenuItem[];

  //LOAD
  ngOnInit(): void {
    this.indexService.getUserByToken().subscribe(user => {
      this.user = user;
      this.getBalanceByUserId();
      this.getIncomesByUserId();
      this.getExpensesByUserId();
      this.getBankAccountsByUserId();
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

  lazyLoad(event: any) {
    let pageNumber = Math.floor(event.first / event.rows) + 1;
    let sortField = event.sortField;
    let sortOrder = event.sortOrder;

    this.indexService.getTransactionsByUserId(pageNumber, event.rows, sortField, sortOrder === -1, this.search).subscribe(data => {
      this.transactions = data.items;
      this.totalRecords = data.totalCount;
    });
  }

  //GET DATA
  getBalanceByUserId() {
    this.indexService.getTotalBalanceByUserId().subscribe(balance => {
      this.totalBalance = balance;
    });
  }

  getIncomesByUserId() {
    this.indexService.getIncomesByUserId().subscribe(incomes => {
      this.incomes = incomes;
      this.totalIncomes = incomes.reduce((acc, income) => acc + income.amount, 0);
    });
  }

  getExpensesByUserId() {
    this.indexService.getExpensesByUserId().subscribe(expenses => {
      this.expenses = expenses;
      this.totalExpenses = expenses.reduce((acc, expense) => acc + expense.amount, 0);
    });
  }

  getBankAccountsByUserId() {
    this.indexService.getBankAccountsByUserId().subscribe(bankAccounts => {
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
    } else if (balance == 0) {
      return 'warning';
    } else if (balance < 0) {
      return 'danger';
    } else {
      return 'info';
    }
  }

  getTransactionColor(transaction: Transaction) {
    if(this.bankAccounts.find(bankAccount => bankAccount.iban === transaction.ibanAccountOrigin) !== undefined){
      return 'warning';
    } else if(this.bankAccounts.find(bankAccount => bankAccount.iban === transaction.ibanAccountDestination) !== undefined){
      return 'success';
    } else {
      return 'info';
    }
  }

  getTransactionsByIban(iban: string) {
    this.transactionsByBankAccount = [];
    this.indexService.getTransactionsByIban(iban).subscribe(data => {
      this.transactionsByBankAccount = data;
      if (this.transactionsByBankAccount.length !== 0){
      } else {
        this.messageService.add({
          severity: 'info',
          summary: 'Sin transacciones',
          detail: 'No hay transacciones en esta cuenta',
          life: 2000,
          closable: false
        });
      }
    });
  }

  //ORDER, FILTER, PAGINATE
  refresh() {
    this.updating = true;
    this.ngOnInit()
    this.lazyLoad({first: 0, rows: this.rows, sortField: this.sortField, sortOrder: this.sortOrder})
    setTimeout(() => {
      this.updating = false;
    }, 200);
  }

  onSortChange(event: any) {
    let value = event.value;

    if (value.indexOf('!') === 0) {
      this.sortOrder = -1;
      this.sortField = value.substring(1, value.length);
    } else {
      this.sortOrder = 1;
      this.sortField = value;
    }
  }

  clearOrders() {
    this.refresh();
    this.sortField = '';
    this.sortOrder = 1;
  }

  clearFilters() {
    this.search = '';
    this.filter = '';

    this.refresh();
  }

  //CREATE
  goToCreateBankAccount() {
    this.displayDialogBankAccount = true;
    this.bankAccountCreate.loadUser(this.user);
    this.refresh();
  }

  createBankAccount() {
    this.displayDialogBankAccount = false;
    this.ngOnInit();
    this.lazyLoad({first: 0, rows: this.rows, sortField: this.sortField, sortOrder: this.sortOrder})
    this.messageService.add({
      severity: 'success',
      summary: 'Cuenta creada',
      detail: 'La cuenta se ha creado correctamente',
      life: 2000,
      closable: false
    });
    this.refresh();
  }

  createTransaction() {
    this.displayDialogTransaction = false;
    this.transactionCreate.loadUser();
    this.indexService.getBankAccountsByUserId().subscribe(bankAccounts => {
      this.bankAccounts = bankAccounts;
    });
    this.ngOnInit();
    this.lazyLoad({first: 0, rows: this.rows, sortField: this.sortField, sortOrder: this.sortOrder})
    this.messageService.add({
      severity: 'success',
      summary: 'Transacción creada',
      detail: 'La transacción se ha creado correctamente',
      life: 2000,
      closable: false
    });
  }

  closeDialog() {
    this.displayDialogBankAccount = false;
    this.displayDialogTransaction = false;
  }
}
