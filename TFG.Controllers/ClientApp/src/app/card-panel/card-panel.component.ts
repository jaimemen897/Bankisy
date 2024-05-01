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
import {DialogModule} from "primeng/dialog";
import {InputOtpModule} from "primeng/inputotp";
import {InputTextModule} from "primeng/inputtext";
import {FormsModule, ReactiveFormsModule} from "@angular/forms";
import {CardCreate} from "../models/CardCreate";
import {DropdownModule} from "primeng/dropdown";
import {BankAccount} from "../models/BankAccount";
import {CreateCardComponent} from "../cards/create-card/create-card.component";
import {BizumCreateComponent} from "../transactions/bizum-create/bizum-create.component";

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
    ProgressSpinnerModule,
    DialogModule,
    InputOtpModule,
    InputTextModule,
    ReactiveFormsModule,
    FormsModule,
    DropdownModule,
    CreateCardComponent
  ],
  templateUrl: './card-panel.component.html',
  styleUrl: './card-panel.component.css'
})
export class CardPanelComponent implements OnInit {

  constructor(private messageService: MessageService, private indexService: IndexService) {
  }

  @ViewChild('optionMenu') optionMenu: any;
  @ViewChild(CreateCardComponent) createCardComponent!: CreateCardComponent;
  cards!: Card[]
  userId!: number
  optionItems!: any[];

  updatePinDialogVisible: boolean = false;
  cardToUpdate!: Card;
  pinToUpdate!: string;
  showNewCardDialog: boolean = false;

  ngOnInit() {
    this.indexService.getCardsByUserId().subscribe(cards => {
      this.cards = cards;
    });
  }

  showPin(card: Card) {
    this.messageService.add({severity: 'info', summary: 'PIN', detail: card.pin, closable: false, life: 2000,});
  }

  showCvv(card: Card) {
    this.messageService.add({severity: 'info', summary: 'CVV', detail: card.cvv, closable: false, life: 2000,});
  }

  blockCard(card: Card) {
    this.indexService.blockCard(card.cardNumber).subscribe(() => {
      this.messageService.add({
        severity: 'info',
        summary: 'Tarjeta bloqueada',
        detail: 'Tarjeta bloqueada correctamente',
        closable: false, life: 2000,
      });
      this.refresh();
    });
  }

  unblockCard(card: Card) {
    this.indexService.unblockCard(card.cardNumber).subscribe(() => {
      this.messageService.add({
        severity: 'info',
        summary: 'Tarjeta desbloqueada',
        detail: 'Tarjeta desbloqueada correctamente',
        closable: false, life: 2000,
      });
      this.refresh();
    });
  }

  renovateCard(card: Card) {
    this.indexService.renovateCard(card.cardNumber).subscribe(() => {
      this.messageService.add({
        severity: 'info',
        summary: 'Tarjeta renovada',
        detail: 'Tarjeta renovada correctamente',
        closable: false, life: 2000,
      });
      this.refresh();
    });
  }

  showUpdatePinDialog(card: Card) {
    this.cardToUpdate = card;
    this.updatePinDialogVisible = true;
  }

  updatePin() {
    let cardUpdate: CardCreate = {
      pin: this.pinToUpdate,
      cardType: this.cardToUpdate.cardType,
      userId: this.cardToUpdate.user.id,
      bankAccountIban: this.cardToUpdate.bankAccount.iban
    };
    this.indexService.updateCard(this.cardToUpdate.cardNumber, cardUpdate).subscribe(() => {
      this.messageService.add({
        severity: 'info',
        summary: 'PIN actualizado',
        detail: 'PIN actualizado correctamente',
        closable: false, life: 2000,
      });
      this.updatePinDialogVisible = false;
      this.refresh();
      this.pinToUpdate = '';
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
            label: 'Renovar tarjeta',
            icon: 'pi pi-refresh',
            command: () => {
              this.renovateCard(card);
            }
          },
          {
            label: 'Cambiar PIN',
            icon: 'pi pi-key',
            command: () => {
              this.showUpdatePinDialog(card)
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

  showNewCardDialogMethod() {
    this.indexService.getUserByToken().subscribe(user => {
      this.createCardComponent.loadUser(user)
      this.showNewCardDialog = true
    })
  }

  createCard() {
    this.showNewCardDialog = false
    this.messageService.add({
      severity: 'success',
      summary: 'Tarjeta solicitada',
      detail: 'La tarjeta se ha solicitado correctamente',
      life: 2000,
      closable: false
    });
    this.refresh()
  }

  protected readonly Card = Card;
}
