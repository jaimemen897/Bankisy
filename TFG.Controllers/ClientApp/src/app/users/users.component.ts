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
import {MenuItem, MessageService, PrimeNGConfig} from "primeng/api";
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
export class UsersComponent implements OnInit{
  constructor(private userService: UserService, private messageService: MessageService, private primengConfig: PrimeNGConfig) {
  }

  users!: User[];
  layout: 'list' | 'grid' = 'list';
  actions: MenuItem[] = [];

  /*pagination*/
  first: number = 0;
  rows: number = 2;
  totalRecords: number = 0;

  ngOnInit() {
    this.users = [];
    this.userService.getUsers(this.first, this.rows).subscribe((data) => {
      this.users = data;
      this.totalRecords = data.totalRecords;
      console.log(this.users);
      console.log(this.totalRecords);
    });

    this.actions = [
      {
        icon: 'pi pi-pencil',
        command: () => {
          this.messageService.add({ severity: 'info', summary: 'Add', detail: 'Data Added' });
        }
      },
      {
        icon: 'pi pi-refresh',
        command: () => {
          this.messageService.add({ severity: 'success', summary: 'Update', detail: 'Data Updated' });
        }
      },
      {
        icon: 'pi pi-trash',
        command: () => {
          this.messageService.add({ severity: 'error', summary: 'Delete', detail: 'Data Deleted' });
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
    this.primengConfig.ripple = true;
  }

  onPageChange(event: PaginatorState) {
    const page = (event.page  || 0) + 1;
    const pageSize = event.rows || 10;
    this.userService.getUsers(page, pageSize).subscribe((response) => {
      this.users = response.data;
      this.totalRecords = response.totalRecords;
    });
  }
}
