import {Component, ViewChild} from '@angular/core';
import {CardModule} from "primeng/card";
import {ButtonModule} from "primeng/button";
import {NgClass, NgForOf, NgOptimizedImage} from "@angular/common";
import {RouterOutlet} from "@angular/router";
import {TagModule} from "primeng/tag";
import {RatingModule} from "primeng/rating";
import {DataViewLazyLoadEvent, DataViewModule} from "primeng/dataview";
import {FormsModule} from "@angular/forms";
import {SpeedDialModule} from "primeng/speeddial";
import {ToastModule} from "primeng/toast";
import {ConfirmationService, MessageService, SelectItem} from "primeng/api";
import {PaginatorModule} from "primeng/paginator";
import {ConfirmDialogModule} from "primeng/confirmdialog";
import {OverlayPanelModule} from "primeng/overlaypanel";
import {InputTextModule} from "primeng/inputtext";
import {User} from "../models/User";
import {UserService} from "../services/users.service";
import {FileUploadModule} from "primeng/fileupload";
import {CreateUsersComponent} from "./create-users/create-users.component";
import {BankaccountCreateComponent} from "../bankaccounts/bankaccount-create/bankaccount-create.component";
import {DialogModule} from "primeng/dialog";

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [
    CardModule,
    ButtonModule,
    NgOptimizedImage,
    RouterOutlet,
    TagModule,
    RatingModule,
    DataViewModule,
    FormsModule,
    NgClass,
    NgForOf,
    SpeedDialModule,
    ToastModule,
    PaginatorModule,
    ConfirmDialogModule,
    OverlayPanelModule,
    InputTextModule,
    FileUploadModule,
    BankaccountCreateComponent,
    DialogModule,
    CreateUsersComponent
  ],
  templateUrl: './users.component.html',
  styleUrl: './users.component.css'
})
export class UsersComponent {
  constructor(private userService: UserService, private confirmationService: ConfirmationService, private messageService: MessageService) {
  }

  @ViewChild(CreateUsersComponent) createUsersComponent!: CreateUsersComponent;

  users!: User[];

  layout: 'list' | 'grid' = 'list';
  rows: number = 5;
  totalRecords: number = 0;

  sortOptions!: SelectItem[];
  sortField!: string;
  sortOrder!: number;
  search!: string;

  headerSaveUpdateUser: string = 'Crear usuario';
  displayDialog: boolean = false;

  lazyLoad(event: DataViewLazyLoadEvent) {
    let pageNumber = event.first / event.rows;
    if (pageNumber < 1) pageNumber = 1; else pageNumber++;

    this.userService.getUsers(pageNumber, event.rows, this.sortField, this.sortOrder === -1, this.search).subscribe((data) => {
      this.users = data.items;
      this.totalRecords = data.totalCount;
    });
    this.sortOptions = [
      {label: 'Nombre ascendiente', value: 'name', icon: 'pi pi-sort-alpha-down'},
      {label: 'Nombre descendiente', value: '!name', icon: 'pi pi-sort-alpha-up'},
    ];
  }

  onSearch(event: any) {
    this.userService.getUsers(1, this.rows, this.sortField, this.sortOrder === -1, event.target.value).subscribe((data) => {
      this.users = data.items;
      this.totalRecords = data.totalCount;
    });
  }

  onSortChange(event: any) {
    let value = event.value;

    if (value.indexOf('!') === 0) {
      this.sortOrder = -1;
      this.sortField = value.substring(1, value.length);
    } else {
      this.sortOrder = 1;
      this.sortField = value;
    }
  }

  getRole(user: User) {
    switch (user.role) {
      case 'Admin':
        return 'success';
      case 'User':
        return 'primary';
      default:
        return 'warning';
    }
  }

  getGenderName(gender: string) {
    switch (gender) {
      case 'Male':
        return 'Hombre';
      case 'Female':
        return 'Mujer';
      case 'Other':
        return 'Otro';
      case 'PreferNotToSay':
        return 'Prefiere no decir';
      default:
        return 'Desconocido';
    }
  }

  goToAddUser() {
    this.headerSaveUpdateUser = 'Crear usuario';
    this.displayDialog = true;
  }

  goToEditUser(id: string) {
    this.createUsersComponent.updateUser(id);
    this.headerSaveUpdateUser = 'Actualizar usuario';
    this.displayDialog = true;
  }

  saveBankAccount() {
    this.displayDialog = false;
    this.lazyLoad({first: 1, rows: this.rows, sortField: this.sortField, sortOrder: this.sortOrder})
  }

  closeDialog() {
    this.displayDialog = false;
  }

  delete(id: string) {
    this.confirmationService.confirm({
      header: '¿Desea eliminar el usuario?',
      message: 'Confirme para continuar',
      accept: () => {
        this.messageService.add({
          severity: 'info',
          summary: 'Eliminado',
          detail: 'Usuario eliminado',
          life: 3000,
          closable: false
        });
        this.userService.deleteUser(id).subscribe(() => {
          this.userService.getUsers(1, this.rows).subscribe((data) => {
            this.users = data.items;
            this.totalRecords = data.totalCount;
          });
        });
      },
      reject: () => {
        this.messageService.add({
          severity: 'error',
          summary: 'Cancelar',
          detail: 'No se ha eliminado',
          life: 3000,
          closable: false
        });
      }
    });
  }

  onUpload() {
    this.messageService.add({
      severity: 'info',
      summary: 'Subido',
      detail: 'Avatar subido',
      life: 3000,
      closable: false
    });

    this.userService.getUsers(1, this.rows).subscribe((data) => {
      this.users = data.items;
      this.totalRecords = data.totalCount;
    });
  }
}
