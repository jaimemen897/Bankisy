import {Component, EventEmitter, OnInit, Output} from '@angular/core';
import {MessageService} from "primeng/api";
import {Router} from "@angular/router";
import {BankAccountService} from "../../services/bankaccounts.service";
import {BankAccountCreate} from "../../models/BankAccountCreate";
import {MultiSelectModule} from "primeng/multiselect";
import {UserService} from "../../services/users.service";
import {DropdownModule} from "primeng/dropdown";
import {FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators} from "@angular/forms";
import {ButtonModule} from "primeng/button";
import {User} from "../../models/User";

@Component({
  selector: 'app-bankaccount-create',
  standalone: true,
  imports: [
    MultiSelectModule,
    FormsModule,
    DropdownModule,
    ButtonModule,
    ReactiveFormsModule
  ],
  templateUrl: './bankaccount-create.component.html',
  styleUrl: './bankaccount-create.component.css'
})
export class BankaccountCreateComponent implements OnInit {

  constructor(private bankAccountService: BankAccountService, private usersService: UserService, private router: Router, private messageService: MessageService) {
  }

  formGroup: FormGroup = new FormGroup({
    selectedUsers: new FormControl<User[] | undefined>(undefined, [Validators.required]),
    selectedAccountType: new FormControl<string | undefined>(undefined, [Validators.required])
  });

  isUpdateMode: boolean = false;
  iban!: string;

  users!: User[]
  accountsTypes: String[] = ['Saving', 'Current', 'FixedTerm', 'Payroll', 'Student'];

  label: string = 'Crear';

  @Output() onSave: EventEmitter<any> = new EventEmitter();
  @Output() onCancel: EventEmitter<any> = new EventEmitter();

  ngOnInit() {
    this.usersService.getAllUsers().subscribe(users => {
      this.users = users;
    });
    this.formGroup.reset()
  }

  loadBankAccount(iban: string) {
    this.bankAccountService.getBankAccountById(iban).subscribe(bankAccount => {
      this.isUpdateMode = true;
      this.label = 'Actualizar';
      this.formGroup.controls.selectedAccountType.setValue(bankAccount.accountType);
      this.formGroup.controls.selectedUsers.setValue(this.users.filter(user => bankAccount.usersId.includes(user.id)));
      this.iban = iban;
    });
  }

  createBankAccount() {
    if (!this.formGroup.valid) {
      this.messageService.add({severity: 'error', summary: 'Error', detail: 'Por favor, rellene todos los campos'});
      return;
    }
    if (this.isUpdateMode) {

      let accountType = this.formGroup.value.selectedAccountType;
      let usersId = this.formGroup.value.selectedUsers.map((user: { id: any; }) => user.id);

      let bankAccount = new BankAccountCreate();
      bankAccount.accountType = accountType;
      bankAccount.usersId = usersId;

      this.bankAccountService.updateBankAccount(bankAccount, this.iban).subscribe(() => {
        this.messageService.add({
          severity: 'success',
          summary: 'Cuenta actualizada',
          detail: 'Cuenta bancaria actualizada'
        });
        this.router.navigate(['/bankaccounts']);
        this.onSave.emit();
      });

    } else {
      let bankAccount = new BankAccountCreate();
      bankAccount.accountType = this.formGroup.value.selectedAccountType;
      bankAccount.usersId = this.formGroup.value.selectedUsers.map((user: { id: any; }) => user.id);

      this.bankAccountService.addBankAccount(bankAccount).subscribe(() => {
        this.messageService.add({severity: 'success', summary: 'Cuenta creada', detail: 'Cuenta bancaria creada'});
        this.formGroup.reset();
        this.router.navigate(['/bankaccounts']);
        this.onSave.emit();
      });
    }
  }

  cancel() {
    this.formGroup.reset();
    this.onCancel.emit();
  }
}
