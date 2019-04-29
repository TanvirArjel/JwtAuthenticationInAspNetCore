import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators, FormGroup, FormControl } from '@angular/forms';
import { LoginService } from '../../services/login/login.service';
import { Router } from '@angular/router';
import { NavMenuService } from '../../../shared/services/nav-menu-service/nav-menu.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  serverErrorMessage: string;
  loginFrom: FormGroup;

  constructor(private formbuilder: FormBuilder, private router: Router,
    private loginService: LoginService, private navMenuService: NavMenuService) { }

  ngOnInit() {
    this.loginFrom = this.formbuilder.group({
      userName: ['', [Validators.required, Validators.minLength(4), Validators.maxLength(20)]],
      password: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(20)]],
      rememberMe: [false]
    });
  }

  getAccessToken(): void {
    if (this.loginFrom.valid) {
      this.loginService.getAccessToken(this.loginFrom.value).subscribe((jsonWebToken) => {
        if (jsonWebToken != null) {
          if (this.loginFrom.get('rememberMe').value) {
            localStorage.setItem('loggedInUser', JSON.stringify(jsonWebToken))
          } else {
            sessionStorage.setItem('loggedInUser', JSON.stringify(jsonWebToken))
          }

          this.navMenuService.setUserName(jsonWebToken.userName);
          this.router.navigate(['/fetch-data']);
        } else {
          this.serverErrorMessage = "There is some problme";
        }
      }, (error) => {
          if (error.status === 400) {
            this.serverErrorMessage = "Either user name or password is invalid!";
          } else {
            this.serverErrorMessage = "There is some problem with the service.Please try again.If the problem persits please contract with system administrator";
          }
       
      });
    } else {
      this.validateAllFormFields(this.loginFrom);
    }
  }

  validateAllFormFields(formGroup: FormGroup) {
    Object.keys(formGroup.controls).forEach(field => {
      const control = formGroup.get(field);
      if (control instanceof FormControl) {
        control.markAsTouched({ onlySelf: true });
      } else if (control instanceof FormGroup) {
        this.validateAllFormFields(control);
      }
    });
  }

}
