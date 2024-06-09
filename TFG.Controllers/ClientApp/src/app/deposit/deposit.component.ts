import {Component, OnInit} from '@angular/core';
import {InputTextModule} from "primeng/inputtext";
import {FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators} from "@angular/forms";
import {ButtonModule} from "primeng/button";
import {CheckoutService} from "../services/checkout.service";
import {loadStripe} from "@stripe/stripe-js";
import {SliderModule} from "primeng/slider";
import {ToastModule} from "primeng/toast";
import {BankAccount} from "../models/BankAccount";
import {DropdownModule} from "primeng/dropdown";
import {MessageService} from "primeng/api";

import {BankAccountService} from "../services/bankaccounts.service";
import {IbanFormatPipe} from "../pipes/IbanFormatPipe";

@Component({
  selector: 'app-deposit',
  standalone: true,
  imports: [
    InputTextModule,
    FormsModule,
    ButtonModule,
    SliderModule,
    ToastModule,
    DropdownModule,
    ReactiveFormsModule,
    IbanFormatPipe
  ],
  templateUrl: './deposit.component.html',
  styleUrl: './deposit.component.css'
})
export class DepositComponent implements OnInit {
  sessionID: string;
  bankAccounts: BankAccount[] = [];

  constructor(private checkoutService: CheckoutService, private messageService: MessageService, private bankAccountService: BankAccountService) {
  }

  formGroup: FormGroup = new FormGroup({
    amount: new FormControl(0, [Validators.required, Validators.min(0.01), Validators.max(10000)]),
    selectedBankAccount: new FormControl(undefined, [Validators.required]),
    slider: new FormControl(0)
  });

  ngOnInit() {
    this.bankAccountService.getBankAccountsByMySelf().subscribe(bankAccounts => {
      this.bankAccounts = bankAccounts;
    });
  }

  async createCheckoutSession() {
    if (this.formGroup.invalid) {
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Rellena todos los campos correctamente.',
        closable: false,
        life: 2000
      });
    } else {
      let amount = this.formGroup.controls.amount.value;
      let selectedBankAccountIban = this.formGroup.controls.selectedBankAccount.value.iban;

      this.checkoutService.createCheckoutSession(amount, selectedBankAccountIban).subscribe(response => {
        this.sessionID = response.id;
        this.redirectToCheckout();
      });
    }
  }

  async redirectToCheckout() {
    const stripe = await loadStripe('pk_test_51P7eS8D74icxIHcUPVwMabVBGZqDBTx8YBhItr2Ht61LQuBLsaBnSCls9AfxtdmAb0Ju8uweakHj8K9v7dTeCwWP00cTmOWBgn');
    if (stripe !== null) {
      const {error} = await stripe.redirectToCheckout({
        sessionId: this.sessionID
      });

      if (error) {
        console.error('Error:', error);
      }
    } else {
      console.error('Stripe failed to initialize.');
    }
  }

  setAmmountOnInput(event: any) {
    this.formGroup.controls.amount.setValue(event.value);
  }

  setAmmountOnSlider(event: any) {
    this.formGroup.controls.slider.setValue(event.target.value);
  }
}
