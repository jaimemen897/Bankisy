import {NgModule} from '@angular/core';
import {RouterModule, Routes} from '@angular/router';
import {UsersComponent} from "../users/users.component";
import {LoginComponent} from "../login/login.component";
import {RegisterComponent} from "../register/register.component";
import {BankaccountsComponent} from "../bankaccounts/bankaccounts.component";
import {TransactionsComponent} from "../transactions/transactions.component";
import {IndexComponent} from "../index/index.component";
import {CardComponent} from "../cards/cards.component";
import {CardPanelComponent} from "../card-panel/card-panel.component";
import {ProfileComponent} from "../profile/profile.component";

export const routes: Routes = [
  {path: '', redirectTo: '/index', pathMatch: 'full'},
  {path: 'index', component: IndexComponent},
  {path: 'users', component: UsersComponent},
  {path: 'login', component: LoginComponent},
  {path: 'register', component: RegisterComponent},
  {path: 'bankaccounts', component: BankaccountsComponent},
  {path: 'transactions', component: TransactionsComponent},
  {path: 'cards', component: CardComponent},
  {path: 'card-panel', component: CardPanelComponent},
  {path: 'profile', component: ProfileComponent}
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {
}
