import {Component, ViewChild} from '@angular/core';
import {MenubarModule} from "primeng/menubar";
import {ButtonModule} from "primeng/button";
import {NgIf} from "@angular/common";
import {DropdownModule} from "primeng/dropdown";
import {ToastModule} from "primeng/toast";
import {ConfirmPopup, ConfirmPopupModule} from "primeng/confirmpopup";
import {ConfirmationService, MessageService} from "primeng/api";

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [
    MenubarModule,
    ButtonModule,
    NgIf,
    DropdownModule,
    ToastModule,
    ConfirmPopupModule
  ],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css'
})
export class NavbarComponent {

  constructor(private confirmationService: ConfirmationService) {
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
