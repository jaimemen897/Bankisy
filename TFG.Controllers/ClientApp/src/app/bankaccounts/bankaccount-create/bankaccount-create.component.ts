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
import {IndexService} from "../../services/index.service";

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

  constructor(private bankAccountService: BankAccountService, private usersService: UserService, private router: Router,
              private messageService: MessageService, private indexService: IndexService) {
  }

  formGroup: FormGroup = new FormGroup({
    selectedUsers: new FormControl<User[] | undefined>([], [Validators.required]),
    selectedAccountType: new FormControl('', [Validators.required])
  });

  isNewUser: boolean = false;
  isUpdateMode: boolean = false;
  iban!: string;

  users!: User[]
  accountsTypes: string[] = ['Saving', 'Current', 'FixedTerm', 'Payroll', 'Student'];

  label: string = 'Crear';

  @Output() onSave: EventEmitter<any> = new EventEmitter();
  @Output() onCancel: EventEmitter<any> = new EventEmitter();

  ngOnInit() {
    if (this.isNewUser) {
      this.usersService.getAllUsers().subscribe(users => {
        this.users = users;
      });
    }

    this.updateSelectedUsersValidators();
    this.formGroup.reset()
  }

  updateSelectedUsersValidators() {
    const selectedUsersControl = this.formGroup.get('selectedUsers') as FormControl;
    if (this.isNewUser) {
      selectedUsersControl.setValidators(null);
    } else {
      selectedUsersControl.setValidators([Validators.required]);
    }
    selectedUsersControl.updateValueAndValidity();
  }

  //this is called when a user is creating an account for himself
  loadUser(userId: User) {
    this.isNewUser = true;
    this.updateSelectedUsersValidators();
    this.formGroup.controls.selectedUsers.setValue([userId]);
  }

  //this is called when updating a bank account from the admin
  loadBankAccount(iban: string) {
    if (!this.isNewUser) {
      this.bankAccountService.getBankAccountById(iban).subscribe(bankAccount => {
        this.isNewUser = false;
        this.updateSelectedUsersValidators();
        this.isUpdateMode = true;
        this.label = 'Actualizar';
        this.formGroup.controls.selectedAccountType.setValue(bankAccount.accountType);
        this.formGroup.controls.selectedUsers.setValue(this.users.filter(user => bankAccount.usersId.includes(user.id)));
        this.iban = iban;
      });
    }
  }

  saveChanges() {
    if (!this.formGroup.valid) {
      this.messageService.add({severity: 'error', summary: 'Error', detail: 'Por favor, rellene todos los campos'});
      return;
    }

    /*UPDATE*/
    if (this.isUpdateMode) {
      let accountType = this.formGroup.value.selectedAccountType;
      let usersId = this.formGroup.value.selectedUsers.map((user: { id: any; }) => user.id);

      let bankAccount = new BankAccountCreate();
      bankAccount.accountType = accountType;
      bankAccount.usersId = usersId;

      this.bankAccountService.updateBankAccount(bankAccount, this.iban).subscribe(() => {
        this.messageService.add({
          severity: 'success', summary: 'Cuenta actualizada', detail: 'Cuenta bancaria actualizada'
        });
        this.router.navigate(['/bankaccounts']);
        this.onSave.emit();
      });

      /*USER CREATE*/
    } else if (this.isNewUser) {
      let bankAccount = new BankAccountCreate();
      bankAccount.accountType = this.formGroup.value.selectedAccountType;
      bankAccount.usersId = this.formGroup.controls.selectedUsers.value.map((user: { id: any; }) => user.id);

      this.indexService.addBankAccount(bankAccount).subscribe(() => {
        this.messageService.add({severity: 'success', summary: 'Cuenta creada', detail: 'Cuenta bancaria creada'});
        this.formGroup.reset();
        this.onSave.emit();
      });

      /*ADMIN CREATE*/
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
