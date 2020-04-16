import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { AddHeaderInterceptor } from './http-interceptors/add-header-interceptor';
import { BsControlStatusDirective } from './directives/bscontrol-status.directive';

@NgModule({
  declarations: [BsControlStatusDirective],
  imports: [
    CommonModule
  ],
  providers: [{
    provide: HTTP_INTERCEPTORS,
    useClass: AddHeaderInterceptor,
    multi: true
  }],
  exports : [BsControlStatusDirective]
})
export class SharedModule { }
