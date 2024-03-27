import {Component, ViewChild} from '@angular/core';
import {ConfirmationService, MessageService, SelectItem} from "primeng/api";
import {Router, RouterOutlet} from "@angular/router";
import {ToastModule} from "primeng/toast";
import {TableModule} from "primeng/table";
import {MultiSelectModule} from "primeng/multiselect";
import {DropdownModule} from "primeng/dropdown";
import {TagModule} from "primeng/tag";
import {DatePipe, NgClass} from "@angular/common";
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
    DatePipe
  ],
  templateUrl: './bankaccounts.component.html',
  styleUrl: './bankaccounts.component.css'
})
export class BankaccountsComponent {
  constructor(private bankAccountService: BankAccountService, private router: Router, private confirmationService: ConfirmationService, private messageService: MessageService) {
  }

  @ViewChild(BankaccountCreateComponent) bankAccountCreateComponent!: BankaccountCreateComponent;
  bankAccounts: BankAccount[] = [];
  rows: number = 10;
  totalRecords: number = 0;
  displayDialog: boolean = false;

  sortField!: string;
  sortOrder!: number;
  search: string;
  filter!: string;

  accountsTypes: string[] = [AccountType.Saving, AccountType.Current, AccountType.FixedTerm, AccountType.Payroll, AccountType.Student];
  users: String[] = [];
  status: String[] = ['Active', 'Inactive'];
  headerSaveUpdateBankAccount: string = 'Crear cuenta de banco';

  /*DATA, ORDERS AND FILTERS*/
  lazyLoad(event: any) {
    let pageNumber = Math.floor(event.first / event.rows) + 1;
    let sortField = event.sortField;
    let sortOrder = event.sortOrder;


    this.bankAccountService.getBankAccounts(pageNumber, event.rows, sortField, sortOrder === -1, this.search, this.filter).subscribe(data => {
      this.bankAccounts = data.items;
      this.totalRecords = data.totalRecords;
      for (let bankAccount of this.bankAccounts) {
        this.users.push(bankAccount.usersName.join(', '))
      }
    });
  }

  //IBAN, SALDO, ESTADO
  onSearch(event: any) {
    this.bankAccountService.getBankAccounts(1, this.rows, this.sortField, this.sortOrder === -1, event.target.value, this.filter).subscribe(data => {
      this.bankAccounts = data.items;
      this.totalRecords = data.totalCount;
      this.search = event.target.value;
    });
  }

  //USUARIOS
  onSearchUser(event: any) {
    this.bankAccountService.getBankAccounts(1, this.rows, this.sortField, this.sortOrder === -1, event.value).subscribe(data => {
      this.bankAccounts = data.items;
      this.totalRecords = data.totalCount;
      this.filter = event.value;
    });
  }

  //TIPO CUENTA
  onSearchFilter(event: any) {
    let accountTypeTranslated = Object.keys(AccountType).find(key => AccountType[key as keyof typeof AccountType] === event.value) as keyof typeof AccountType;
    this.bankAccountService.getBankAccounts(1, this.rows, this.sortField, this.sortOrder === -1, this.search, accountTypeTranslated).subscribe(data => {
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
    this.bankAccountService.getBankAccounts(1, this.rows, this.sortField, this.sortOrder === -1, this.search, this.filter).subscribe(data => {
      this.bankAccounts = data.items;
      this.totalRecords = data.totalCount;
    });

    this.sortField = '';
    this.sortOrder = 1;
  }

  clearFilters() {
    this.search = '';
    this.filter = '';

    this.bankAccountService.getBankAccounts(1, this.rows, this.sortField, this.sortOrder === -1, this.search, this.filter).subscribe(data => {
      this.bankAccounts = data.items;
      this.totalRecords = data.totalCount;
    });
  }

  /*REDIRECTIONS*/
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

  goToTransactions(iban: string) {
    this.router.navigate(['/transactions', iban]);
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
          life: 3000,
          closable: false
        });
        this.bankAccountService.deleteBankAccount(iban).subscribe(() => {
          this.bankAccountService.getBankAccounts(1, this.rows).subscribe((data) => {
            this.bankAccounts = data.items;
            this.totalRecords = data.totalCount;
          });
        });
      },
      reject: () => {
        this.messageService.add({
          severity: 'error',
          summary: 'Cancelar',
          detail: 'No se ha eliminado',
          life: 3000,
          closable: false
        });
      }
    });
  }

  goToLogin() {
    this.router.navigate(['/login']);
  }

  goToUsers() {
    this.router.navigate(['/users']);
  }

  /*COLORS*/
  getSeverity(accountType: string) {
    if (accountType === 'Saving' || accountType === 'Ahorros') {
      return 'success';
    } else if (accountType === 'Current' || accountType === 'Corriente') {
      return 'info';
    } else if (accountType === 'FixedTerm' || accountType === 'Plazo fijo') {
      return 'warning';
    } else if (accountType === 'Payroll' || accountType === 'Nómina') {
      return 'danger';
    } else if (accountType === 'Student' || accountType === 'Estudiante') {
      return 'primary';
    } else {
      return 'secondary';
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

  saveBankAccount() {
    this.displayDialog = false;
    this.lazyLoad({first: 1, rows: this.rows, sortField: this.sortField, sortOrder: this.sortOrder})
  }

  closeDialog() {
    this.displayDialog = false;
  }
}
