import {Component, ElementRef, HostListener, OnInit} from '@angular/core';
import {Message, MessageService} from "primeng/api";

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent implements OnInit {
  title = 'app';
  messages1: Message[] = [];

  constructor(private messageService: MessageService) {
  }

  ngOnInit() {
    this.messageService.messageObserver.subscribe(message => {
      this.messages1 = this.messages1.concat(message);
    });
  }

  isInLoginOrRegister(){
    return window.location.href.includes('login') || window.location.href.includes('register');
  }
}
