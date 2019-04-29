import { Injectable } from '@angular/core';
import { HttpEvent, HttpInterceptor, HttpHandler, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthenticationService } from '../services/authentication/authentication.service';

@Injectable()
export class AddHeaderInterceptor implements HttpInterceptor {

  constructor(private authenticationService: AuthenticationService) { }

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

    let accessToken = this.authenticationService.getAccessToken();
    
    // Clone the request to add the new header
    const clonedRequest = request.clone({ headers: request.headers.set('Authorization', `Bearer ${accessToken}`) });

    // Pass the cloned request instead of the original request to the next handle
    return next.handle(clonedRequest);

  }

}
