import {Component} from '@angular/core';
import {UserService} from "../users.service";
import {ActivatedRoute, Router, RouterOutlet} from "@angular/router";
import {FormControl, FormGroup, ReactiveFormsModule, Validators} from "@angular/forms";
import {UserCreate} from "../users.component";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {InputTextModule} from "primeng/inputtext";
import {SharedModule} from "primeng/api";
import {ToastModule} from "primeng/toast";

@Component({
  selector: 'app-update',
  standalone: true,
  imports: [
    ButtonModule,
    CardModule,
    InputTextModule,
    ReactiveFormsModule,
    RouterOutlet,
    SharedModule,
    ToastModule
  ],
  templateUrl: './users-update.component.html',
  styleUrl: './users-update.component.css'
})
export class UsersUpdateComponent {
  user: UserCreate;

  constructor(private userService: UserService, private route: ActivatedRoute, private router: Router) {
    this.userService.getUserById(this.route.snapshot.paramMap.get('id') ?? '').subscribe(user => {
        this.formGroup.controls.name.setValue(user.name);
        this.formGroup.controls.email.setValue(user.email);
        this.formGroup.controls.username.setValue(user.username);
        this.formGroup.controls.dni.setValue(user.dni);
        this.formGroup.controls.gender.setValue(user.gender);
      }
    );
  }

  formGroup = new FormGroup({
    name: new FormControl('', [Validators.minLength(3), Validators.maxLength(50)]),
    email: new FormControl('', [Validators.email]),
    username: new FormControl('', [Validators.minLength(3), Validators.maxLength(50)]),
    dni: new FormControl('', [Validators.minLength(9), Validators.maxLength(9)]),
    gender: new FormControl('', [Validators.minLength(3), Validators.maxLength(50)]),
    password: new FormControl('', [Validators.minLength(3), Validators.maxLength(50)]),
  });

  sendForm() {
    const id = this.route.snapshot.paramMap.get('id') ?? '';
    let name = this.formGroup.controls.name.value ?? this.user.name;
    let email = this.formGroup.controls.email.value ?? this.user.email;
    let username = this.formGroup.controls.username.value ?? this.user.username;
    let dni = this.formGroup.controls.dni.value ?? this.user.dni;
    let gender = this.formGroup.controls.gender.value ?? this.user.gender;
    let password = this.formGroup.controls.password.value ?? this.user.password;

    let user: UserCreate = {
      name: name,
      email: email,
      username: username,
      dni: dni,
      gender: gender,
      password: password
    };
    if (this.formGroup.valid) {
      this.userService.updateUser(user, id).subscribe(() => {
          this.router.navigate(['/users']);
        }
      );

    } else {
      console.log('Form is invalid');
    }
  }
}
