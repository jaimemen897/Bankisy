import {Component, Inject, OnInit} from '@angular/core';
import {Message, MessageService} from "primeng/api";
import {slideInAnimation} from "./route-animations";
import {SocketService} from "./services/socket.service";
import {DOCUMENT} from "@angular/common";


@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  animations: [slideInAnimation]
})
export class AppComponent implements OnInit {
  title = 'app';
  messages1: Message[] = [];
  themeSelection: boolean = false;

  constructor(@Inject(DOCUMENT) private document: Document, private messageService: MessageService, private socketService: SocketService) {
    let theme = window.localStorage.getItem('theme');
    if (theme){
      this.themeSelection = theme == 'dark';
      this.changeTheme(this.themeSelection);
    }
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

  changeTheme(state: boolean) {
    let theme = state ? 'dark' : 'light';
    window.localStorage.setItem('theme', theme);
    let themeLink = this.document.getElementById('app-theme') as HTMLLinkElement;
    themeLink.href = 'aura-' + theme + '-blue.css';
  }
}
