import {Component} from '@angular/core';
import {UserService} from "./users.service";
import {CardModule} from "primeng/card";
import {ButtonModule} from "primeng/button";
import {NgClass, NgForOf, NgOptimizedImage} from "@angular/common";
import {Router, RouterOutlet} from "@angular/router";
import {TagModule} from "primeng/tag";
import {RatingModule} from "primeng/rating";
import {DataViewLazyLoadEvent, DataViewModule} from "primeng/dataview";
import {FormsModule} from "@angular/forms";
import {SpeedDialModule} from "primeng/speeddial";
import {ToastModule} from "primeng/toast";
import {ConfirmationService, MessageService} from "primeng/api";
import {PaginatorModule} from "primeng/paginator";
import {ConfirmDialogModule} from "primeng/confirmdialog";
import {OverlayPanelModule} from "primeng/overlaypanel";

export class User {
  id: string;
  name: string;
  email: string;
  username: string;
  dni: string;
  gender: string;
  avatar: string;
  role: string;
  isDeleted: boolean;
  createdAt: Date;
  updatedAt: Date;
}

export class UserCreate {
  name: string;
  email: string;
  username: string;
  dni: string
  gender: string;
  password: string;
}

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
    OverlayPanelModule
  ],
  templateUrl: './users.component.html',
  styleUrl: './users.component.css'
})
export class UsersComponent {
  constructor(private userService: UserService, private router: Router, private confirmationService: ConfirmationService, private messageService: MessageService) {
  }

  users!: User[];

  layout: 'list' | 'grid' = 'list';
  rows: number = 5;
  totalRecords: number = 0;

  lazyLoad(event: DataViewLazyLoadEvent) {
    let pageNumber = event.first / event.rows;
    if (pageNumber < 1) pageNumber = 1; else pageNumber++;
    this.userService.getUsers(pageNumber, event.rows).subscribe((data) => {
      this.users = data.items;
      this.totalRecords = data.totalCount;
    });
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

  goToAddUser() {
    this.router.navigate(['/users/create']).then(() => console.log('Navigate to add users'));
  }

  goToEditUser(id: string) {
    this.router.navigate(['/users/edit/' + id]).then(() => {
    });
  }

  deleteUser(id: string) {
    this.userService.deleteUser(id).subscribe(() => {
      this.userService.getUsers(1, this.rows).subscribe((data) => {
        this.users = data.items;
        this.totalRecords = data.totalCount;
      });
    });
  }

  goToFileUpload(id: string) {
    this.router.navigate(['/users/upload/' + id]).then(() => console.log('Navigate to file upload'));
  }

  confirm(id:string) {
    this.confirmationService.confirm({
      header: '¿Desea eliminar el usuario?',
      message: 'Confirme para continuar',
      accept: () => {
        this.messageService.add({severity: 'info', summary: 'Eliminado', detail: 'Usuario eliminado', life: 3000});
        this.userService.deleteUser(id).subscribe(() => {
          this.userService.getUsers(1, this.rows).subscribe((data) => {
            this.users = data.items;
            this.totalRecords = data.totalCount;
          });
        });
      },
      reject: () => {
        this.messageService.add({severity: 'error', summary: 'Cancelar', detail: 'No se ha eliminado', life: 3000});
      }
    });
  }
}
