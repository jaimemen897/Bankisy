import {Component, OnInit} from '@angular/core';
import {Message, MessageService} from "primeng/api";
import {slideInAnimation} from "./route-animations";
import {SocketService} from "./services/socket.service";


@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  animations: [slideInAnimation]
})
export class AppComponent implements OnInit {
  title = 'app';
  messages1: Message[] = [];

  constructor(private messageService: MessageService, private socketService: SocketService) {
  }

  ngOnInit() {
    this.messageService.messageObserver.subscribe(message => {
      this.messages1 = this.messages1.concat(message);
    });
    this.socketService.startConnection();
    this.socketService.transferNotification();
  }

  prepareRoute(outlet: any) {
    return outlet && outlet.activatedRouteData && outlet.activatedRouteData['animation'];
  }

  isInLoginOrRegister() {
    return window.location.href.includes('login') || window.location.href.includes('register');
  }
}
