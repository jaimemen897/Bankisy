import {Component} from '@angular/core';
import {FormControl, FormGroup, ReactiveFormsModule, Validators} from "@angular/forms";
import {UserService} from "../users.service";
import {Router, RouterOutlet} from "@angular/router";
import {UserCreate} from "../users.component";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {InputTextModule} from "primeng/inputtext";
import {SharedModule} from "primeng/api";
import {ToastModule} from "primeng/toast";
import {PasswordModule} from "primeng/password";
import {DividerModule} from "primeng/divider";

@Component({
  selector: 'app-create',
  standalone: true,
  imports: [
    ButtonModule,
    CardModule,
    InputTextModule,
    ReactiveFormsModule,
    RouterOutlet,
    SharedModule,
    ToastModule,
    PasswordModule,
    DividerModule,
  ],
  templateUrl: './users-create.component.html',
  styleUrl: './users-create.component.css'
})
export class UsersCreateComponent {

  constructor(private userService: UserService, private router: Router) {
  }

  formGroup = new FormGroup({
    name: new FormControl('', [Validators.required, Validators.minLength(3), Validators.maxLength(50)]),
    email: new FormControl('', [Validators.required, Validators.email]),
    username: new FormControl('', [Validators.required, Validators.minLength(3), Validators.maxLength(50)]),
    dni: new FormControl('', [Validators.required, Validators.minLength(9), Validators.maxLength(9)]),
    gender: new FormControl('', [Validators.required]),
    password: new FormControl('', [Validators.required, Validators.minLength(3), Validators.maxLength(50)]),
  });

  sendForm() {
    let name = this.formGroup.controls.name.value ?? '';
    let email = this.formGroup.controls.email.value ?? '';
    let password = this.formGroup.controls.password.value ?? '';

    let user: UserCreate = {
      name: name,
      email: email,
      username: this.formGroup.controls.username.value ?? '',
      dni: this.formGroup.controls.dni.value ?? '',
      gender: this.formGroup.controls.gender.value ?? '',
      password: password
    };
    if (this.formGroup.valid) {
      this.userService.addUser(user).subscribe(() => {
          this.router.navigate(['/users']).then(() => console.log('Navigate to create users'));
        }
      );

    } else {
      console.log('Form is invalid');
    }
  }
}
