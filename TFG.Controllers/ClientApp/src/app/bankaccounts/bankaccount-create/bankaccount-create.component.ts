import {Component, EventEmitter, OnInit, Output} from '@angular/core';
import {MessageService} from "primeng/api";
import {ActivatedRoute, Router} from "@angular/router";
import {BankAccountService} from "../../services/bankaccounts.service";
import {BankAccountCreate} from "../../models/BankAccountCreate";
import {MultiSelectModule} from "primeng/multiselect";
import {UserService} from "../../services/users.service";
import {DropdownModule} from "primeng/dropdown";
import {FormsModule} from "@angular/forms";
import {ButtonModule} from "primeng/button";
import {User} from "../../models/User";

@Component({
  selector: 'app-bankaccount-create',
  standalone: true,
  imports: [
    MultiSelectModule,
    FormsModule,
    DropdownModule,
    ButtonModule
  ],
  templateUrl: './bankaccount-create.component.html',
  styleUrl: './bankaccount-create.component.css'
})
export class BankaccountCreateComponent implements OnInit {

  constructor(private bankAccountService: BankAccountService, private usersService: UserService, private router: Router,
              private messageService: MessageService, private activatedRoute: ActivatedRoute) {
  }

  isUpdateMode: boolean = false;
  iban!: string;

  users!: User[]
  selectedUsers!: User[];
  accountsTypes: String[] = ['Saving', 'Current', 'FixedTerm', 'Payroll', 'Student'];
  selectedAccountType!: string;
  label: string = 'Crear';

  bankAccount: BankAccountCreate;
  @Output() onSave: EventEmitter<any> = new EventEmitter();
  @Output() onCancel: EventEmitter<any> = new EventEmitter();

  ngOnInit() {
    this.usersService.getAllUsers().subscribe(users => {
      this.users = users;
    });
  }

  loadBankAccount(iban: string) {
    this.bankAccountService.getBankAccountById(iban).subscribe(bankAccount => {
      this.label = 'Actualizar';
      this.bankAccount = bankAccount;
      this.selectedAccountType = bankAccount.accountType;
      this.selectedUsers = this.users.filter(user => bankAccount.usersId.includes(user.id));
      this.iban = iban;
      this.isUpdateMode = true;
    });
  }

  createBankAccount() {
    if (this.isUpdateMode) {
      this.bankAccount.accountType = this.selectedAccountType;
      this.bankAccount.usersId = this.selectedUsers.map(user => user.id);
      console.log(this.selectedAccountType)
      console.log(this.bankAccount.accountType)
      this.bankAccount = {...this.bankAccount, accountType: this.selectedAccountType, usersId: this.selectedUsers.map(user => user.id)};

      this.bankAccountService.updateBankAccount(this.bankAccount, this.iban).subscribe(() => {
        this.messageService.add({
          severity: 'success',
          summary: 'Cuenta actualizada',
          detail: 'Cuenta bancaria actualizada'
        });
        this.router.navigate(['/bankaccounts']);
        this.onSave.emit();
      });
    } else {
      this.bankAccount.accountType = this.selectedAccountType;
      this.bankAccount.usersId = this.selectedUsers.map(user => user.id);
      this.bankAccountService.addBankAccount(this.bankAccount).subscribe(() => {
        this.messageService.add({severity: 'success', summary: 'Cuenta creada', detail: 'Cuenta bancaria creada'});
        this.router.navigate(['/bankaccounts']);
        this.onSave.emit();
      });
    }

  }

  cancel() {
    this.onCancel.emit();
  }
}
