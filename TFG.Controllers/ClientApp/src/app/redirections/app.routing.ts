import {NgModule} from '@angular/core';
import {RouterModule, Routes} from '@angular/router';
import {UsersComponent} from "../users/users.component";
import {LoginComponent} from "../login/login.component";
import {RegisterComponent} from "../register/register.component";
import {BankaccountsComponent} from "../bankaccounts/bankaccounts.component";

export const routes: Routes = [
  {path: '', redirectTo: '/users', pathMatch: 'full'},
  {path: 'users', component: UsersComponent},
  {path: 'login', component: LoginComponent},
  {path: 'register/:mode', component: RegisterComponent},
  {path: 'register/:id/:mode', component: RegisterComponent},
  {path: 'bankaccounts', component: BankaccountsComponent},
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {
}
