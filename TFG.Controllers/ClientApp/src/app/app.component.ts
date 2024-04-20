import {Component, OnInit} from '@angular/core';
import {Message, MessageService} from "primeng/api";
import {slideInAnimation} from "./route-animations";

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  animations: [slideInAnimation]
})
export class AppComponent implements OnInit {
  title = 'app';
  messages1: Message[] = [];

  constructor(private messageService: MessageService) {
  }

  prepareRoute(outlet: any) {
    return outlet && outlet.activatedRouteData && outlet.activatedRouteData['animation'];
  }

  ngOnInit() {
    this.messageService.messageObserver.subscribe(message => {
      this.messages1 = this.messages1.concat(message);
    });
  }

  isInLoginOrRegister() {
    return window.location.href.includes('login') || window.location.href.includes('register');
  }
}
