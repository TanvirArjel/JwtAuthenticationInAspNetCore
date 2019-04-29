import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormControl } from '@angular/forms';
import { RegistrationService } from '../../services/registration/registration.service';
import { Router } from '@angular/router';
import { NavMenuService } from '../../../shared/services/nav-menu-service/nav-menu.service';

@Component({
  selector: 'app-registration',
  templateUrl: './registration.component.html',
  styleUrls: ['./registration.component.css']
})
export class RegistrationComponent implements OnInit {
  serverErrorMessage: string;
  registrationFrom: FormGroup;
  constructor(private formBuilder: FormBuilder, private registratonService: RegistrationService,
    private navMenuService: NavMenuService, private router: Router) { }

  ngOnInit() {
    this.registrationFrom = this.formBuilder.group({
      email: [null, [Validators.required, Validators.email]],
      userName: [null, [Validators.required, Validators.minLength(4), Validators.maxLength(20)]],
      password: [null, [Validators.required, Validators.minLength(6), Validators.maxLength(20)]],
      confirmPassword: [null, [Validators.required]],
    }, { validators: this.confirmPasswordValidator('password', 'confirmPassword') });
  }

  confirmPasswordValidator(passwordKey: string, confirmPasswordKey: string) {
    return (group: FormGroup) => {
      let passwordInput = group.controls[passwordKey], confirmPasswordInput = group.controls[confirmPasswordKey];
      if (passwordInput.value && confirmPasswordInput.value) {
        if (passwordInput.value !== confirmPasswordInput.value) {
          return confirmPasswordInput.setErrors({ notEqual: true })
        } else {
          return confirmPasswordInput.setErrors(null);
        }
      }
    }
  }

  isEmailAlreadyExists(email: string): void {
    if (email.length > 4) {
      let emailControl = this.registrationFrom.controls['email'];

      if (!emailControl.hasError('email')) {
        this.registratonService.isEmailAlreadyExists(email).subscribe((response) => {
          if (response) {
            emailControl.setErrors({ 'duplicateEmail': true });
          } else {
            emailControl.setErrors({ 'duplicateEmail': null });
            emailControl.updateValueAndValidity();
          }
        });
      }
     
    }
  }

  isUserNameAlreadyExists(userName: string): void {
    if (userName.length >= 4) {
      let userNameControl = this.registrationFrom.controls['userName'];

      if (!userNameControl.hasError('maxlength')) {
        this.registratonService.isUserNameAlreadyExists(userName).subscribe((response) => {
          if (response) {
            userNameControl.setErrors({ 'duplicateUserName': true });
          } else {
            userNameControl.setErrors({ 'duplicateUserName': null });
            userNameControl.updateValueAndValidity();
          }
        });
      }

    }
  }

  register(): void {
    if (this.registrationFrom.valid) {
      this.registratonService.register(this.registrationFrom.value).subscribe((jsonWebToken) => {
        if (jsonWebToken != null) {
          sessionStorage.setItem('loggedInUser', JSON.stringify(jsonWebToken))
          this.navMenuService.setUserName(jsonWebToken.userName);
          this.router.navigate(['/fetch-data']);
        } else {
          this.serverErrorMessage = "There is some problme";
        }
      }, (error) => {
          if (error.status === 400) {
            let errorMessage = "One or more inputs failed validation!"
            if ('errors' in error.error) {
              errorMessage = JSON.stringify(error.error.errors)
            } else {
              errorMessage = JSON.stringify(error.error)
            }
          this.serverErrorMessage = errorMessage;
        } else {
          this.serverErrorMessage = "There is some problem with the service.Please try again.If the problem persits please contract with system administrator";
        }

      });
    } else {
      this.validateAllFormFields(this.registrationFrom);
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
