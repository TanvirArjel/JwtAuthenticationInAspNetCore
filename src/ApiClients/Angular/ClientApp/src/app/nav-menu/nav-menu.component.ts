import { Component, NgZone } from '@angular/core';
import { Router } from '@angular/router';
import { JsonWebToken } from '../modules/authentication/models/json-web-token';
import { NavMenuService } from '../modules/shared/services/nav-menu-service/nav-menu.service';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent {
  isExpanded = false;
  loggedInUserName: string;

  constructor(private router: Router, private navMenuService : NavMenuService) { }

  ngOnInit() {

    let loggedInUser = JSON.parse(localStorage.getItem('loggedInUser')) || JSON.parse(sessionStorage.getItem('loggedInUser'))
    this.loggedInUserName = loggedInUser != null ? loggedInUser.userName : null;

    this.navMenuService.loggedInUserName.subscribe(userName => {
      this.loggedInUserName = userName
    }, err => {
      console.log(err);
    });
    
  }


  logOut(): void {
    localStorage.removeItem('loggedInUser');
    sessionStorage.removeItem('loggedInUser');
    this.loggedInUserName = null;
    this.router.navigate(['/user/login']);
  }

  collapse() {
    this.isExpanded = false;
  }

  toggle() {
    this.isExpanded = !this.isExpanded;
  }
}
