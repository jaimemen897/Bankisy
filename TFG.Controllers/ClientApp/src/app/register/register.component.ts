import {Component, OnInit} from '@angular/core';
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators} from "@angular/forms";
import {InputTextModule} from "primeng/inputtext";
import {ActivatedRoute, Router, RouterOutlet} from "@angular/router";
import {MessageService, SharedModule} from "primeng/api";
import {ToastModule} from "primeng/toast";
import {PasswordModule} from "primeng/password";
import {RadioButtonModule} from "primeng/radiobutton";
import {Location, NgIf} from '@angular/common';
import {AuthService} from "../auth/auth.service";
import {UserCreate} from "../users/users.component";
import {UserService} from "../users/users.service";
import {Token} from "../login/login.component";

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
    NgIf
  ],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent implements OnInit {
  constructor(private authService: AuthService, private router: Router, private location: Location, private messageService: MessageService, private route: ActivatedRoute, private usersService: UserService) {
  }

  mode: 'create' | 'update' | 'register';
  id: string;

  formGroup = new FormGroup({
    name: new FormControl('', [Validators.required, Validators.minLength(3), Validators.maxLength(50)]),
    email: new FormControl('', [Validators.required, Validators.email]),
    username: new FormControl('', [Validators.required, Validators.minLength(3), Validators.maxLength(50)]),
    dni: new FormControl('', [Validators.required, Validators.minLength(9), Validators.maxLength(9)]),
    gender: new FormControl('', [Validators.required]),
    password: new FormControl('', [Validators.required, Validators.minLength(3), Validators.maxLength(50)]),
  });

  ngOnInit() {
    this.id = this.route.snapshot.paramMap.get('id') as string;
    this.mode = this.route.snapshot.paramMap.get('mode') as 'create' | 'update' | 'register';

    if (this.mode === 'update') {
      this.formGroup.controls.password.clearValidators();
      this.formGroup.controls.password.setValidators([Validators.minLength(3), Validators.maxLength(50)]);
      this.usersService.getUserById(this.route.snapshot.paramMap.get('id') ?? '').subscribe(user => {
          this.formGroup.controls.name.setValue(user.name);
          this.formGroup.controls.email.setValue(user.email);
          this.formGroup.controls.username.setValue(user.username);
          this.formGroup.controls.dni.setValue(user.dni);
          this.formGroup.controls.gender.setValue(user.gender);
        }
      );
    }
  }

  sendForm() {
    const id = this.route.snapshot.paramMap.get('id') ?? '';
    let name = this.formGroup.controls.name.value ?? '';
    let email = this.formGroup.controls.email.value ?? '';
    let username = this.formGroup.controls.username.value ?? '';
    let dni = this.formGroup.controls.dni.value ?? '';
    let gender = this.formGroup.controls.gender.value ?? '';
    let password = this.formGroup.controls.password.value ?? '';
    if (this.formGroup.valid) {
      let user: UserCreate = {
        name: name,
        email: email,
        username: username,
        dni: dni,
        gender: gender,
        password: password
      };
      switch (this.mode) {
        case 'create': {
          this.usersService.addUser(user).subscribe(() => {
            this.router.navigate(['/users']);
          });
          break;
        }
        case 'update': {
          this.usersService.updateUser(user, id).subscribe(() => {
              this.router.navigate(['/users']);
            }
          );
          break;
        }
        case 'register': {
          this.authService.register(name, email, username, dni, gender, password).subscribe((data: Token) => {
              localStorage.setItem('token', data.token);
              this.router.navigate(['/users']);
            }
          );
          break;
        }
        default: {
          this.router.navigate(['/users']);
        }
      }
    } else {
      this.formGroup.markAllAsTouched();
      this.messageService.add({severity: 'error', summary: 'Error', detail: 'Hay errores en el formulario'});
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

  isValid() {
    return this.formGroup.valid;
  }

  goBack() {
    this.location.back();
  }

  getButtonLabel() {
    switch (this.mode) {
      case 'create':
        return 'Crear usuario';
      case 'update':
        return 'Actualizar usuario';
      case 'register':
        return 'Crear usuario';
      default:
        return 'Siguiente';
    }
  }

  getTittle() {
    switch (this.mode) {
      case 'create':
        return 'Crear usuario';
      case 'update':
        return 'Actualizar usuario';
      case 'register':
        return 'Registro';
      default:
        return 'Usuario';
    }
  }
}
