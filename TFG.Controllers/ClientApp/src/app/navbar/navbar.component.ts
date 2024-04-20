import {Component, OnInit, ViewChild} from '@angular/core';
import {MenubarModule} from "primeng/menubar";
import {ButtonModule} from "primeng/button";
import {NgIf} from "@angular/common";
import {DropdownModule} from "primeng/dropdown";
import {ToastModule} from "primeng/toast";
import {ConfirmPopup, ConfirmPopupModule} from "primeng/confirmpopup";
import {ConfirmationService, MenuItem, MessageService} from "primeng/api";
import {IndexService} from "../services/index.service";
import {User} from "../models/User";
import {MenuModule} from "primeng/menu";

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
    MenuModule
  ],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css'
})
export class NavbarComponent implements OnInit {

  constructor(private confirmationService: ConfirmationService, private indexService: IndexService) {
  }

  user!: User;
  items: MenuItem[];

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

  confirm(event: Event) {
    event.stopImmediatePropagation();
    this.confirmationService.confirm({
      target: event.target as EventTarget,
      message: '¿Estás seguro?',
      accept: () => {
        this.logout();
      },

    });
  }

}
