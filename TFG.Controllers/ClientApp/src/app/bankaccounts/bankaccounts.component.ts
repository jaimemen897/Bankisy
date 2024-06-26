import {Component, ViewChild} from '@angular/core';
import {ConfirmationService, MessageService} from "primeng/api";
import {RouterOutlet} from "@angular/router";
import {ToastModule} from "primeng/toast";
import {TableModule} from "primeng/table";
import {MultiSelectModule} from "primeng/multiselect";
import {DropdownModule} from "primeng/dropdown";
import {TagModule} from "primeng/tag";
import {DatePipe, NgClass, NgStyle} from "@angular/common";
import {InputTextModule} from "primeng/inputtext";
import {TooltipModule} from "primeng/tooltip";
import {ButtonModule} from "primeng/button";
import {FormsModule} from "@angular/forms";
import {OverlayPanelModule} from "primeng/overlaypanel";
import {ConfirmDialogModule} from "primeng/confirmdialog";
import {BankAccount} from "../models/BankAccount";
import {BankAccountService} from "../services/bankaccounts.service";
import {DialogModule} from "primeng/dialog";
import {BankaccountCreateComponent} from "./bankaccount-create/bankaccount-create.component";
import {AccountType} from "../models/AccountType";
import {Transaction} from "../models/Transaction";
import {StyleClassModule} from "primeng/styleclass";
import {CascadeSelectModule} from "primeng/cascadeselect";
import {IbanFormatPipe} from "../pipes/IbanFormatPipe";
import {TransactionsService} from "../services/transactions.service";

@Component({
  selector: 'app-bankaccounts',
  standalone: true,
  imports: [
    RouterOutlet,
    ToastModule,
    TableModule,
    MultiSelectModule,
    DropdownModule,
    TagModule,
    NgClass,
    InputTextModule,
    TooltipModule,
    ButtonModule,
    FormsModule,
    OverlayPanelModule,
    ConfirmDialogModule,
    DialogModule,
    BankaccountCreateComponent,
    DatePipe,
    NgStyle,
    StyleClassModule,
    CascadeSelectModule,
    IbanFormatPipe
  ],
  templateUrl: './bankaccounts.component.html',
  styleUrl: './bankaccounts.component.css'
})
export class BankaccountsComponent {
  constructor(private transactionService: TransactionsService, private bankAccountService: BankAccountService, private confirmationService: ConfirmationService, private messageService: MessageService) {
  }

  @ViewChild(BankaccountCreateComponent) bankAccountCreateComponent!: BankaccountCreateComponent;
  @ViewChild('transactionPanel') transactionPanel!: any;
  bankAccounts: BankAccount[] = [];
  rows: number = 10;
  totalRecords: number = 0;
  displayDialog: boolean = false;

  pageNumber!: number;
  sortField!: string;
  sortOrder!: number;
  search: string;
  filter!: string;
  isDeleted!: boolean;

  accountsTypes: string[] = [AccountType.Saving, AccountType.Current, AccountType.FixedTerm, AccountType.Payroll, AccountType.Student];
  users: string[] = [];
  status: string[] = ['Active', 'Inactive'];
  transactions: Transaction[] = [];
  headerSaveUpdateBankAccount: string = 'Crear cuenta de banco';

  //DATA, ORDERS AND FILTERS
  lazyLoad(event: any) {
    this.pageNumber = Math.floor(event.first / event.rows) + 1;
    this.sortField = event.sortField;
    this.sortOrder = event.sortOrder;
    this.rows = event.rows;

    if (event.filters?.isDeleted) {
      this.isDeleted = event.filters.isDeleted.value;
    }

    this.bankAccountService.getBankAccounts(this.pageNumber, this.rows, this.sortField, this.sortOrder === -1, this.search, this.filter, this.isDeleted).subscribe(data => {
      this.bankAccounts = data.items;
      this.totalRecords = data.totalRecords;
      for (let bankAccount of this.bankAccounts) {
        this.users.push(bankAccount.usersName.join(', '))
      }
      this.users = this.users.filter((value, index) => this.users.indexOf(value) === index);
    });
  }

  //IBAN, SALDO, ESTADO
  onSearch(event: any) {
    this.bankAccountService.getBankAccounts(this.pageNumber, this.rows, this.sortField, this.sortOrder === -1, event.target.value, this.filter).subscribe(data => {
      this.bankAccounts = data.items;
      this.totalRecords = data.totalCount;
      this.search = event.target.value;
    });
  }

