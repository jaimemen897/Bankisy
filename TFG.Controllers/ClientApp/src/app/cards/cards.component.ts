import {Component, ViewChild} from '@angular/core';
import {ConfirmationService, MessageService} from "primeng/api";
import {Router, RouterOutlet} from "@angular/router";
import {ToastModule} from "primeng/toast";
import {TableModule} from "primeng/table";
import {MultiSelectModule} from "primeng/multiselect";
import {DropdownModule} from "primeng/dropdown";
import {TagModule} from "primeng/tag";
import {DatePipe, NgClass, NgIf} from "@angular/common";
import {InputTextModule} from "primeng/inputtext";
import {TooltipModule} from "primeng/tooltip";
import {ButtonModule} from "primeng/button";
import {FormsModule} from "@angular/forms";
import {OverlayPanelModule} from "primeng/overlaypanel";
import {ConfirmDialogModule} from "primeng/confirmdialog";
import {Card} from "../models/Card";
import {DialogModule} from "primeng/dialog";
import {CardType} from "../models/CardType";
import {Transaction} from "../models/Transaction";
import {CardService} from "../services/card.service";
import {CreateCardComponent} from "./create-card/create-card.component";

@Component({
  selector: 'app-cards',
  standalone: true,
  imports: [
    RouterOutlet,
    ToastModule,
    TableModule,
    MultiSelectModule,
    DropdownModule,
    TagModule,
    NgClass,
    InputTextModule,
    TooltipModule,
    ButtonModule,
    FormsModule,
    OverlayPanelModule,
    ConfirmDialogModule,
    DialogModule,
    DatePipe,
    NgIf,
    CreateCardComponent
  ],
  templateUrl: './cards.component.html',
  styleUrl: './cards.component.css'
})
export class CardComponent {
  constructor(private cardService: CardService, private router: Router, private confirmationService: ConfirmationService, private messageService: MessageService) {
  }

  @ViewChild(CreateCardComponent) cardCreateComponent!: CreateCardComponent;

  cards: Card[] = [];
  rows: number = 10;
  totalRecords: number = 0;
  displayDialog!: boolean;

  sortField!: string;
  sortOrder!: number;
  search: string;
  filter!: string;
  isDeleted!: boolean;
  isBlocked!: boolean;

  cardsType: string[] = [CardType.Debit, CardType.Visa, CardType.Prepaid, CardType.Virtual, CardType.Credit, CardType.Mastercard, CardType.AmericanExpress];
  status: string[] = ['Active', 'Inactive'];
  users: string[] = [];
  transactions: Transaction[] = [];
  headerSaveUpdateCard: string = 'Crear tarjeta';

  //DATA, ORDERS AND FILTERS
  lazyLoad(event: any) {
    let pageNumber = Math.floor(event.first / event.rows) + 1;
    let sortField = event.sortField;
    let sortOrder = event.sortOrder;

    if (event.filters?.isDeleted) {
      this.isDeleted = event.filters.isDeleted.value;
    }

    if (event.filters?.isBlocked) {
      this.isBlocked = event.filters.isBlocked.value;
    }

    this.cardService.getCards(pageNumber, event.rows, sortField, sortOrder === -1, this.search, this.filter, this.isDeleted, this.isBlocked).subscribe(data => {
      this.cards = data.items;
      this.totalRecords = data.totalRecords;
      for (let card of this.cards) {
        this.users.push(card.user.name);
      }
      this.users = this.users.filter((value, index) => this.users.indexOf(value) === index);

    });
  }

  //SEARCH
  onSearch(event: any) {
    this.cardService.getCards(1, this.rows, this.sortField, this.sortOrder === -1, event.target.value, this.filter, this.isDeleted, this.isBlocked).subscribe(data => {
      this.cards = data.items;
      this.totalRecords = data.totalCount;
      this.search = event.target.value;
    });
  }

  //USUARIOS
  onSearchUser(event: any) {
    this.cardService.getCards(1, this.rows, this.sortField, this.sortOrder === -1, event.value, this.filter, this.isDeleted, this.isBlocked).subscribe(data => {
      this.cards = data.items;
      this.totalRecords = data.totalCount;
      this.filter = event.value;
    });
  }

  //TIPO TARJETA
  onSearchFilter(event: any) {
    let cardTypeTranslated = Object.keys(CardType).find(key => CardType[key as keyof typeof CardType] === event.value) as keyof typeof CardType;
    this.cardService.getCards(1, this.rows, this.sortField, this.sortOrder === -1, this.search, cardTypeTranslated, this.isDeleted, this.isBlocked).subscribe(data => {
      this.cards = data.items;
      this.totalRecords = data.totalCount;
      this.filter = event.value;
    });
  }

  onSearchIsDeleted(event: any) {
    this.isDeleted = event.checked;
    this.lazyLoad({first: 1, rows: this.rows, sortField: this.sortField, sortOrder: this.sortOrder});
  }

  onSortChange(event: any) {
    let value = event.value;

    if (value.indexOf('!') === 0) {
      this.sortOrder = -1;
      this.sortField = value.substring(1, value.length);
    } else {
      this.sortOrder = 1;
      this.sortField = value;
    }
  }

  clearOrders() {
    this.cardService.getCards(1, this.rows, this.sortField, this.sortOrder === -1, this.search, this.filter, this.isDeleted, this.isBlocked).subscribe(data => {
      this.cards = data.items;
      this.totalRecords = data.totalCount;
    });

    this.sortField = '';
    this.sortOrder = 1;
  }

  clearFilters() {
    this.search = '';
    this.filter = '';
    this.isDeleted = false;

    this.cardService.getCards(1, this.rows, this.sortField, this.sortOrder === -1, this.search, this.filter, this.isDeleted, this.isBlocked).subscribe(data => {
      this.cards = data.items;
      this.totalRecords = data.totalCount;
    });
  }

