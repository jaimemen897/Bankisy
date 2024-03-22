import {BrowserModule} from '@angular/platform-browser';
import {NgModule, OnInit} from '@angular/core';
import {FormsModule} from '@angular/forms';
import {HTTP_INTERCEPTORS, HttpClientModule} from '@angular/common/http';
import {provideRouter, RouterModule, withComponentInputBinding} from '@angular/router';
import {AppComponent} from './app.component';
import {ButtonModule} from "primeng/button";
import {ConfirmationService, FilterMatchMode, PrimeNGConfig} from 'primeng/api';
import {UsersComponent} from "./users/users.component";
import {MessageService} from 'primeng/api';
import {routes} from "./Redirections/app.routing";
import {BrowserAnimationsModule} from '@angular/platform-browser/animations';
import {MessagesModule} from "primeng/messages";
import {LoginComponent} from "./login/login.component";
import {AuthInterceptor} from "./interceptors/auth.interceptor";
import {UserService} from "./services/users.service";
import {ErrorHttpInterceptor} from "./interceptors/error-http.interceptor";

@NgModule({
  declarations: [
    AppComponent,
  ],
    imports: [
        BrowserModule,
        HttpClientModule,
        FormsModule,
        RouterModule.forRoot([]),
        ButtonModule,
        UsersComponent,
        LoginComponent,
        BrowserAnimationsModule,
        MessagesModule
    ],
  providers: [MessageService,
    {provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true},
    {provide: HTTP_INTERCEPTORS, useClass: ErrorHttpInterceptor, multi: true},
    provideRouter(routes, withComponentInputBinding()),
    UserService, ConfirmationService, MessageService
  ],
  bootstrap: [AppComponent]
})
export class AppModule implements OnInit {
  constructor(private primengConfig: PrimeNGConfig) {
  }

  ngOnInit() {
    this.primengConfig.ripple = true;
    this.primengConfig.zIndex = {
      modal: 1100,
      overlay: 1000,
      menu: 1000,
      tooltip: 1100
    };
    this.primengConfig.filterMatchModeOptions = {
      text: [FilterMatchMode.STARTS_WITH, FilterMatchMode.CONTAINS, FilterMatchMode.NOT_CONTAINS, FilterMatchMode.ENDS_WITH, FilterMatchMode.EQUALS, FilterMatchMode.NOT_EQUALS],
      numeric: [FilterMatchMode.EQUALS, FilterMatchMode.NOT_EQUALS, FilterMatchMode.LESS_THAN, FilterMatchMode.LESS_THAN_OR_EQUAL_TO, FilterMatchMode.GREATER_THAN, FilterMatchMode.GREATER_THAN_OR_EQUAL_TO],
      date: [FilterMatchMode.DATE_IS, FilterMatchMode.DATE_IS_NOT, FilterMatchMode.DATE_BEFORE, FilterMatchMode.DATE_AFTER]
    };
  }
}
