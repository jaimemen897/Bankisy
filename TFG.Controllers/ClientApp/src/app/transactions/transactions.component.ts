import {Component, OnInit} from '@angular/core';
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
import {Transaction} from "../models/Transaction";
import {TransactionsService} from "../services/transactions.service";
import {SocketService} from "../services/socket.service";

@Component({
  selector: 'app-transactions',
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
    DatePipe
  ],
  templateUrl: './transactions.component.html',
  styleUrl: './transactions.component.css'
})
export class TransactionsComponent {
  constructor(private transactionService: TransactionsService, private router: Router, private confirmationService: ConfirmationService, private messageService: MessageService, private socketService: SocketService) {
  }

  transactions: Transaction[] = [];
  rows: number = 10;
  totalRecords: number = 0;

  pageNumber!: number;
  sortOptions!: SelectItem[];
  sortField!: string;
  sortOrder!: number;
  search: string;

  users: string[] = [];

  lazyLoad(event: any) {
    this.pageNumber = Math.floor(event.first / event.rows) + 1;
    this.sortField = event.sortField;
    this.sortOrder = event.sortOrder;
    this.rows = event.rows;

    this.transactionService.getTransactions(this.pageNumber, this.rows, this.sortField, this.sortOrder === -1, this.search).subscribe(data => {
      this.transactions = data.items;
      this.totalRecords = data.totalRecords;
    });

    this.sortOptions = [
      {label: 'Concepto', value: 'concept'},
      {label: 'Cantidad', value: 'amount'},
    ];
  }

  onSearch(event: any) {
    this.transactionService.getTransactions(1, this.rows, this.sortField, this.sortOrder === -1, event.target.value).subscribe(data => {
      this.transactions = data.items;
      this.totalRecords = data.totalCount;
      this.search = event.target.value;
    });
  }

  onSearchUser(event: any) {
    this.transactionService.getTransactions(1, this.rows, this.sortField, this.sortOrder === -1, event.value).subscribe(data => {
      this.transactions = data.items;
      this.totalRecords = data.totalCount;
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

  deleteTransaction(id: string) {
    this.confirmationService.confirm({
      header: '¿Desea eliminar la transacción?',
      message: 'Confirme para continuar',
      accept: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Eliminada',
          detail: 'La transacción ha sido eliminada',
          life: 2000,
          closable: false
        });
        this.transactionService.deleteTransaction(id).subscribe(() => {
          this.transactionService.getTransactions(this.pageNumber, this.rows, this.sortField, this.sortOrder === -1, this.search).subscribe(data => {
            this.transactions = data.items;
            this.totalRecords = data.totalRecords;
          });
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

  clearOrders() {
    this.transactionService.getTransactions(1, this.rows, this.sortField, this.sortOrder === -1, this.search).subscribe(data => {
      this.transactions = data.items;
      this.totalRecords = data.totalCount;
    });
  }

  clearFilters() {
    this.search = '';

    this.transactionService.getTransactions(this.pageNumber, this.rows, this.sortField, this.sortOrder === -1, this.search).subscribe(data => {
      this.transactions = data.items;
      this.totalRecords = data.totalRecords;
    });
  }

  getAmountColor(amount: number) {
    if (amount < 10) {
      return 'danger';
    } else if (amount < 200) {
      return 'warning';
    } else if (amount < 500) {
      return 'info';
    } else if (amount < 1000) {
      return 'success';
    } else {
      return 'contrast';
    }
  }
}
