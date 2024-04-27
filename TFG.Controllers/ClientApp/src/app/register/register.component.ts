import {Component} from '@angular/core';
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators} from "@angular/forms";
import {InputTextModule} from "primeng/inputtext";
import {Router, RouterOutlet} from "@angular/router";
import {MessageService, SharedModule} from "primeng/api";
import {ToastModule} from "primeng/toast";
import {PasswordModule} from "primeng/password";
import {RadioButtonModule} from "primeng/radiobutton";
import {Location, NgIf} from '@angular/common';
import {Token} from "../login/login.component";
import {AuthService} from "../services/auth.service";
import {UserService} from "../services/users.service";
import {UserCreate} from "../models/UserCreate";
import {DropdownModule} from "primeng/dropdown";
import {Gender} from "../models/Gender";
import {passwordMatchValidator} from "./passwordMatchValidator";

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
    RadioButtonModule,
    NgIf,
    DropdownModule
  ],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {
  constructor(private authService: AuthService, private router: Router, private location: Location, private messageService: MessageService, private usersService: UserService) {
  }

  id: string;
  genders: string[] = [Gender.Male, Gender.Female, Gender.Other, Gender.PreferNotToSay];

  formGroup = new FormGroup({
    name: new FormControl('', [Validators.required, Validators.minLength(3), Validators.maxLength(50)]),
    email: new FormControl('', [Validators.required, Validators.email]),
    username: new FormControl('', [Validators.required, Validators.minLength(3), Validators.maxLength(50)]),
    dni: new FormControl('', [Validators.required, Validators.minLength(9), Validators.maxLength(9)]),
    phone: new FormControl('', [Validators.required, Validators.minLength(9), Validators.maxLength(9)]),
    gender: new FormControl('', [Validators.required]),
    password: new FormControl('', [Validators.required, Validators.minLength(3), Validators.maxLength(50)]),
    confirmpassword: new FormControl('', [Validators.required, Validators.minLength(3), Validators.maxLength(50)])
  }, {validators: passwordMatchValidator});


  sendForm() {
    if (this.formGroup.valid) {
      let genderTranslated = Object.keys(Gender).find(key => Gender[key as keyof typeof Gender] === this.formGroup.value.gender) as keyof typeof Gender;
      let user: UserCreate = {
        name: this.formGroup.controls.name.value as string,
        email: this.formGroup.controls.email.value as string,
        username: this.formGroup.controls.username.value as string,
        dni: this.formGroup.controls.dni.value as string,
        phone: this.formGroup.controls.phone.value as string,
        gender: genderTranslated,
        password: this.formGroup.controls.password.value as string
      };
      this.authService.register(user).subscribe((data: Token) => {
          localStorage.setItem('token', data.token);
          this.router.navigate(['/index']);
        }
      );
    } else {
      this.formGroup.markAllAsTouched();
      this.messageService.add({severity: 'error', summary: 'Error', detail: 'Por favor, rellene todos los campos'});
    }
  }

  validateDNI(event: any) {
    let dni = event.target.value;
    const letters = 'TRWAGMYFPDXBNJZSQVHLCKE';
    const numbers = dni.substring(0, 8);
    const letter = dni.substring(8, 9);
    const correctLetter = letters[parseInt(numbers) % 23];
    if (correctLetter !== letter) {
      this.formGroup.controls.dni.setErrors({incorrect: true});
    } else {
      this.formGroup.controls.dni.setErrors(null);
    }
  }

  goBack() {
    this.location.back();
  }
}
