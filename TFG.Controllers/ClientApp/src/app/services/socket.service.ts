import {Injectable} from '@angular/core';
import * as signalR from '@microsoft/signalr';
import {MessageService} from "primeng/api";

@Injectable({
  providedIn: 'root'
})
export class SocketService {
  constructor(private messageService: MessageService) {
  }

  private hubConnection: signalR.HubConnection;

  public startConnection = () => {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:5196/myHub')
      .build();

    this.hubConnection
      .start()
      .then(() => console.log('Connection success, waiting for messages'))
      .catch(err => console.log('Error while starting connection: ' + err));
  }

  public transferNotification = () => {
    this.hubConnection.on('ReceiveMessage', (user, message) => {
      this.messageService.add({severity: 'info', summary: user, detail: message, life: 2000, closable: false});
    });
  }
}
