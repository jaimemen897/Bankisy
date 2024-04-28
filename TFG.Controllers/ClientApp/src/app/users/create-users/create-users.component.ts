import {Component, EventEmitter, Output} from '@angular/core';
import {UserService} from "../../services/users.service";
import {MessageService} from "primeng/api";
import {FormControl, FormGroup, ReactiveFormsModule, Validators} from "@angular/forms";
import {Gender} from "../../models/Gender";
import {UserCreate} from "../../models/UserCreate";
import {ButtonModule} from "primeng/button";
import {DropdownModule} from "primeng/dropdown";
import {MultiSelectModule} from "primeng/multiselect";
import {PaginatorModule} from "primeng/paginator";
import {InputTextModule} from "primeng/inputtext";
import {NgIf} from "@angular/common";
import {PasswordModule} from "primeng/password";
import {passwordMatchValidator} from "../../register/passwordMatchValidator";

@Component({
  selector: 'app-create-users',
  standalone: true,
  imports: [
    ButtonModule,
    DropdownModule,
    MultiSelectModule,
    PaginatorModule,
    ReactiveFormsModule,
    InputTextModule,
    NgIf,
    PasswordModule
  ],
  templateUrl: './create-users.component.html',
  styleUrl: './create-users.component.css'
})
export class CreateUsersComponent {
  constructor(private userService: UserService, private messageService: MessageService) {
  }

  formGroup: FormGroup = new FormGroup({
    name: new FormControl('', [Validators.required, Validators.minLength(3), Validators.maxLength(50)]),
    email: new FormControl('', [Validators.required, Validators.email]),
    username: new FormControl('', [Validators.required, Validators.minLength(3), Validators.maxLength(50)]),
    dni: new FormControl('', [Validators.required, Validators.minLength(9), Validators.maxLength(9)]),
    phone: new FormControl('', [Validators.required, Validators.minLength(9), Validators.maxLength(9)]),
    gender: new FormControl('', [Validators.required]),
    password: new FormControl('', [Validators.required, Validators.minLength(3), Validators.maxLength(50)]),
    confirmpassword: new FormControl('', [Validators.required, Validators.minLength(3), Validators.maxLength(50)])
  }, {validators: passwordMatchValidator});


  isUpdateMode: boolean = false;
  id!: string;

  genders: string[] = [Gender.Male, Gender.Female, Gender.Other, Gender.PreferNotToSay];
  label: string = 'Crear';

  @Output() onSave: EventEmitter<any> = new EventEmitter();
  @Output() onCancel: EventEmitter<any> = new EventEmitter();

  //USERS CREATE AN ACCOUNT
  updateUser(userId: string) {
    this.isUpdateMode = true;
    this.id = userId;
    this.label = 'Actualizar';
    this.formGroup.controls.password.clearValidators();
    this.formGroup.controls.confirmpassword.clearValidators();
    this.formGroup.controls.password.setValidators([Validators.minLength(3), Validators.maxLength(50)]);
    this.formGroup.controls.confirmpassword.setValidators([Validators.minLength(3), Validators.maxLength(50)]);
    this.formGroup.controls.password.updateValueAndValidity();
    this.formGroup.controls.confirmpassword.updateValueAndValidity();

    this.userService.getUserById(userId).subscribe(user => {
      this.formGroup.controls.name.setValue(user.name);
      this.formGroup.controls.email.setValue(user.email);
      this.formGroup.controls.username.setValue(user.username);
      this.formGroup.controls.dni.setValue(user.dni);
      this.formGroup.controls.phone.setValue(user.phone);
      let genderTranslated = Gender[user.gender as keyof typeof Gender];
      this.formGroup.controls.gender.setValue(genderTranslated);
    });
  }

  saveChanges() {
    if (!this.formGroup.valid) {
      this.messageService.add({severity: 'error', summary: 'Error', detail: 'Por favor, rellene todos los campos', life: 2000, closable: false});
      return;
    }
    //TRANSLATE GENDER
    let genderTranslated = Object.keys(Gender).find(key => Gender[key as keyof typeof Gender] === this.formGroup.value.gender) as keyof typeof Gender;

    let userCreate = new UserCreate();
    userCreate.name = this.formGroup.controls.name.value;
    userCreate.email = this.formGroup.controls.email.value;
    userCreate.username = this.formGroup.controls.username.value;
    userCreate.dni = this.formGroup.controls.dni.value;
    userCreate.phone = this.formGroup.controls.phone.value;
    userCreate.gender = genderTranslated;
    userCreate.password = this.formGroup.controls.password.value;

    //UPDATE
    if (this.isUpdateMode) {
      this.userService.updateUser(userCreate, this.id).subscribe(() => {
        this.messageService.add({
          severity: 'success',
          summary: 'Usuario actualizado',
          detail: 'Usuario actualizado'
        });
        this.formGroup.reset();
        this.onSave.emit();
      });

      //CREATE
    } else {
      this.userService.addUser(userCreate).subscribe(() => {
        this.messageService.add({
          severity: 'success',
          summary: 'Usuario creado',
          detail: 'Usuario creado'
        });
        this.formGroup.reset();
        this.onSave.emit();
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

  cancel() {
    this.formGroup.reset();
    this.onCancel.emit();
  }
}
