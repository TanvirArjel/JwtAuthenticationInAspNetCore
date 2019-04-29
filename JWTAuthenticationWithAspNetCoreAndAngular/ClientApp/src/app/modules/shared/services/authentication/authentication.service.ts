import { Injectable } from '@angular/core';
import { JsonWebToken } from '../../../authentication/models/json-web-token';

@Injectable({
  providedIn: 'root'
})
export class AuthenticationService {

  constructor() { }

  getLoggedInUser(): JsonWebToken {
    return JSON.parse(localStorage.getItem('loggedInUser')) || JSON.parse(sessionStorage.getItem('loggedInUser'));
  }

  getAccessToken(): string {
    const loggedInUser = JSON.parse(localStorage.getItem('loggedInUser')) || JSON.parse(sessionStorage.getItem('loggedInUser'));
    const accessToken = loggedInUser != null ? loggedInUser.accessToken : null;
    return accessToken;
  }

  isUserLoggedIn(): boolean {
    const loggedInUser = localStorage.getItem('loggedInUser') || sessionStorage.getItem('loggedInUser');
    return !!loggedInUser;
  }
}
