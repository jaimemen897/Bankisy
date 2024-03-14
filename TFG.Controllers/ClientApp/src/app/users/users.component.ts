import {Component, OnInit} from '@angular/core';
import {UserService} from "./users.service";
import {CardModule} from "primeng/card";
import {ButtonModule} from "primeng/button";
import {NgClass, NgForOf, NgOptimizedImage} from "@angular/common";
import {RouterOutlet} from "@angular/router";
import {TagModule} from "primeng/tag";
import {RatingModule} from "primeng/rating";
import {DataViewModule} from "primeng/dataview";
import {FormsModule} from "@angular/forms";
import {SpeedDialModule} from "primeng/speeddial";
import {ToastModule} from "primeng/toast";
import {MenuItem, MessageService} from "primeng/api";
import {PaginatorModule, PaginatorState} from "primeng/paginator";

export class User {
  id: string;
  name: string;
  email: string;
  avatar: string;
  role: string;
  isDeleted: boolean;
  createdAt: Date;
  updatedAt: Date;
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
    PaginatorModule
  ],
  templateUrl: './users.component.html',
  styleUrl: './users.component.css'
})
export class UsersComponent implements OnInit {
  constructor(private userService: UserService, private messageService: MessageService) {
  }

  users!: User[];
  layout: 'list' | 'grid' = 'list';
  actions: MenuItem[] = [];

  /*pagination*/
  first: number = 1;
  rows: number = 5;
  totalRecords: number = 0;

  ngOnInit() {
    this.userService.getUsers(this.first, this.rows).subscribe((data) => {
      this.users = data.items;
      this.totalRecords = data.totalCount;
    });

    this.actions = [
      {
        icon: 'pi pi-pencil',
        command: () => {
          this.messageService.add({severity: 'info', summary: 'Add', detail: 'Data Added'});
        }
      },
      {
        icon: 'pi pi-refresh',
        command: () => {
          this.messageService.add({severity: 'success', summary: 'Update', detail: 'Data Updated'});
        }
      },
      {
        icon: 'pi pi-trash',
        command: () => {
          this.messageService.add({severity: 'error', summary: 'Delete', detail: 'Data Deleted'});
        }
      },
      {
        icon: 'pi pi-upload',
        routerLink: ['/fileupload']
      },
      {
        icon: 'pi pi-external-link',
        target: '_blank',
        url: 'http://angular.io'
      }
    ];
  }

  onPageChange(event: PaginatorState) {
    const page = event.page || 0;
    const pageSize = event.rows || 10;

    this.first = event.first || 0;

    this.userService.getUsers(page, pageSize).subscribe((data) => {
      this.users = data.items;
      this.totalRecords = data.totalCount;
    });
  }

  getRole(user: User) {
    switch (user.role) {
      case 'Admin':
        return 'success';
      case 'User':
        return 'info';
      default:
        return 'warning';
    }
  }
}
