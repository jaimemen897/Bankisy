import {Pipe, PipeTransform} from '@angular/core';

@Pipe({
  standalone: true,
  name: 'ibanFormat'
})
export class IbanFormatPipe implements PipeTransform {

  transform(value: string): string {
    let formattedIban = '';
    for (let i = 0; i < value.length; i++) {
      if (i % 4 === 0 && i !== 0) {
        formattedIban += ' ';
      }
      formattedIban += value[i];
    }
    return formattedIban;
  }

}
