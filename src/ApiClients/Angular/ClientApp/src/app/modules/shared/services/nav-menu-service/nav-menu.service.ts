import { Injectable, Output, EventEmitter } from '@angular/core';
import { BehaviorSubject, Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class NavMenuService {
  private userNameSource = new Subject<string>();
  loggedInUserName = this.userNameSource.asObservable();

  constructor() { }

  setUserName(userName: string) {
    this.userNameSource.next(userName)
  }

}
