import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { catchError, of, switchMap } from 'rxjs';
import { BaseFormComponent } from '../base-form.component';
import { AuthService } from './auth.service';
import { LoginRequest } from './login-request';
import { RegisterRequest } from './register-request';
import { RegisterResult } from './register-result';
import { RegisterService } from './register.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent extends BaseFormComponent implements OnInit {

  title?: string;
  registerResult?: RegisterResult;

  constructor(
    private activatedRoute: ActivatedRoute,
    private router: Router,
    private registerService: RegisterService,
    private loginService: AuthService
  ) {
    super();
  }

  ngOnInit(): void {
    this.form = new FormGroup({
      email: new FormControl("", Validators.required),
      password: new FormControl("", Validators.required)
    });
  }

  onSubmit() {
    var registerRequest = {} as RegisterRequest;

    registerRequest.email = this.form.controls['email'].value;
    registerRequest.password = this.form.controls['password'].value;

    this.registerService.register(registerRequest)
      .pipe(
        switchMap(registerResult => {
          console.log(registerResult);
          this.registerResult = registerResult;
          if (registerResult.success) {
            const loginRequest: LoginRequest = {
              email: registerRequest.email,
              password: registerRequest.password
            };

            return this.loginService.login(loginRequest);
          }
          else {
            throw new Error("Registration Failed");
          }
        }), catchError(error => {
          console.log(error);
          return of(null);
        })
      )
      .subscribe(loginResult => {
        if (loginResult?.success) {
          this.router.navigate(['/']);
        }
      });
  }
}
