import {Component} from '@angular/core';
import {CardModule} from "primeng/card";
import {FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators} from "@angular/forms";
import {ButtonModule} from "primeng/button";
import {InputTextModule} from "primeng/inputtext";
import {DividerModule} from "primeng/divider";
import {ToastModule} from "primeng/toast";
import {Router, RouterLink, RouterLinkActive, RouterOutlet} from "@angular/router";
import {NgIf} from "@angular/common";
import {MessageService} from "primeng/api";
import {MessageModule} from "primeng/message";
import {UserLogin} from "../models/UserLogin";
import {AuthService} from "../services/auth.service";

export class Token {
  token: string;
  user: UserLogin;
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
    RouterOutlet,
    NgIf,
    MessageModule
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  constructor(private authService: AuthService, private router: Router, private messageService: MessageService) {
  }

  formGroup = new FormGroup({
    username: new FormControl('', [Validators.required, Validators.minLength(3), Validators.maxLength(50)]),
    password: new FormControl('', [Validators.required, Validators.minLength(3), Validators.maxLength(50)]),
  });

  sendLogin() {
    let username = this.formGroup.controls.username.value ?? '';
    let password = this.formGroup.controls.password.value ?? '';
    if (this.formGroup.valid) {
      this.authService.login(username, password).subscribe((data: Token) => {
          localStorage.setItem('token', data.token);
          this.router.navigate(['/users']);
        }
      );

    } else {
      this.messageService.add({severity: 'error', summary: 'Error', detail: 'Hay errores en el formulario'});
    }
  }

  goToRegister() {
    this.router.navigate(['/register', 'register']);
  }

  isValid() {
    return this.formGroup.valid;
  }
}

