import {NgModule} from '@angular/core';
import {RouterModule, Routes} from '@angular/router';
import {UsersComponent} from "../users/users.component";
import {LoginComponent} from "../login/login.component";
import {RegisterComponent} from "../register/register.component";
import {BankaccountsComponent} from "../bankaccounts/bankaccounts.component";
import {TransactionsComponent} from "../transactions/transactions.component";
import {BankaccountCreateComponent} from "../bankaccounts/bankaccount-create/bankaccount-create.component";
import {IndexComponent} from "../index/index.component";

export const routes: Routes = [
  {path: '', redirectTo: '/index', pathMatch: 'full'},
  {path: 'index', component: IndexComponent},
  {path: 'users', component: UsersComponent},
  {path: 'login', component: LoginComponent},
  {path: 'register/:mode', component: RegisterComponent},
  {path: 'register/:id/:mode', component: RegisterComponent},
  {path: 'bankaccounts', component: BankaccountsComponent},
  {path: 'bankaccounts/:mode/:id', component: BankaccountCreateComponent},
  {path: 'transactions', component: TransactionsComponent},
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {
}
