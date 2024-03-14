import {Component} from '@angular/core';
import {CardModule} from "primeng/card";
import {FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators} from "@angular/forms";
import {ButtonModule} from "primeng/button";
import {AuthService} from "./auth.service";
import {InputTextModule} from "primeng/inputtext";
import {DividerModule} from "primeng/divider";
import {ToastModule} from "primeng/toast";
import {Router, RouterLink, RouterLinkActive, RouterOutlet} from "@angular/router";

export class Token {
  token: string;
  user: User;
}

export class User {
  Id: string;
  Name: string;
  Email: string;
  Role: number;
}

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CardModule,
    FormsModule,
    ButtonModule,
    InputTextModule,
    ReactiveFormsModule,
    DividerModule,
    ToastModule,
    RouterLink,
    RouterLinkActive,
    RouterOutlet
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  constructor(private authService: AuthService, private router: Router) {
  }

  formGroup = new FormGroup({
    email: new FormControl('', [Validators.required, Validators.email]),
    password: new FormControl('', [Validators.required, Validators.minLength(3), Validators.maxLength(50)]),
  });

  sendLogin() {
    let email = this.formGroup.controls.email.value ?? '';
    let password = this.formGroup.controls.password.value ?? '';
    if (this.formGroup.valid) {
      this.authService.login(email, password).subscribe((data: Token) => {
          localStorage.setItem('token', data.token);
          this.router.navigate(['/users']);
        }
      );

    } else {
      console.log(this.formGroup.controls.email.value, this.formGroup.controls.password.value);
    }
  }

  goToRegister() {
    this.router.navigate(['/register']);
  }
}

