import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, ParamMap } from '@angular/router';
import { switchMap } from 'rxjs/operators';
import { UserService } from '../../user.service';
import Swal from 'sweetalert2/dist/sweetalert2.js';

@Component({
  selector: 'app-validate-email',
  templateUrl: './validate-email.component.html',
  styles: [],
})
export class ValidateEmailComponent implements OnInit {
  order: string;
  param1: string;
  constructor(private route: ActivatedRoute,private service:UserService,private router:Router) {
    console.log('Called Constructor');
    this.route.queryParams.subscribe((params) => {
      this.param1 = params['param1'];
    });
    console.log(this.param1);
    this.ValidateEmail();
  }

  ngOnInit(): void {

  }

  ValidateEmail(){
    var val = {
      uid: this.param1
    };
    this.service.ValidateEmail(val).subscribe((res:any)=>{
      console.log(res.myResponseStatus);
      if(res.myResponseStatus == 1){
        Swal.fire('Success',res.myResponseMessage,'success');
        this.router.navigateByUrl('/home');
      }
      if(res.myResponseStatus == 0){
        Swal.fire('Error',res.myResponseMessage,'error');
      }

    });
  }
}
