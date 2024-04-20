import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {io} from "socket.io-client";

@Injectable({
  providedIn: 'root'
})
export class SocketService {
  private socket: any;

  /*constructor() {
    this.socket = io('http://localhost:5196/transactions');
  }

  listenForTransactions(): Observable<any> {
    return new Observable(observer => {
      this.socket.on('transaction', (data: any) => {
        observer.next(data);
      });
    });
  }*/
}
