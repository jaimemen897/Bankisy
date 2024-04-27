import {Component, OnInit, ViewChild} from '@angular/core';
import {Card} from "../models/Card";
import {MessageService} from "primeng/api";
import {CardModule} from "primeng/card";
import {RouterOutlet} from "@angular/router";
import {ToastModule} from "primeng/toast";
import {IndexService} from "../services/index.service";
import {ButtonModule} from "primeng/button";
import {BlockUIModule} from "primeng/blockui";
import {RippleModule} from "primeng/ripple";
import {DatePipe, NgStyle} from "@angular/common";
import {AvatarModule} from "primeng/avatar";
import {MenuModule} from "primeng/menu";
import {CardType} from "../models/CardType";
import {ScrollPanelModule} from "primeng/scrollpanel";
import {TableModule} from "primeng/table";
import {OverlayPanelModule} from "primeng/overlaypanel";
import {ProgressSpinnerModule} from "primeng/progressspinner";

@Component({
  selector: 'app-card-panel',
  standalone: true,
  imports: [
    CardModule,
    RouterOutlet,
    ToastModule,
    ButtonModule,
    BlockUIModule,
    RippleModule,
    NgStyle,
    AvatarModule,
    MenuModule,
    ScrollPanelModule,
    TableModule,
    OverlayPanelModule,
    DatePipe,
    ProgressSpinnerModule
  ],
  templateUrl: './card-panel.component.html',
  styleUrl: './card-panel.component.css'
})
export class CardPanelComponent implements OnInit {

  constructor(private messageService: MessageService, private indexService: IndexService) {
  }

  @ViewChild('optionMenu') optionMenu: any;
  cards!: Card[]
  userId!: number
  optionItems!: any[];

  ngOnInit() {
    this.indexService.getCardsByUserId().subscribe(cards => {
      this.cards = cards;
    });
  }

  showPin(card: Card) {
    this.messageService.add({severity: 'info', summary: 'PIN', detail: card.pin});
  }

  showCvv(card: Card) {
    this.messageService.add({severity: 'info', summary: 'CVV', detail: card.cvv});
  }

  blockCard(card: Card) {
    this.indexService.blockCard(card.cardNumber).subscribe(() => {
      this.messageService.add({
        severity: 'info',
        summary: 'Tarjeta bloqueada',
        detail: 'Tarjeta bloqueada correctamente'
      });
      this.refresh();
    });
  }

  unblockCard(card: Card) {
    this.indexService.unblockCard(card.cardNumber).subscribe(() => {
      this.messageService.add({
        severity: 'info',
        summary: 'Tarjeta desbloqueada',
        detail: 'Tarjeta desbloqueada correctamente'
      });
      this.refresh();
    });
  }

  showOptions(event: any, card: Card) {
    this.optionMenu.toggle(event);
    this.optionItems = [
      {
        label: 'Opciones',
        items: [
          {
            label: 'Ver PIN',
            icon: 'pi pi-eye',
            command: () => {
              this.showPin(card);
            }
          },
          {
            label: 'Ver CVV',
            icon: 'pi pi-eye',
            command: () => {
              this.showCvv(card);
            }
          },
          {
            label: 'Bloquear tarjeta',
            icon: 'pi pi-lock',
            visible: !card.isBlocked,
            command: () => {
              this.blockCard(card);
            }
          },
          {
            label: 'Desbloquear tarjeta',
            icon: 'pi pi-unlock',
            visible: card.isBlocked,
            command: () => {
              this.unblockCard(card);
            }
          }
        ]
      },
    ];
  }

  refresh() {
    this.indexService.getCardsByUserId().subscribe(cards => {
      this.cards = cards;
    });
  }

  getCardTypeValue(cardType: string): string {
    return CardType[cardType as keyof typeof CardType];
  }

  getBackgroundColor(card?: Card) {
    if (!card) {
      return {backgroundColor: "#4b4b4b", color: "white"};
    }
    switch (card.cardType) {
      case "Visa":
        return {backgroundColor: "#0095ff", color: "#ffffff"}; // Azul
      case "Mastercard":
        return {backgroundColor: "#bf00ff", color: "#ffffff"}; // Morado
      case "AmericanExpress":
        return {backgroundColor: "#f63737", color: "#ffffff"}; // Rojo
      case "Debit":
        return {backgroundColor: "#31b05f", color: "#ffffff"}; // Verde
      case "Credit":
        return {backgroundColor: "#e76916", color: "#ffffff"}; // Naranja
      case "Prepaid":
        return {backgroundColor: "#ffff41", color: "#000000"}; // Amarillo
      case "Virtual":
        return {backgroundColor: "#65ffff", color: "#000000"}; // Cyan
      default:
        return {backgroundColor: "#414141", color: "#ffffff"}; //Negro
    }
  }

  protected readonly Card = Card;
}
