import {Component, OnInit} from '@angular/core';
import {UserService} from "./users.service";
import {CardModule} from "primeng/card";
import {ButtonModule} from "primeng/button";
import {NgOptimizedImage} from "@angular/common";
import {RouterOutlet} from "@angular/router";

export class User {
  id: string;
  name: string;
  email: string;
  avatar: string;
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
    RouterOutlet
  ],
  templateUrl: './users.component.html',
  styleUrl: './users.component.css'
})
export class UsersComponent implements OnInit{
  users: User[];
  constructor(private userService: UserService) {
  }

  ngOnInit() {
    this.userService.getUsers().subscribe((data) => {
      this.users = data;
    });
  }
}
