import {Component} from '@angular/core';
import {DepositService} from "../services/deposit.service";
import {InputTextModule} from "primeng/inputtext";
import {FormsModule} from "@angular/forms";
import {ButtonModule} from "primeng/button";

@Component({
  selector: 'app-deposit',
  standalone: true,
  imports: [
    InputTextModule,
    FormsModule,
    ButtonModule
  ],
  templateUrl: './deposit.component.html',
  styleUrl: './deposit.component.css'
})
export class DepositComponent {

  constructor(private depositService: DepositService) {
  }

  amount: number;

  deposit() {
    const depositCreate = {
      Source: 'tok_visa',
      Amount: this.amount
    };
    this.depositService.deposit(depositCreate).subscribe(
      () => {
        console.log('Deposit successful');
      }
    );
  }
}
