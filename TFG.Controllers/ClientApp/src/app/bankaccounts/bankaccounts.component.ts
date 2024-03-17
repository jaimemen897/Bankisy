import {Component} from '@angular/core';
import {ConfirmationService, MessageService, SelectItem} from "primeng/api";
import {Router, RouterOutlet} from "@angular/router";
import {BankAccountService} from "./bankaccounts.service";
import {ToastModule} from "primeng/toast";
import {TableModule} from "primeng/table";
import {MultiSelectModule} from "primeng/multiselect";
import {DropdownModule} from "primeng/dropdown";
import {TagModule} from "primeng/tag";
import {NgClass} from "@angular/common";
import {InputTextModule} from "primeng/inputtext";
import {TooltipModule} from "primeng/tooltip";
import {ButtonModule} from "primeng/button";
import {FormsModule} from "@angular/forms";
import {OverlayPanelModule} from "primeng/overlaypanel";
import {ConfirmDialogModule} from "primeng/confirmdialog";

export class BankAccount {
  id: string;
  iban: string;
  balance: number;
  accountType: string;
  usersId: string[];
  usersName: string[];
  isDeleted: boolean;
}

export class BankAccountCreate {
  balance: number;
  accountType: string;
  usersId: string[];
}

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
    ConfirmDialogModule
  ],
  templateUrl: './bankaccounts.component.html',
  styleUrl: './bankaccounts.component.css'
})
export class BankaccountsComponent {
  constructor(private bankAccountService: BankAccountService, private router: Router, private confirmationService: ConfirmationService, private messageService: MessageService) {
  }

  bankAccounts: BankAccount[] = [];
  rows: number = 10;
  totalRecords: number = 0;

  sortOptions!: SelectItem[];
  sortField!: string;
  sortOrder!: number;
  search: string;
  filter!: string;

  accountsTypes: String[] = ['Saving', 'Current', 'FixedTerm', 'Payroll', 'Student'];
  users: String[] = [];
  status: String[] = ['Active', 'Inactive'];

  lazyLoad(event: any) {
    let pageNumber = Math.floor(event.first / event.rows) + 1;
    this.bankAccountService.getBankAccounts(pageNumber, event.rows, this.sortField, this.sortOrder === -1, this.search, this.filter).subscribe(data => {
      this.bankAccounts = data.items;
      this.totalRecords = data.totalRecords;
      for (let bankAccount of this.bankAccounts) {
        this.users.push(bankAccount.usersName.join(', '))
      }
    });

    this.sortOptions = [
      {label: 'Mayor saldo', value: '!balance'},
      {label: 'Menor saldo', value: 'balance'}
    ];
  }

  onSearch(event: any) {
    this.bankAccountService.getBankAccounts(1, this.rows, this.sortField, this.sortOrder === -1, event.target.value, this.filter).subscribe(data => {
      this.bankAccounts = data.items;
      this.totalRecords = data.totalCount;
      this.search = event.target.value;
    });
  }

  onSearchUser(event: any) {
    console.log(event);
    this.bankAccountService.getBankAccounts(1, this.rows, this.sortField, this.sortOrder === -1, event.value).subscribe(data => {
      this.bankAccounts = data.items;
      this.totalRecords = data.totalCount;
      this.filter = event.value;
    });
  }

  onSearchFilter(event: any) {
    this.bankAccountService.getBankAccounts(1, this.rows, this.sortField, this.sortOrder === -1, this.search, event.value).subscribe(data => {
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

  goToAddBankAccount() {
    this.router.navigate(['/bankaccounts/add']);
  }

  goToEditBankAccount(id: string) {
    this.router.navigate(['/bankaccounts/edit', id]);
  }

  goToTransactions(id: string) {
    this.router.navigate(['/transactions', id]);
  }

  deleteBankAccount(id: string) {
    console.log(id);
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
        this.bankAccountService.deleteBankAccount(id).subscribe(() => {
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

  getSeverity(accountType: string) {
    if (accountType === 'Saving') {
      return 'success';
    } else if (accountType === 'Current') {
      return 'info';
    } else if (accountType === 'FixedTerm') {
      return 'warning';
    } else if (accountType === 'Payroll') {
      return 'danger';
    } else if (accountType === 'Student') {
      return 'primary';
    } else {
      return 'secondary';
    }
  }

  getBalanceColor(balance: number) {
    if (balance > 0) {
      return 'success';
    } else if (balance < 0) {
      return 'danger';
    } else {
      return 'info';
    }
  }

  clearFilters() {
    this.search = '';
    this.filter = '';

    this.bankAccountService.getBankAccounts(1, this.rows, this.sortField, this.sortOrder === -1, this.search, this.filter).subscribe(data => {
      this.bankAccounts = data.items;
      this.totalRecords = data.totalCount;
    });
  }
}
