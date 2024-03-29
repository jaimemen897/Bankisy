import {Component, EventEmitter, Output} from '@angular/core';
import {ButtonModule} from "primeng/button";
import {DropdownModule} from "primeng/dropdown";
import {MultiSelectModule} from "primeng/multiselect";
import {FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators} from "@angular/forms";
import {BankAccountService} from "../../services/bankaccounts.service";
import {UserService} from "../../services/users.service";
import {Router} from "@angular/router";
import {MessageService} from "primeng/api";
import {IndexService} from "../../services/index.service";
import {User} from "../../models/User";
import {BankAccount} from "../../models/BankAccount";
import {CardType} from "../../models/CardType";
import {CardService} from "../../services/card.service";
import {CardCreate} from "../../models/CardCreate";
import {InputOtpModule} from 'primeng/inputotp';
import {InputTextModule} from "primeng/inputtext";


@Component({
  selector: 'app-create-card',
  standalone: true,
  imports: [
    ButtonModule,
    DropdownModule,
    MultiSelectModule,
    ReactiveFormsModule,
    FormsModule,
    InputOtpModule,
    InputTextModule
  ],
  templateUrl: './create-card.component.html',
  styleUrl: './create-card.component.css'
})
export class CreateCardComponent {
  constructor(private bankAccountService: BankAccountService, private usersService: UserService, private router: Router,
              private messageService: MessageService, private indexService: IndexService, private cardService: CardService) {
  }

  formGroup: FormGroup = new FormGroup({
    pin: new FormControl('', [Validators.required, Validators.minLength(4), Validators.maxLength(4)]),
    selectedCardType: new FormControl('', [Validators.required]),
    selectedUser: new FormControl<User | undefined>(undefined, [Validators.required]),
    selectedBankAccount: new FormControl<BankAccount | undefined>(undefined, [Validators.required])
  });

  isNewUser: boolean = false;
  isUpdateMode: boolean = false;
  cardNumber!: string;

  users!: User[]
  bankAccounts!: BankAccount[]
  cardsType: string[] = [CardType.Debit, CardType.Visa, CardType.Credit, CardType.Prepaid, CardType.Virtual, CardType.AmericanExpress, CardType.MasterCard];

  label: string = 'Crear';

  @Output() onSave: EventEmitter<any> = new EventEmitter();
  @Output() onCancel: EventEmitter<any> = new EventEmitter();

  //CHANGE SELECTED USERS VALIDATORS DEPENDING ON THE MODE
  updateSelectedUserValidators() {
    const selectedUserControl = this.formGroup.get('selectedUser') as FormControl;
    if (this.isNewUser) {
      selectedUserControl.setValidators(null);
    } else {
      selectedUserControl.setValidators([Validators.required]);
    }
    selectedUserControl.updateValueAndValidity();
  }

  //USERS CREATE A CARD
  loadUser(userId: User) {
    this.isNewUser = true;
    this.updateSelectedUserValidators();
    this.formGroup.controls.selectedUser.setValue([userId]);
    this.indexService.getBankAccountsByUserId().subscribe(bankAccounts => {
      this.bankAccounts = bankAccounts;
    });
  }

  //ADMIN CREATE AN ACCOUNT FOR A USER
  loadUsers() {
    this.isNewUser = false;
    this.usersService.getAllUsers().subscribe(users => {
      this.users = users;
    });
    this.updateSelectedUserValidators();
  }

  //LOAD BANK ACCOUNTS FOR THE SELECTED USER (ADMIN)
  loadBankAccounts(selectedBankAccount?: BankAccount) {
    let userId = this.formGroup.controls.selectedUser.value.id;
    this.bankAccountService.getBankAccountsByUserId(userId).subscribe(bankAccounts => {
      this.bankAccounts = bankAccounts;
      if (this.bankAccounts.length > 0) {
        this.formGroup.controls.selectedBankAccount.setValue(this.bankAccounts.find(bankAccount => bankAccount.iban === selectedBankAccount?.iban));
      }
    });
  }

  //ADMIN UPDATE AN ACCOUNT FOR A USER
  loadCard(cardNumber: string) {
    if (!this.isNewUser) {
      this.cardService.getCard(cardNumber).subscribe(card => {
        this.isNewUser = false;
        this.isUpdateMode = true;
        this.updateSelectedUserValidators();
        this.label = 'Actualizar';
        let cardType = CardType[card.cardType as keyof typeof CardType];

        this.cardNumber = cardNumber;
        this.formGroup.controls.selectedUser.setValue(card.user);
        this.loadUsers();
        this.loadBankAccounts(card.bankAccount);
        this.formGroup.controls.selectedBankAccount.setValue(card.bankAccount);
        this.formGroup.controls.selectedCardType.setValue(cardType);
        this.formGroup.controls.pin.setValue(card.pin);
      });
    }
  }

  saveChanges() {
    if (!this.formGroup.valid) {
      this.messageService.add({severity: 'error', summary: 'Error', detail: 'Por favor, rellene todos los campos'});
      return;
    }
    //TRANSLATE CARD TYPE
    let cardTypeTranslated = Object.keys(CardType).find(key => CardType[key as keyof typeof CardType] === this.formGroup.value.selectedCardType) as keyof typeof CardType;

    //UPDATE
    if (this.isUpdateMode) {
      let userId = this.formGroup.value.selectedUser.id;

      let card = new CardCreate();
      card.cardType = cardTypeTranslated
      card.userId = userId;
      card.bankAccountIban = this.formGroup.controls.selectedBankAccount.value.iban;
      card.pin = this.formGroup.controls.pin.value;

      this.cardService.updateCard(card, this.cardNumber).subscribe(() => {
        this.messageService.add({
          severity: 'success', summary: 'Tarjeta actualizada', detail: 'Tarjeta actualizada'
        });
        this.router.navigate(['/cards']);
        this.onSave.emit();
      });

      //USER CREATE
    } else if (this.isNewUser) {
      let card = new CardCreate();
      card.cardType = cardTypeTranslated;
      card.userId = this.formGroup.controls.selectedUser.value.id;
      card.bankAccountIban = this.formGroup.controls.selectedBankAccount.value.iban;
      card.pin = this.formGroup.controls.pin.value;

      this.indexService.createCard(card).subscribe(() => {
        this.formGroup.reset();
        this.onSave.emit();
      });

      //ADMIN CREATE
    } else {
      let card = new CardCreate();
      card.cardType = cardTypeTranslated;
      card.userId = this.formGroup.value.selectedUser.id;
      card.bankAccountIban = this.formGroup.value.selectedBankAccount.iban;
      card.pin = this.formGroup.value.pin;

      this.cardService.addCard(card).subscribe(() => {
        this.messageService.add({severity: 'success', summary: 'Tarjeta creada', detail: 'Tarjeta creada'});
        this.formGroup.reset();
        this.router.navigate(['/cards']);
        this.onSave.emit();
      });
    }
  }

  cancel() {
    this.formGroup.reset();
    this.onCancel.emit();
  }

  protected readonly console = console;
}
