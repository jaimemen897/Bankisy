import {Component, Inject, OnInit, ViewChild} from '@angular/core';
import {MenubarModule} from "primeng/menubar";
import {ButtonModule} from "primeng/button";
import {DatePipe, DOCUMENT, NgIf} from "@angular/common";
import {DropdownModule} from "primeng/dropdown";
import {ToastModule} from "primeng/toast";
import {ConfirmPopup, ConfirmPopupModule} from "primeng/confirmpopup";
import {MenuItem, MessageService} from "primeng/api";
import {IndexService} from "../services/index.service";
import {User} from "../models/User";
import {MenuModule} from "primeng/menu";
import {ConfirmDialogModule} from "primeng/confirmdialog";
import {InputSwitchModule} from "primeng/inputswitch";
import {FormsModule} from "@angular/forms";
import {BizumCreateComponent} from "../transactions/bizum-create/bizum-create.component";
import {DialogModule} from "primeng/dialog";
import {AvatarModule} from "primeng/avatar";
import {CreateTransactionComponent} from "../transactions/create/create-transaction.component";
import {PanelModule} from "primeng/panel";
import {TableModule} from "primeng/table";
import {TagModule} from "primeng/tag";
import {BankaccountCreateComponent} from "../bankaccounts/bankaccount-create/bankaccount-create.component";

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [
    MenubarModule,
    ButtonModule,
    NgIf,
    DropdownModule,
    ToastModule,
    ConfirmPopupModule,
    MenuModule,
    ConfirmDialogModule,
    InputSwitchModule,
    FormsModule,
    BizumCreateComponent,
    DialogModule,
    AvatarModule,
    CreateTransactionComponent,
    DatePipe,
    PanelModule,
    TableModule,
    TagModule,
    BankaccountCreateComponent
  ],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css'
})
export class NavbarComponent implements OnInit {

  constructor(@Inject(DOCUMENT) private document: Document, private indexService: IndexService, private messageService: MessageService) {
    let theme = window.localStorage.getItem('theme');
    if (theme) {
      this.themeSelection = theme == 'dark';
      this.changeTheme(this.themeSelection);
      this.document.documentElement.classList.add(theme + '-theme');
    }
  }

  user!: User;
  items: MenuItem[];
  visibleSidebar: boolean = false;
  themeSelection: boolean = false;

  displayDialogBankAccount: boolean = false;
  displayDialogBankAccountNewUser: boolean = false;
  displayCreateBankAccount: boolean = false;
  displayDialogTransaction: boolean = false;
  displayDialogBizum: boolean = false;

  @ViewChild(CreateTransactionComponent) transactionCreate!: CreateTransactionComponent
  @ViewChild(BankaccountCreateComponent) bankAccountCreate!: BankaccountCreateComponent
  @ViewChild(BizumCreateComponent) bizumCreateComponent!: BizumCreateComponent

  ngOnInit() {
    this.indexService.getUserByToken().subscribe(
      data => {
        this.user = data;
      });
    this.items = [
      {
        label: 'Usuarios',
        icon: 'pi pi-user',
        routerLink: ['/users']
      },
      {
        label: 'Cuentas de banco',
        icon: 'pi pi-money-bill',
        routerLink: ['/bankaccounts']
      },
      {
        label: 'Transacciones',
        icon: 'pi pi-money-bill',
        routerLink: ['/transactions']
      },
      {
        label: 'Tarjetas',
        icon: 'pi pi-credit-card',
        routerLink: ['/cards']
      }

    ];
  }

  @ViewChild('logoutMes') confirmPopup!: ConfirmPopup;

  logout() {
    localStorage.removeItem('token');
    location.reload();
  }

  isLogged() {
    return localStorage.getItem('token') !== null;
  }

  accept() {
    this.confirmPopup.accept();
  }

  reject() {
    this.confirmPopup.reject();
  }

  changeTheme(state: boolean) {
    let theme = state ? 'dark' : 'light';
    window.localStorage.setItem('theme', theme);
    let themeLink = this.document.getElementById('app-theme') as HTMLLinkElement;
    this.document.documentElement.classList.remove('dark-theme', 'light-theme');
    this.document.documentElement.classList.add(theme + '-theme');
    themeLink.href = 'aura-' + theme + '-blue.css';
  }

  goToCreateBankAccount() {
    this.bankAccountCreate.loadUser(this.user);
    this.displayDialogBankAccount = true;
  }

  createBankAccount() {
    this.displayDialogBankAccount = false;
    this.displayDialogBankAccountNewUser = false;
    this.messageService.add({
      severity: 'success',
      summary: 'Cuenta creada',
      detail: 'La cuenta se ha creado correctamente',
      life: 2000,
      closable: false
    });
  }

  goToCreateTransaction() {
    this.transactionCreate.loadUser();
    this.displayDialogTransaction = true;
  }

  createTransaction() {
    this.displayDialogTransaction = false;
    this.transactionCreate.loadUser();
    this.messageService.add({
      severity: 'success',
      summary: 'Transacción creada',
      detail: 'La transacción se ha creado correctamente',
      life: 2000,
      closable: false
    });
  }

  goToCreateBizum() {
    this.bizumCreateComponent.loadUser();
    this.displayDialogBizum = true;
  }

  createBizum() {
    this.displayDialogBizum = false;
    this.bizumCreateComponent.loadUser()
    this.messageService.add({
      severity: 'success',
      summary: 'Bizum realizado',
      detail: 'El Bizum se ha realizado correctamente',
      life: 2000,
      closable: false
    });
  }

  closeDialog() {
    this.displayDialogBankAccount = false;
    this.displayDialogTransaction = false;
    this.displayDialogBizum = false;
    this.displayDialogBankAccountNewUser = false;
    this.displayCreateBankAccount = false;
  }
}
