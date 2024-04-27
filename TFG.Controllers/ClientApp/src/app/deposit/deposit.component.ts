import {Component} from '@angular/core';
import {InputTextModule} from "primeng/inputtext";
import {FormsModule} from "@angular/forms";
import {ButtonModule} from "primeng/button";
import {CheckoutService} from "../services/checkout.service";
import {loadStripe} from "@stripe/stripe-js";

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
  accountId: string;
  amount: number = 10;
  sessionID: string;

  constructor(private checkoutService: CheckoutService) {
  }

  async createCheckoutSession() {
    this.checkoutService.createCheckoutSession(this.amount).subscribe(response => {
      this.sessionID = response.id;
      this.redirectToCheckout();
    });
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
}
