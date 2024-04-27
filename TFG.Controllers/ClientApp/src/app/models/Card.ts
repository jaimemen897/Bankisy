import {BankAccount} from "./BankAccount";
import {User} from "./User";

export class Card {
  cardNumber: string;
  pin: string;
  cardType: string;
  expirationDate: Date;
  cvv: string;
  isDeleted: boolean;
  isBlocked: boolean;
  user: User;
  bankAccount: BankAccount;
}
