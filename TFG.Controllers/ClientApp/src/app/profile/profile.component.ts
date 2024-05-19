import {Component, OnInit} from '@angular/core';
import {Router, RouterOutlet} from "@angular/router";
import {MessageService} from "primeng/api";
import {Gender} from "../models/Gender";
import {FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators} from "@angular/forms";
import {UserCreate} from "../models/UserCreate";
import {Location, NgIf, NgOptimizedImage} from '@angular/common';
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {DropdownModule} from "primeng/dropdown";
import {InputTextModule} from "primeng/inputtext";
import {PasswordModule} from "primeng/password";
import {ToastModule} from "primeng/toast";

import {Token} from "../login/login.component";
import {FileUploadModule} from "primeng/fileupload";
import {OverlayPanelModule} from "primeng/overlaypanel";
import {TooltipModule} from "primeng/tooltip";
import {SplitButtonModule} from "primeng/splitbutton";
import {passwordMatchValidator} from "../register/passwordMatchValidator";
import {environment} from "../../environments/environment";
import {UserService} from "../services/users.service";
import {User} from "../models/User";

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    ButtonModule,
    CardModule,
    DropdownModule,
    FormsModule,
    InputTextModule,
    NgIf,
    PasswordModule,
    ReactiveFormsModule,
    RouterOutlet,
    ToastModule,
    FileUploadModule,
    OverlayPanelModule,
    TooltipModule,
    NgOptimizedImage,
    SplitButtonModule
  ],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit {
  constructor(private messageService: MessageService, private location: Location,
              private router: Router, private userService: UserService) {
    this.user = this.userService.getUser();
  }

  apiUrl = `${environment.apiUrl}/index/avatar`
  user: User;
  genders: string[] = [Gender.Male, Gender.Female, Gender.Other, Gender.PreferNotToSay];
  avatar!: string;

  formGroup = new FormGroup({
    name: new FormControl('', [Validators.required, Validators.minLength(3), Validators.maxLength(50)]),
    email: new FormControl('', [Validators.required, Validators.email]),
    username: new FormControl('', [Validators.required, Validators.minLength(3), Validators.maxLength(50)]),
    dni: new FormControl('', [Validators.required, Validators.minLength(9), Validators.maxLength(9)]),
    phone: new FormControl('', [Validators.minLength(9), Validators.maxLength(9)]),
    gender: new FormControl('', [Validators.required]),
    password: new FormControl('', [Validators.minLength(3), Validators.maxLength(50)]),
    confirmpassword: new FormControl('', [Validators.minLength(3), Validators.maxLength(50)])
  }, {validators: passwordMatchValidator});
  items = [
    {
      label: 'Borrar', icon: 'pi pi-fw pi-trash', command: () => {
        this.defaultAvatar();
      }
    }];

  ngOnInit(): void {
    this.formGroup.controls.name.setValue(this.user.name);
    this.formGroup.controls.email.setValue(this.user.email);
    this.formGroup.controls.username.setValue(this.user.username);
    this.formGroup.controls.dni.setValue(this.user.dni);
    this.formGroup.controls.phone.setValue(this.user.phone);
    let genderTranslated = Gender[this.user.gender as keyof typeof Gender];
    this.formGroup.controls.gender.setValue(genderTranslated);
    this.avatar = this.user.avatar;
  }

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

      this.userService.updateProfile(user).subscribe((data: Token) => {
          localStorage.setItem('token', data.token);
          this.messageService.add({severity: 'success', summary: 'Éxito', detail: 'Perfil actualizado correctamente'});
          this.router.navigate(['/']);
        }
      );
    } else {
      this.formGroup.markAllAsTouched();
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Por favor, rellene todos los campos',
        life: 2000,
        closable: false
      });
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

  onUpload() {
    this.messageService.add({
      severity: 'info',
      summary: 'Subido',
      detail: 'Avatar subido',
      life: 2000,
      closable: false
    });
    this.userService.setUser()
    this.user = this.userService.getUser()
    this.avatar = this.user.avatar
  }

  defaultAvatar() {
    this.userService.deleteMyAvatar().subscribe(() => {
      this.messageService.add({
        severity: 'info',
        summary: 'Eliminado',
        detail: 'Avatar eliminado',
        life: 2000,
        closable: false
      });
      this.userService.setUser()
      this.user = this.userService.getUser()
      this.avatar = this.user.avatar
    });
  }

  goBack() {
    this.location.back();
  }
}