  //REDIRECTIONS
  goToCreateCard() {
    this.headerSaveUpdateCard = 'Crear tarjeta';
    this.displayDialog = true;
    this.cardCreateComponent.loadUsers();
    this.refresh();
  }

  goToEditCard(cardNumber: string) {
    this.headerSaveUpdateCard = 'Actualizar tarjeta';
    this.cardCreateComponent.loadCard(cardNumber);
    this.displayDialog = true;
    this.refresh();
  }

  refresh() {
    this.lazyLoad({first: 1, rows: this.rows, sortField: this.sortField, sortOrder: this.sortOrder})
  }

  deleteCard(cardNumber: string) {
    this.confirmationService.confirm({
      header: '¿Desea eliminar la tarjeta?',
      message: 'Confirme para continuar',
      accept: () => {
        this.messageService.add({
          severity: 'info',
          summary: 'Eliminada',
          detail: 'Tarjeta eliminada correctamente',
          life: 2000,
          closable: false
        });
        this.cardService.deleteCard(cardNumber).subscribe(() => {
          this.cardService.getCards(1, this.rows, this.sortField, this.sortOrder === -1, this.search, this.filter, this.isDeleted, this.isBlocked).subscribe(data => {
            this.cards = data.items;
            this.totalRecords = data.totalCount;
          });
        });
      },
      reject: () => {
        this.messageService.add({
          severity: 'error',
          summary: 'Cancelar',
          detail: 'No se ha eliminado',
          life: 2000,
          closable: false
        });
      }
    });
  }

  goToLogin() {
    this.router.navigate(['/login']);
  }

  goToUsers() {
    this.router.navigate(['/users']);
  }

  //COLORS
  getSeverity(cardType: string) {
    if (cardType === 'Debit' || cardType === 'Débito') {
      return 'success';
    } else if (cardType === 'Visa') {
      return 'secondary';
    } else if (cardType === 'Credit' || cardType === 'Crédito') {
      return 'info';
    } else if (cardType === 'Prepaid' || cardType === 'Prepago') {
      return 'warning';
    } else if (cardType === 'Virtual') {
      return 'danger';
    } else if (cardType === 'AmericanExpress' || cardType === 'American Express') {
      return 'contrast';
    } else if (cardType === 'MasterCard') {
      return 'secondary';
    } else {
      return 'contrast';
    }
  }

  getBalanceColor(balance: number) {
    if (balance > 1000) {
      return 'success';
    } else if (balance == 0) {
      return 'warning';
    } else if (balance < 0) {
      return 'danger';
    } else {
      return 'info';
    }
  }

  getCardTypeValue(cardType: string): string {
    return CardType[cardType as keyof typeof CardType];
  }

  saveCard() {
    this.displayDialog = false;
    this.lazyLoad({first: 1, rows: this.rows, sortField: this.sortField, sortOrder: this.sortOrder})
  }

  closeDialog() {
    this.displayDialog = false;
  }

  //ACTIONS
  renovateCard(cardNumber: string) {
    this.cardService.renovateCard(cardNumber).subscribe(() => {
      this.cardService.getCards(1, this.rows, this.sortField, this.sortOrder === -1, this.search, this.filter, this.isDeleted, this.isBlocked).subscribe(data => {
        this.cards = data.items;
        this.totalRecords = data.totalCount;
      });
      this.messageService.add({
        severity: 'success',
        summary: 'Tarjeta renovada',
        detail: 'Tarjeta renovada correctamente',
        life: 2000,
        closable: false
      });
    });
  }

  blockCard(cardNumber: string) {
    this.confirmationService.confirm({
      header: '¿Desea bloquear la tarjeta?',
      message: 'Confirme para continuar',
      accept: () => {
        this.cardService.blockCard(cardNumber).subscribe(() => {
          this.cardService.getCards(1, this.rows, this.sortField, this.sortOrder === -1, this.search, this.filter, this.isDeleted, this.isBlocked).subscribe(data => {
            this.cards = data.items;
            this.totalRecords = data.totalCount;
          });
          this.messageService.add({
            severity: 'info',
            summary: 'Bloqueada',
            detail: 'Tarjeta bloqueada correctamente',
            life: 2000,
            closable: false
          });
        });
      },
      reject: () => {
        this.messageService.add({
          severity: 'error',
          summary: 'Cancelar',
          detail: 'No se ha bloqueado',
          life: 2000,
          closable: false
        });
      }
    });
  }

  unblockCard(cardNumber: string) {
    this.cardService.unblockCard(cardNumber).subscribe(() => {
      this.cardService.getCards(1, this.rows, this.sortField, this.sortOrder === -1, this.search, this.filter, this.isDeleted, this.isBlocked).subscribe(data => {
        this.cards = data.items;
        this.totalRecords = data.totalCount;
      });
      this.messageService.add({
        severity: 'info',
        summary: 'Desbloqueada',
        detail: 'Tarjeta desbloqueada correctamente',
        life: 2000,
        closable: false
      });
    });
  }

  activateCard(cardNumber: string) {
    this.cardService.activateCard(cardNumber).subscribe(() => {
      this.cardService.getCards(1, this.rows, this.sortField, this.sortOrder === -1, this.search, this.filter, this.isDeleted, this.isBlocked).subscribe(data => {
        this.cards = data.items;
        this.totalRecords = data.totalCount;
      });
      this.messageService.add({
        severity: 'success',
        summary: 'Activada',
        detail: 'Tarjeta activada correctamente',
        life: 2000,
        closable: false
      });
    });
  }

}
