import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { UserService } from '../../user.service';
import Swal from 'sweetalert2/dist/sweetalert2.js';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styles: [
  ]
})
export class LoginComponent implements OnInit {

  constructor(private service:UserService,private router:Router) { }

  UserName:string="";
  Password:string="";

  ngOnInit(): void {
    if(localStorage.getItem('myToken') != null)
      this.router.navigateByUrl('/home');
  }

  login(){
    var val = {
      UserName:this.UserName,
      Password:this.Password
    }
    this.service.usrLogin(val).subscribe(
      (res:any)=>{
          if(res.myResponseStatus == "1")
          {
            localStorage.setItem('myToken',res.myToken);
            this.router.navigateByUrl('/home');
          }
          if(res.myResponseStatus == "0")
          {
            Swal.fire('Authentication Failed',res.myResponseMessage,'error');
            localStorage.removeItem('myToken');
          }

      }

    );
  }
  createNew(){
    this.router.navigateByUrl('/user/registration');
  }

}
