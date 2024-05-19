import {Component, Inject, OnChanges, OnDestroy, OnInit} from '@angular/core';
import {Message, MessageService} from "primeng/api";
import {fadeInAnimation} from "./route-animations";
import {SocketService} from "./services/socket.service";
import {DOCUMENT} from "@angular/common";
import {UserService} from "./services/users.service";
import {User} from "./models/User";


@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  animations: [fadeInAnimation]
})
export class AppComponent implements OnInit, OnDestroy {
  title = 'app';
  messages1: Message[] = [];
  user: User | null = null;
  userSubscription: any;

  constructor(@Inject(DOCUMENT) private document: Document, private messageService: MessageService, private socketService: SocketService, private userService: UserService) {
    let theme = window.localStorage.getItem('theme');
    if (theme) {
      this.changeTheme(theme == 'dark');
      this.document.documentElement.classList.add(theme + '-theme');
    } else {
      this.changeTheme(false);
      this.document.documentElement.classList.add('light-theme');
    }
    this.userService.setUser();
  }

  ngOnInit() {
    this.userSubscription = this.userService.user$.subscribe(user => {
      this.user = user;
    });

    this.messageService.messageObserver.subscribe(message => {
      this.messages1 = this.messages1.concat(message);
    });
    this.socketService.startConnection();
    this.socketService.transferNotification();
  }

  ngOnDestroy() {
    if (this.userSubscription) {
      this.userSubscription.unsubscribe();
    }
  }

  prepareRoute(outlet: any) {
    return outlet?.activatedRouteData?.['animation'];
  }

  isInLoginOrRegister() {
    return window.location.href.includes('login') || window.location.href.includes('register');
  }

  changeTheme(state: boolean) {
    let theme = state ? 'dark' : 'light';
    window.localStorage.setItem('theme', theme);
    let themeLink = this.document.getElementById('app-theme') as HTMLLinkElement;
    this.document.documentElement.classList.remove('dark-theme', 'light-theme');
    this.document.documentElement.classList.add(theme + '-theme');
    themeLink.href = 'aura-' + theme + '-blue.css';
  }
}
