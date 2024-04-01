import {Component} from '@angular/core';
import {MenubarModule} from "primeng/menubar";

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [
    MenubarModule
  ],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css'
})
export class NavbarComponent {
  items = [
    {
      label: 'Inicio',
      icon: 'pi pi-fw pi-home',
      routerLink: ['/']
    },
    {
      label: 'Secciones',
      icon: 'pi pi-fw pi-list',
      items: [
        {
          label: 'Usuarios',
          icon: 'pi pi-fw pi-user',
          routerLink: ['/users']
        },
        {
          label: 'Cuentas de banco',
          icon: 'pi pi-fw pi-money-bill',
          routerLink: ['/bankaccounts']
        },
        {
          label: 'Transacciones',
          icon: 'pi pi-fw pi-money-bill',
          routerLink: ['/transactions'],
        },
        {
          label: 'Tarjetas',
          icon: 'pi pi-fw pi-credit-card',
          routerLink: ['/cards']
        }
      ]
    },
    {
      label: 'Sesión',
      icon: 'pi pi-fw pi-user',
      items: [
        {
          label: 'Iniciar sesión',
          icon: 'pi pi-fw pi-sign-in',
          visible: !this.isLogged(),
          routerLink: ['/login']
        },
        {
          label: 'Registrarse',
          icon: 'pi pi-fw pi-user-plus',
          visible: !this.isLogged(),
          routerLink: ['/register']
        },
        {
          label: 'Cerrar sesión',
          icon: 'pi pi-fw pi-sign-out',
          visible: this.isLogged(),
          command: () => this.logout()
        }
      ]
    }
  ];

  logout() {
    localStorage.removeItem('token');
    location.reload();
  }

  isLogged() {
    return localStorage.getItem('token') !== null;
  }

}
