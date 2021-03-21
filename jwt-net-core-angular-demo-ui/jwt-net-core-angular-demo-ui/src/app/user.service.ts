import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  readonly APIUrl = 'http://localhost:9105/api/';

  constructor(private fb: FormBuilder, private http: HttpClient) {}

  formModel = this.fb.group({
    UserName: ['', Validators.required],
    Email: ['', Validators.email],
    FirstName: [''],
    LastName: [''],
    Passwords: this.fb.group(
      {
        Password: ['', [Validators.required, Validators.minLength(4)]],
        ConfirmPassword: ['', Validators.required],
      },
      { validator: this.usrComparePasswords }
    ),
  });
  usrRegister() {
    var body = {
      UserName: this.formModel.value.UserName,
      FirstName: this.formModel.value.FirstName,
      LastName: this.formModel.value.LastName,
      Email: this.formModel.value.Email,
      Password: this.formModel.value.Passwords.Password,
    };
    return this.http.post(this.APIUrl + 'User/Register', body);
  }

  usrLogin(formData: any) {
    return this.http.post(this.APIUrl + 'User/Login', formData);
  }

  ValidateEmail(val:any){
    return this.http.post(this.APIUrl+'User/ValidateEmail',val);
  }

  getUserProfile() {
    return this.http.get(this.APIUrl + 'User');
  }

  usrComparePasswords(fb: FormGroup) {
    let confirmPasswordControl = fb.get('ConfirmPassword');
    if (fb.get('Password').value != confirmPasswordControl.value)
      confirmPasswordControl.setErrors({ passwordMismatch: true });
    else confirmPasswordControl.setErrors(null);
  }
}