  //USUARIOS
  onSearchUser(event: any) {
    this.bankAccountService.getBankAccounts(this.pageNumber, this.rows, this.sortField, this.sortOrder === -1, event.value).subscribe(data => {
      this.bankAccounts = data.items;
      this.totalRecords = data.totalCount;
      this.filter = event.value;
    });
  }

  //TIPO CUENTA
  onSearchFilter(event: any) {
    let accountTypeTranslated = Object.keys(AccountType).find(key => AccountType[key as keyof typeof AccountType] === event.value) as keyof typeof AccountType;
    this.bankAccountService.getBankAccounts(this.pageNumber, this.rows, this.sortField, this.sortOrder === -1, this.search, accountTypeTranslated).subscribe(data => {
      this.bankAccounts = data.items;
      this.totalRecords = data.totalCount;
      this.filter = event.value;
    });
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
    this.bankAccountService.getBankAccounts(this.pageNumber, this.rows, this.sortField, this.sortOrder === -1, this.search, this.filter).subscribe(data => {
      this.bankAccounts = data.items;
      this.totalRecords = data.totalCount;
    });

    this.sortField = '';
    this.sortOrder = 1;
  }

  clearFilters() {
    this.search = '';
    this.filter = '';

    this.bankAccountService.getBankAccounts(this.pageNumber, this.rows, this.sortField, this.sortOrder === -1, this.search, this.filter).subscribe(data => {
      this.bankAccounts = data.items;
      this.totalRecords = data.totalCount;
    });
  }

  //REDIRECTIONS
  goToCreateBankAccount() {
    this.headerSaveUpdateBankAccount = 'Crear cuenta de banco';
    this.displayDialog = true;
    this.bankAccountCreateComponent.loadUsers();
  }

  goToEditBankAccount(iban: string) {
    this.headerSaveUpdateBankAccount = 'Actualizar cuenta de banco';
    this.displayDialog = true;
    this.bankAccountCreateComponent.loadBankAccount(iban);
  }

  deleteBankAccount(iban: string) {
    this.confirmationService.confirm({
      header: '¿Desea eliminar la cuenta de banco?',
      message: 'Confirme para continuar',
      accept: () => {
        this.messageService.add({
          severity: 'info',
          summary: 'Eliminada',
          detail: 'Cuenta eliminada',
          life: 2000,
          closable: false
        });
        this.bankAccountService.deleteBankAccount(iban).subscribe(() => {
          this.bankAccountService.getBankAccounts(this.pageNumber, this.rows, this.sortField, this.sortOrder === -1, this.search, this.filter).subscribe(data => {
            this.bankAccounts = data.items;
            this.totalRecords = data.totalCount;
          });
        });
      }
    });
  }

  activateBankAccount(iban: string) {
    this.bankAccountService.activateBankAccount(iban).subscribe(() => {
      this.bankAccountService.getBankAccounts(this.pageNumber, this.rows, this.sortField, this.sortOrder === -1, this.search, this.filter).subscribe(data => {
        this.bankAccounts = data.items;
        this.totalRecords = data.totalCount;
      });
    });

    this.messageService.add({
      severity: 'info',
      summary: 'Activada',
      detail: 'Cuenta activada',
      life: 2000,
      closable: false
    });
  }

  getTransactionsByIban(iban: string, event: any) {
    this.transactionService.getTransactionsByIban(iban).subscribe(data => {
      this.transactions = data;
      if (this.transactions.length !== 0) {
        this.transactionPanel.toggle(event);
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

  //COLORS
  getSeverity(accountType: string) {
    if (accountType === 'Saving' || accountType === 'Ahorros') {
      return 'success';
    } else if (accountType === 'Current' || accountType === 'Corriente') {
      return 'secondary';
    } else if (accountType === 'FixedTerm' || accountType === 'Plazo fijo') {
      return 'info';
    } else if (accountType === 'Payroll' || accountType === 'Nómina') {
      return 'warning';
    } else if (accountType === 'Student' || accountType === 'Estudiante') {
      return 'danger';
    } else {
      return 'contrast';
    }
  }

  getBalanceColor(balance: number) {
    if (balance > 1000) {
      return 'success';
    } else if (balance == 0) {
      return 'warning';
    } else if (balance < 0) {
      return 'danger';
    } else {
      return 'info';
    }
  }

  getAccountName(accountType: string): string {
    return AccountType[accountType as keyof typeof AccountType];
  }

  saveBankAccount() {
    this.displayDialog = false;
    this.bankAccountService.getBankAccounts(this.pageNumber, this.rows, this.sortField, this.sortOrder === -1, this.search, this.filter).subscribe(data => {
      this.bankAccounts = data.items;
      this.totalRecords = data.totalCount;
    });
  }

  closeDialog() {
    this.displayDialog = false;
  }
}
