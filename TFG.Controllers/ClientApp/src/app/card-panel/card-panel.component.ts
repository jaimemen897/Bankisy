import {Component, OnInit} from '@angular/core';
import {Card} from "../models/Card";
import {CardService} from "../services/card.service";
import {MessageService} from "primeng/api";
import {CardModule} from "primeng/card";
import {RouterOutlet} from "@angular/router";
import {ToastModule} from "primeng/toast";
import {IndexService} from "../services/index.service";
import {ButtonModule} from "primeng/button";
import {BlockUIModule} from "primeng/blockui";
import {RippleModule} from "primeng/ripple";

@Component({
  selector: 'app-card-panel',
  standalone: true,
  imports: [
    CardModule,
    RouterOutlet,
    ToastModule,
    ButtonModule,
    BlockUIModule,
    RippleModule
  ],
  templateUrl: './card-panel.component.html',
  styleUrl: './card-panel.component.css'
})
export class CardPanelComponent implements OnInit{

  constructor(private cardService: CardService, private messageService: MessageService, private indexService: IndexService) {
  }

  cards!: Card[]
  userId!: number

  ngOnInit() {
    this.indexService.getCardsByUserId().subscribe(cards => {
      this.cards = cards;
    });
  }

  showCVV(card: Card) {
    this.messageService.add({severity:'info', summary:'CVV', detail: card.cvv});
  }

  blockCard(card: Card) {
    this.cardService.blockCard(card.cardNumber).subscribe(() => {
      this.messageService.add({severity:'success', summary:'Success', detail: 'Card blocked successfully'});
      this.refresh();
    });
  }

  unblockCard(card: Card) {
    this.cardService.unblockCard(card.cardNumber).subscribe(() => {
      this.messageService.add({severity:'success', summary:'Success', detail: 'Card unblocked successfully'});
      this.refresh();
    });
  }

  showOptions(card: Card) {
    this.messageService.add({severity:'info', summary:'Options', detail: card.cardType});
  }

  refresh() {
    this.indexService.getCardsByUserId().subscribe(cards => {
      this.cards = cards;
    });
  }

}
