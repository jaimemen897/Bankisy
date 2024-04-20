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
import {AccountType} from "../models/AccountType";
import {SocketService} from "../services/socket.service";
import {BizumCreateComponent} from "../transactions/bizum-create/bizum-create.component";
import {ProgressSpinnerModule} from "primeng/progressspinner";
import {InplaceModule} from "primeng/inplace";
import {InputTextModule} from "primeng/inputtext";

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
    ScrollPanelModule,
    BizumCreateComponent,
    ProgressSpinnerModule,
    InplaceModule,
    InputTextModule
  ],
  templateUrl: './index.component.html',
  styleUrl: './index.component.css'
})
export class IndexComponent implements OnInit {

  constructor(private indexService: IndexService, private messageService: MessageService, private socketService: SocketService) {
  }

  @ViewChild(CreateTransactionComponent) transactionCreate!: CreateTransactionComponent
  @ViewChild(BankaccountCreateComponent) bankAccountCreate!: BankaccountCreateComponent
  @ViewChild(BizumCreateComponent) bizumCreateComponent!: BizumCreateComponent
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
  displayDialogBankAccountNewUser: boolean = false;
  displayCreateBankAccount: boolean = false;
  displayDialogTransaction: boolean = false;
  displayDialogBizum: boolean = false;
  updating: boolean = false;
  items: MenuItem[];
  transactionsDate: MenuItem[];

  //LOAD
  ngOnInit(): void {
    /*this.socketService.listenForTransactions().subscribe((transaction: any) => {
      this.refresh();
      this.messageService.add({
        severity: 'info',
        summary: 'Nueva transacción',
        detail: 'Se ha realizado una nueva transacción',
        life: 2000,
        closable: false
      });
    });*/
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
    this.transactionsDate = [
      {
        label: 'Hoy',
        icon: 'pi pi-calendar',
        command: () => {
          this.filter = new Date().toISOString().split('T')[0];
          this.refresh();
        }
      },
      {
        label: 'Última semana',
        icon: 'pi pi-calendar-plus',
        command: () => {
          let date = new Date();
          date.setDate(date.getDate() - 7);
          this.filter = date.toISOString().split('T')[0];
          this.refresh();
        }
      },
      {
        label: 'Último mes',
        icon: 'pi pi-calendar-minus',
        command: () => {
          let date = new Date();
          date.setMonth(date.getMonth() - 1);
          this.filter = date.toISOString().split('T')[0];
          this.refresh();
        }
      },
      {
        label: 'Último año',
        icon: 'pi pi-calendar-minus',
        command: () => {
          let date = new Date();
          date.setFullYear(date.getFullYear() - 1);
          this.filter = date.toISOString().split('T')[0];
          this.refresh();
        }
      },
      {
        label: 'Todos',
        icon: 'pi pi-calendar-minus',
        command: () => {
          this.filter = '';
          this.refresh();
        }
      }
    ];
  }

  lazyLoad(event: any) {
    let pageNumber = Math.floor(event.first / event.rows) + 1;
    let sortField = event.sortField;
    let sortOrder = event.sortOrder;

    this.indexService.getTransactionsByUserId(pageNumber, event.rows, sortField, sortOrder === -1, this.search, this.filter).subscribe(data => {
      this.transactions = data.items;
      this.totalRecords = data.totalRecords;
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
      if (this.bankAccounts.length === 0) {
        this.displayDialogBankAccountNewUser = true;
      }
    });
  }

  getAccountName(accountType: string): string {
    return AccountType[accountType as keyof typeof AccountType];
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
    if (this.bankAccounts.find(bankAccount => bankAccount.iban === transaction.ibanAccountOrigin) !== undefined) {
      return 'warning';
    } else if (this.bankAccounts.find(bankAccount => bankAccount.iban === transaction.ibanAccountDestination) !== undefined) {
      return 'success';
    } else {
      return 'info';
    }
  }

  getTransactionsByIban(iban: string) {
    this.transactionsByBankAccount = [];
    this.indexService.getTransactionsByIban(iban).subscribe(data => {
      this.transactionsByBankAccount = data;
      if (this.transactionsByBankAccount.length !== 0) {
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
    this.indexService.getBankAccountsByUserId().subscribe(bankAccounts => {
      this.bankAccounts = bankAccounts;
    });
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
    this.refresh();
    this.bankAccountCreate.loadUser(this.user);
    this.displayDialogBankAccount = true;
  }

  goToCreateBankAccountNewUser() {
    this.refresh();
    this.bankAccountCreate.loadUser(this.user);
    this.displayDialogBankAccountNewUser = true;
    this.displayCreateBankAccount = true;
  }

  goToCreateTransaction() {
    this.refresh();
    console.log(this.transactionCreate)
    this.transactionCreate.loadUser();
    this.displayDialogTransaction = true;
  }

  goToCreateBizum() {
    this.refresh();
    console.log(this.bizumCreateComponent)
    this.bizumCreateComponent.loadUser();
    this.displayDialogBizum = true;
  }

  createBankAccount() {
    this.indexService.getBankAccountsByUserId().subscribe(bankAccounts => {
      this.bankAccounts = bankAccounts;
    });
    this.displayDialogBankAccount = false;
    this.displayDialogBankAccountNewUser = false;
    this.refresh();
    this.messageService.add({
      severity: 'success',
      summary: 'Cuenta creada',
      detail: 'La cuenta se ha creado correctamente',
      life: 2000,
      closable: false
    });
  }

  createTransaction() {
    this.refresh();
    this.displayDialogTransaction = false;
    this.transactionCreate.loadUser();
    this.messageService.add({
      severity: 'success',
      summary: 'Transacción creada',
      detail: 'La transacción se ha creado correctamente',
      life: 2000,
      closable: false
    });
  }

  createBizum() {
    this.refresh()
    this.displayDialogBizum = false;
    this.bizumCreateComponent.loadUser()
    this.messageService.add({
      severity: 'success',
      summary: 'Bizum realizado',
      detail: 'El Bizum se ha realizado correctamente',
      life: 2000,
      closable: false
    });
  }

  activeBizum(iban: string){
    this.indexService.activeBizum(iban).subscribe(data => {
      this.messageService.add({
        severity: 'success',
        summary: 'Bizum activado',
        detail: 'Los bizums se han activado en esta cuenta',
        life: 2000,
        closable: false
      });
    });

  }

  closeDialog() {
    this.displayDialogBankAccount = false;
    this.displayDialogTransaction = false;
    this.displayDialogBizum = false;
    this.displayDialogBankAccountNewUser = false;
    this.displayCreateBankAccount = false;
  }
}
