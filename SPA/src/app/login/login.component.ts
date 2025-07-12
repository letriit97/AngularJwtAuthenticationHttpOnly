import { Component, OnInit } from '@angular/core';
import { AuthenticaionService, LoginRequest } from '../services/authenticaion.service';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
  request: LoginRequest = new LoginRequest('Admin', '123');
  constructor(
    private _services: AuthenticaionService,
    private _snackBar: MatSnackBar,
    private _router: Router
  ) { }

  ngOnInit(): void {
  }

  

  login() {
    this._services.login(this.request).subscribe({
      next: result => {
        this._snackBar.open('Đăng nhập thành công !', 'Đóng' );
        localStorage.setItem('user', JSON.stringify(result))
        this._router.navigate(["/"])
      }
    })
  }

}
