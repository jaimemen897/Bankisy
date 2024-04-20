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
import {TransactionCreate} from "../models/TransactionCreate";
import {BizumCreateComponent} from "../transactions/bizum-create/bizum-create.component";
import {CreateTransactionComponent} from "../transactions/create/create-transaction.component";

export const routes: Routes = [
  {path: '', redirectTo: '/index', pathMatch: 'full', data: { animation: 'IndexPage' }},
  {path: 'index', component: IndexComponent, data: { animation: 'IndexPage' }},
  {path: 'users', component: UsersComponent, data: { animation: 'UsersPage' }},
  {path: 'login', component: LoginComponent, data: { animation: 'LoginPage' }},
  {path: 'register', component: RegisterComponent, data: { animation: 'RegisterPage' }},
  {path: 'bankaccounts', component: BankaccountsComponent, data: { animation: 'BankAccountsPage' }},
  {path: 'transactions', component: TransactionsComponent, data: { animation: 'TransactionsPage' }},
  {path: 'cards', component: CardComponent, data: { animation: 'CardsPage' }},
  {path: 'card-panel', component: CardPanelComponent, data: { animation: 'CardPanelPage' }},
  {path: 'profile', component: ProfileComponent, data: { animation: 'ProfilePage' }},
  {path: 'transactions/create', component: CreateTransactionComponent, data: { animation: 'TransactionsPage' }},
  {path: 'bizum', component: BizumCreateComponent, data: { animation: 'TransactionsPage' }},

];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {
}
