import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { environment } from "../../environments/environment";
import { AuthService } from "./auth.service";
import { RegisterRequest } from "./register-request";
import { RegisterResult } from "./register-result";

@Injectable({
  providedIn: 'root'
})
export class RegisterService {
  constructor(protected http: HttpClient, private authService: AuthService) { }

  register(item: RegisterRequest): Observable<RegisterResult> {
    var url = environment.baseUrl + 'api/account/register';
    return this.http.post<RegisterResult>(url, item);
  }
}
