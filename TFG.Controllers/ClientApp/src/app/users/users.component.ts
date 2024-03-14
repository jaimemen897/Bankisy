import {AfterViewInit, Component, OnInit} from '@angular/core';
import {UserService} from "./users.service";
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

export class UserCreate {
  name: string;
  email: string;
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
    PaginatorModule
  ],
  templateUrl: './users.component.html',
  styleUrl: './users.component.css'
})
export class UsersComponent implements OnInit{
  constructor(private userService: UserService) {
  }

  users!: User[];
  layout: 'list' | 'grid' = 'list';
  actions: MenuItem[] = [];

  rows: number = 5;
  totalRecords: number = 0;

  ngOnInit() {
    this.actions = [
      {
        icon: 'pi pi-eye',
        routerLink: ['/edit']
      },
      {
        icon: 'pi pi-user-edit',
        routerLink: ['/edit']
      },
      {
        icon: 'pi pi-trash',
        routerLink: ['/delete']
      },
      {
        icon: 'pi pi-image',
        routerLink: ['/fileupload']
      },
      {
        icon: 'pi pi-external-link',
        target: '_blank',
      }
    ];
  }

  lazyLoad(event: DataViewLazyLoadEvent) {
    let pageNumber = event.first/event.rows;
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
}
