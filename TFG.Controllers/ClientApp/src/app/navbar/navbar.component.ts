import {Component, Inject, OnInit, ViewChild} from '@angular/core';
import {MenubarModule} from "primeng/menubar";
import {ButtonModule} from "primeng/button";
import {DOCUMENT, NgIf} from "@angular/common";
import {DropdownModule} from "primeng/dropdown";
import {ToastModule} from "primeng/toast";
import {ConfirmPopup, ConfirmPopupModule} from "primeng/confirmpopup";
import {ConfirmationService, MenuItem, MessageService} from "primeng/api";
import {IndexService} from "../services/index.service";
import {User} from "../models/User";
import {MenuModule} from "primeng/menu";
import {ConfirmDialogModule} from "primeng/confirmdialog";
import {InputSwitchModule} from "primeng/inputswitch";
import {FormsModule} from "@angular/forms";

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
    FormsModule
  ],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css'
})
export class NavbarComponent implements OnInit {

  constructor(@Inject(DOCUMENT) private document: Document, private indexService: IndexService) {
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
}
