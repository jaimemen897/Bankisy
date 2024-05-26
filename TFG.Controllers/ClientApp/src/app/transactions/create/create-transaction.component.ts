import {Component, EventEmitter, OnInit, Output} from '@angular/core';
import {ButtonModule} from "primeng/button";
import {DropdownModule} from "primeng/dropdown";
import {MultiSelectModule} from "primeng/multiselect";
import {PaginatorModule} from "primeng/paginator";
import {FormControl, FormGroup, ReactiveFormsModule, Validators} from "@angular/forms";
import {MessageService} from "primeng/api";
import {User} from "../../models/User";
import {BankAccount} from "../../models/BankAccount";
import {InputTextModule} from "primeng/inputtext";
import {StyleClassModule} from "primeng/styleclass";
import {TransactionCreate} from "../../models/TransactionCreate";
import {NgStyle} from "@angular/common";
import {BankAccountService} from "../../services/bankaccounts.service";
import {TransactionsService} from "../../services/transactions.service";
import {UserService} from "../../services/users.service";
import {IbanFormatPipe} from "../../pipes/IbanFormatPipe";

@Component({
  selector: 'app-create-transaction',
  standalone: true,
  imports: [
    ButtonModule,
    DropdownModule,
    MultiSelectModule,
    PaginatorModule,
    ReactiveFormsModule,
    InputTextModule,
    StyleClassModule,
    NgStyle,
    IbanFormatPipe
  ],
  templateUrl: './create-transaction.component.html',
  styleUrl: './create-transaction.component.css'
})
export class CreateTransactionComponent implements OnInit {
  constructor(private messageService: MessageService, private userService: UserService, private bankAccountService: BankAccountService, private transactionsService: TransactionsService) {
  }

  formGroup: FormGroup = new FormGroup({
    concept: new FormControl('', [Validators.required, Validators.minLength(3), Validators.maxLength(35)]),
    ibanAccountOrigin: new FormControl('', [Validators.required]),
    ibanAccountDestination: new FormControl('', [Validators.required, Validators.pattern('ES[0-9]{22}')]),
    amount: new FormControl('', [Validators.required, Validators.min(0.01), Validators.max(200000), Validators.pattern('^[0-9]+(.[0-9]{1,2})?$')])
  });

  user!: User;
  bankAccounts!: BankAccount[];

  @Output() onSave: EventEmitter<any> = new EventEmitter();
  @Output() onCancel: EventEmitter<any> = new EventEmitter();

  ngOnInit() {
    this.loadUser();
  }

  loadUser() {
    this.userService.user$.subscribe(user => {
      this.user = user;
    });
    this.bankAccountService.getBankAccountsByMySelf().subscribe(bankAccounts => {
      this.bankAccounts = bankAccounts;
    });
  }

  createTransaction() {
    if (!this.formGroup.valid) {
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Por favor, rellene todos los campos',
        life: 2000,
        closable: false
      });
      return;
    }

    let transaction = new TransactionCreate();
    transaction.Concept = this.formGroup.controls.concept.value;
    transaction.IbanAccountOrigin = this.formGroup.controls.ibanAccountOrigin.value.iban;
    transaction.IbanAccountDestination = this.formGroup.controls.ibanAccountDestination.value;
    transaction.Amount = this.formGroup.controls.amount.value;

    this.transactionsService.createTransaction(transaction).subscribe(() => {
      this.formGroup.reset();
      this.onSave.emit();
    });
  }

  cancel() {
    this.formGroup.reset();
    this.onCancel.emit();
  }
}
