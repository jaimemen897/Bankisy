import {Component, EventEmitter, OnInit, Output} from '@angular/core';
import {MessageService} from "primeng/api";
import {BankAccountService} from "../../services/bankaccounts.service";
import {BankAccountCreate} from "../../models/BankAccountCreate";
import {MultiSelectModule} from "primeng/multiselect";
import {UserService} from "../../services/users.service";
import {DropdownModule} from "primeng/dropdown";
import {FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators} from "@angular/forms";
import {ButtonModule} from "primeng/button";
import {User} from "../../models/User";

import {AccountType} from "../../models/AccountType";
import {ActivatedRoute} from "@angular/router";

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

  constructor(private bankAccountService: BankAccountService, private usersService: UserService, private messageService: MessageService, private route: ActivatedRoute) {
  }

  formGroup: FormGroup = new FormGroup({
    selectedUsers: new FormControl<User[] | undefined>([], [Validators.required]),
    selectedAccountType: new FormControl('', [Validators.required])
  });

  isNewUser: boolean = false;
  isUpdateMode: boolean = false;
  iban!: string;

  users!: User[]
  accountsTypes: string[] = [AccountType.Saving, AccountType.Current, AccountType.FixedTerm, AccountType.Payroll, AccountType.Student];

  label: string = 'Crear';

  @Output() onSave: EventEmitter<any> = new EventEmitter();
  @Output() onCancel: EventEmitter<any> = new EventEmitter();

  ngOnInit(): void {
    this.route.data.subscribe(data => {
      this.isNewUser = data.newUser;
      this.updateSelectedUsersValidators();

      if (this.isNewUser) {
        this.loadUser(this.route.snapshot.params.userId);
      }
    });
  }

  //CHANGE SELECTED USERS VALIDATORS DEPENDING ON THE MODE
  updateSelectedUsersValidators() {
    const selectedUsersControl = this.formGroup.get('selectedUsers') as FormControl;
    if (this.isNewUser) {
      selectedUsersControl.setValidators(null);
    } else {
      selectedUsersControl.setValidators([Validators.required]);
    }
    selectedUsersControl.updateValueAndValidity();
  }

  //USERS CREATE AN ACCOUNT
  loadUser(userId: User) {
    this.isNewUser = true;
    this.updateSelectedUsersValidators();
    this.formGroup.controls.selectedUsers.setValue([userId]);
  }

  //ADMIN CREATE AN ACCOUNT FOR A USER
  loadUsers() {
    this.isNewUser = false;
    this.usersService.getAllUsers().subscribe(users => {
      this.users = users;
    });
    this.updateSelectedUsersValidators();
  }

  //ADMIN UPDATE AN ACCOUNT FOR A USER
  loadBankAccount(iban: string) {
    if (!this.isNewUser) {
      this.loadUsers();
      this.bankAccountService.getBankAccountById(iban).subscribe(bankAccount => {
        this.isNewUser = false;
        this.isUpdateMode = true;
        this.updateSelectedUsersValidators();
        this.label = 'Actualizar';
        let accountTypeTranslated = AccountType[bankAccount.accountType as keyof typeof AccountType];
        this.formGroup.controls.selectedAccountType.setValue(accountTypeTranslated);
        this.formGroup.controls.selectedUsers.setValue(this.users.filter(user => bankAccount.usersId.includes(user.id)));
        this.iban = iban;
      });
    }
  }

  saveChanges() {
    if (!this.formGroup.valid) {
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        closable: false,
        detail: 'Por favor, rellene todos los campos',
        life: 2000
      });
      return;
    }
    //TRANSLATE ACCOUNT TYPE
    let accountTypeTranslated = Object.keys(AccountType).find(key => AccountType[key as keyof typeof AccountType] === this.formGroup.value.selectedAccountType) as keyof typeof AccountType;

    /*UPDATE*/
    if (this.isUpdateMode) {
      let usersId = this.formGroup.value.selectedUsers.map((user: { id: any; }) => user.id);

      let bankAccount = new BankAccountCreate();
      bankAccount.accountType = accountTypeTranslated;
      bankAccount.usersId = usersId;

      this.bankAccountService.updateBankAccount(bankAccount, this.iban).subscribe(() => {
        this.messageService.add({
          severity: 'success', summary: 'Cuenta actualizada', closable: false, detail: 'Cuenta bancaria actualizada'
        });
        this.onSave.emit();
      });

      /*USER CREATE*/
    } else if (this.isNewUser) {
      let bankAccount = new BankAccountCreate();
      bankAccount.accountType = accountTypeTranslated;
      bankAccount.usersId = this.formGroup.controls.selectedUsers.value.map((user: { id: any; }) => user.id);

      this.bankAccountService.addBankAccountMySelf(bankAccount).subscribe(() => {
        this.formGroup.reset();
        this.onSave.emit();
      });

      /*ADMIN CREATE*/
    } else {
      let bankAccount = new BankAccountCreate();
      bankAccount.accountType = accountTypeTranslated;
      bankAccount.usersId = this.formGroup.value.selectedUsers.map((user: { id: any; }) => user.id);

      this.bankAccountService.addBankAccount(bankAccount).subscribe(() => {
        this.messageService.add({
          severity: 'success',
          summary: 'Cuenta creada',
          closable: false,
          detail: 'Cuenta bancaria creada'
        });
        this.formGroup.reset();
        this.onSave.emit();
      });
    }
  }

  cancel() {
    this.formGroup.reset();
    this.onCancel.emit();
  }


}
