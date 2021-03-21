import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { UserService } from 'src/app/user.service';
import Swal from 'sweetalert2/dist/sweetalert2.js';

@Component({
  selector: 'app-registration',
  templateUrl: './registration.component.html',
  styles: [
  ]
})
export class RegistrationComponent implements OnInit {

  constructor(public service:UserService,private router:Router) { }

  UserName:string="";
  Password:string="";
  Email:string="";

  ngOnInit(): void {
    if(localStorage.getItem('myToken') != null)
      this.router.navigateByUrl('/home');
    this.service.formModel.reset();
  }

  register(){
    this.service.usrRegister().subscribe(
      (res:any) =>{

        if(res.myResponseStatus == "1"){
          this.service.formModel.reset();

          Swal.fire('New Account Created',res.myResponseMessage,'success');
        } else {
          res.errors.forEach((element:{code:any;description:any}) => {
            switch (element.code){
              case "DuplicateUserName":

                Swal.fire('Username is already taken','Registration Failed','error');
                break;
              default:
                Swal.fire(element.description,'Registration Failed','error');
                break;
            }
          });
        }
      },
      err => {
        console.log(err);
      }
    );
  }

  login(){
    this.router.navigateByUrl('/user/login');
  }
}
