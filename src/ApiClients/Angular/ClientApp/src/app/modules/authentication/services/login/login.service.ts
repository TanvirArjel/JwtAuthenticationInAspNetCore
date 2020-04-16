import { Injectable } from '@angular/core';
import { throwError as observableThrowError, Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { LoginModel } from '../../models/login-model';
import { HttpHeaders, HttpErrorResponse, HttpClient } from '@angular/common/http';
import { JsonWebToken } from '../../models/json-web-token';
import { AppSettings } from '../../../../app-settings';

@Injectable({
  providedIn: 'root'
})
export class LoginService {

  private baseUrl = AppSettings.ApiHostUrl + '/api/authentication/';
  constructor(private httpClient: HttpClient) { }

  getAccessToken(loginModel: LoginModel): Observable<JsonWebToken> {
    const body = JSON.stringify(loginModel);
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    return this.httpClient.post<JsonWebToken>(this.baseUrl + 'login', body, {
      headers: headers
    }).pipe(catchError(this.handleError.bind(this)));
  }

  isUserLoggedIn(): boolean {
    const loggenInUser = localStorage.getItem('loggedInUser') || sessionStorage.getItem('loggedInUser');
    return !!loggenInUser;
  }

  handleError(error: HttpErrorResponse) {
    console.error();
    return observableThrowError(error);
  }
}
