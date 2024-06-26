import {Injectable} from '@angular/core';
import * as signalR from '@microsoft/signalr';
import {MessageService} from "primeng/api";
import {environment} from "../../environments/environment";
import {BehaviorSubject} from "rxjs";
import {User} from "../models/User";

@Injectable({
  providedIn: 'root'
})
export class SocketService {
  private apiUrl = `${environment.apiUrl}/myHub`

  private eventSubject = new BehaviorSubject<User>(new User());
  eventEmit$ = this.eventSubject.asObservable();

  constructor(private messageService: MessageService) {
  }

  private hubConnection: signalR.HubConnection;

  public startConnection = () => {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.apiUrl, {accessTokenFactory: () => this.getAccessToken()})
      .build();

    this.hubConnection
      .start()
      .then(() => console.log('Connection success, waiting for messages'))
      .catch((err: any) => console.log('Error while starting connection: ' + err));
  }

  private getAccessToken = (): string => {
    return localStorage?.getItem('token') ?? '';
  }

  public transferNotification = () => {
    this.hubConnection.on('TransferReceived', (user: string, message: string) => {
      this.eventSubject.next(new User());
      this.messageService.add({
        severity: 'info',
        summary: 'Transferencia recibida',
        detail: message,
        life: 2000,
        closable: false
      });
    });
  }
}
