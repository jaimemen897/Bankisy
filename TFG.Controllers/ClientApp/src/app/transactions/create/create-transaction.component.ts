import {Component, EventEmitter, OnInit, Output} from '@angular/core';
import {ButtonModule} from "primeng/button";
import {DropdownModule} from "primeng/dropdown";
import {MultiSelectModule} from "primeng/multiselect";
import {PaginatorModule} from "primeng/paginator";
import {FormControl, FormGroup, ReactiveFormsModule, Validators} from "@angular/forms";
import {UserService} from "../../services/users.service";
import {MessageService} from "primeng/api";
import {User} from "../../models/User";
import {TransactionsService} from "../../services/transactions.service";
import {IndexService} from "../../services/index.service";
import {BankAccount} from "../../models/BankAccount";
import {InputTextModule} from "primeng/inputtext";
import {StyleClassModule} from "primeng/styleclass";
import {TransactionCreate} from "../../models/TransactionCreate";
import {NgIf} from "@angular/common";

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
    NgIf
  ],
  templateUrl: './create-transaction.component.html',
  styleUrl: './create-transaction.component.css'
})
export class CreateTransactionComponent implements OnInit {
  constructor(private messageService: MessageService, private indexService: IndexService) {
  }

  formGroup: FormGroup = new FormGroup({
    concept: new FormControl('', [Validators.required, Validators.minLength(3), Validators.maxLength(35)]),
    ibanAccountOrigin: new FormControl('', [Validators.required]),
    ibanAccountDestination: new FormControl('', [Validators.required, Validators.pattern('ES[0-9]{22}')]),
    amount: new FormControl('', [Validators.required, Validators.min(0.01), Validators.max(200000), Validators.pattern('^[0-9]+(\.[0-9]{1,2})?$')])
  });

  user!: User;
  bankAccounts!: BankAccount[];

  @Output() onSave: EventEmitter<any> = new EventEmitter();
  @Output() onCancel: EventEmitter<any> = new EventEmitter();

  ngOnInit() {
    this.indexService.getUserByToken().subscribe(user => {
      this.user = user;
      this.indexService.getBankAccountsByUserId().subscribe(bankAccounts => {
        this.bankAccounts = bankAccounts;
      });
    });
  }

  loadUser() {
    this.indexService.getUserByToken().subscribe(user => {
      this.user = user;
      this.indexService.getBankAccountsByUserId().subscribe(bankAccounts => {
        this.bankAccounts = bankAccounts;
      });
    });
  }

  createTransaction() {
    if (!this.formGroup.valid) {
      this.messageService.add({severity: 'error', summary: 'Error', detail: 'Por favor, rellene todos los campos'});
      return;
    }

    let transaction = new TransactionCreate();
    transaction.Concept = this.formGroup.controls.concept.value;
    transaction.IbanAccountOrigin = this.formGroup.controls.ibanAccountOrigin.value.iban;
    transaction.IbanAccountDestination = this.formGroup.controls.ibanAccountDestination.value;
    transaction.Amount = this.formGroup.controls.amount.value;

    this.indexService.addTransaction(transaction).subscribe(() => {
      this.formGroup.reset();
      this.onSave.emit();
    });
  }

  cancel() {
    this.formGroup.reset();
    this.onCancel.emit();
  }
}
