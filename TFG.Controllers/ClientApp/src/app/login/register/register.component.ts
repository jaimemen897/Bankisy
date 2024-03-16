import { Component } from '@angular/core';
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators} from "@angular/forms";
import {InputTextModule} from "primeng/inputtext";
import {Router, RouterOutlet} from "@angular/router";
import {SharedModule} from "primeng/api";
import {ToastModule} from "primeng/toast";
import {AuthService} from "../auth.service";
import {Token} from "../login.component";
import {PasswordModule} from "primeng/password";
import {RadioButtonModule} from "primeng/radiobutton";

@Component({
  selector: 'app-register',
  standalone: true,
    imports: [
        ButtonModule,
        CardModule,
        FormsModule,
        InputTextModule,
        ReactiveFormsModule,
        RouterOutlet,
        SharedModule,
        ToastModule,
        PasswordModule,
        RadioButtonModule
    ],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {
  constructor(private authService: AuthService, private router: Router) {
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
    let username = this.formGroup.controls.username.value ?? '';
    let dni = this.formGroup.controls.dni.value ?? '';
    let gender = this.formGroup.controls.gender.value ?? '';
    let password = this.formGroup.controls.password.value ?? '';
    if (this.formGroup.valid) {
      this.authService.register(name, email, username, dni, gender, password).subscribe((data: Token) => {
          localStorage.setItem('token', data.token);
          this.router.navigate(['/users']);
        }
      );

    } else {
      console.log('Form is invalid');
    }
  }

}
