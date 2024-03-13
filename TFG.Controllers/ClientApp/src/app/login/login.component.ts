import {Component} from '@angular/core';
import {CardModule} from "primeng/card";
import {FormControl, FormGroup, FormsModule, NgForm, Validators} from "@angular/forms";
import {ButtonModule} from "primeng/button";
import {AuthService} from "./auth.service";
import {InputTextModule} from "primeng/inputtext";

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CardModule,
    FormsModule,
    ButtonModule,
    InputTextModule
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  constructor(private authService: AuthService) {
  }

  formGroup = new FormGroup({
    email: new FormControl('', [Validators.required, Validators.email]),
    password: new FormControl('', [Validators.required, Validators.minLength(5), Validators.maxLength(50)]),
  });

  onSubmit() {
    if (this.formGroup.valid) {
      this.authService.login(this.formGroup.value.email ?? 'si', this.formGroup.value.password ?? 'si');
    }
  }
}

