import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'caseInsensitive'
})
export class CaseInsensitivePipe implements PipeTransform {
  transform(value: string): string {
    return value ? value.toLowerCase() : value;
  }
}
