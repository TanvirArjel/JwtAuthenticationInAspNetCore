import { Directive, Self } from '@angular/core';
import { NgControl } from '@angular/forms';

@Directive({
  selector: '[formControlName],[ngModel],[formControl]',
  host: {
    '[class.is-valid]': 'ngClassValid',
    '[class.is-invalid]': 'ngClassInvalid'
  }
})
export class BsControlStatusDirective {

  public constructor(@Self() private control: NgControl) {}

  get ngClassValid(): boolean {
    if (this.control.control == null || this.control.control.untouched) {
      return false;
    }
    return this.control.control.valid;
  }

  get ngClassInvalid(): boolean {
    if (this.control.control == null || this.control.control.untouched) {
      return false;
    }
    return this.control.control.invalid;
  }
}
