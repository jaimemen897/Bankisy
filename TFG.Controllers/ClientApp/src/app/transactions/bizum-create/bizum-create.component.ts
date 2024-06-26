import {Component, EventEmitter, Output} from '@angular/core';
import {MessageService} from "primeng/api";
import {FormControl, FormGroup, ReactiveFormsModule, Validators} from "@angular/forms";
import {User} from "../../models/User";
import {BizumCreate} from "../../models/BizumCreate";
import {ButtonModule} from "primeng/button";
import {DropdownModule} from "primeng/dropdown";
import {InputTextModule} from "primeng/inputtext";

import {PaginatorModule} from "primeng/paginator";
import {TransactionsService} from "../../services/transactions.service";
import {UserService} from "../../services/users.service";

@Component({
  selector: 'app-bizum-create',
  standalone: true,
  imports: [
    ButtonModule,
    DropdownModule,
    InputTextModule,
    PaginatorModule,
    ReactiveFormsModule
  ],
  templateUrl: './bizum-create.component.html',
  styleUrl: './bizum-create.component.css'
})
export class BizumCreateComponent {
  constructor(private messageService: MessageService, private transactionService: TransactionsService, private userService: UserService) {
    this.userService.user$.subscribe(user => {
      this.user = user;
    });
  }

  formGroup: FormGroup = new FormGroup({
    concept: new FormControl('', [Validators.required, Validators.minLength(3), Validators.maxLength(35)]),
    phoneUserDestination: new FormControl('', [Validators.required, Validators.pattern('^[0-9]{9}$')]),
    amount: new FormControl('', [Validators.required, Validators.min(0.50), Validators.max(500), Validators.pattern('^[0-9]+(.[0-9]{1,2})?$')])
  });

  user!: User;

  @Output() onSave: EventEmitter<any> = new EventEmitter();
  @Output() onCancel: EventEmitter<any> = new EventEmitter();
  @Output() onUserLoaded: EventEmitter<any> = new EventEmitter();

  loadUser() {
    this.userService.user$.subscribe(user => {
      this.user = user;
    });
    this.onUserLoaded.emit(this.user);
  }

  createBizum() {
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

    let bizumCreate = new BizumCreate();
    bizumCreate.concept = this.formGroup.controls.concept.value;
    bizumCreate.phoneNumberUserOrigin = this.user.phone;
    bizumCreate.phoneNumberUserDestination = this.formGroup.controls.phoneUserDestination.value;
    bizumCreate.amount = this.formGroup.controls.amount.value;

    this.transactionService.createBizum(bizumCreate).subscribe(() => {
      this.formGroup.reset();
      this.onSave.emit();
    });
  }

  cancel() {
    this.formGroup.reset();
    this.onCancel.emit();
  }
}
