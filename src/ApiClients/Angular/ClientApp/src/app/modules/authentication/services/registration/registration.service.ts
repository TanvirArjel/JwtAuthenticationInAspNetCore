import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { throwError as observableThrowError, Observable } from 'rxjs';
import { RegisterModel } from '../../models/register-model';
import { JsonWebToken } from '../../models/json-web-token';
import { catchError } from 'rxjs/operators';
import { AppSettings } from '../../../../app-settings';

@Injectable({
  providedIn: 'root'
})
export class RegistrationService {
  private baseUrl = AppSettings.ApiHostUrl + '/api/authentication/';
  constructor(private httpClient: HttpClient) { }

  register(registerModel: RegisterModel): Observable<JsonWebToken> {
    const requestBody = JSON.stringify(registerModel);
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    return this.httpClient.post<JsonWebToken>(this.baseUrl + "register", requestBody, { headers: headers })
      .pipe(catchError(this.handleError.bind(this)));
  }

  isEmailAlreadyExists(email: string): Observable<boolean> {
    const params = new HttpParams({
      fromObject: {
        email: email
      }
    });
    return this.httpClient.get<boolean>(this.baseUrl + "IsEmailAlreadyExists", { params: params }).pipe(catchError(this.handleError.bind(this)));
  }

  isUserNameAlreadyExists(userName: string): Observable<boolean> {
    const params = new HttpParams({
      fromObject: {
        userName: userName
      }
    });
    return this.httpClient.get<boolean>(this.baseUrl + "IsUserNameAlreadyExists", { params: params }).pipe(catchError(this.handleError.bind(this)));
  }

  handleError(error: HttpErrorResponse) {
    console.error();
    return observableThrowError(error);
  }
}
