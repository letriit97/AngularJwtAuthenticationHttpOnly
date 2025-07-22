import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthenticaionService } from '../services/authenticaion.service';
import { DataService } from '../services/data.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {

  colors: string[] = [];

  constructor(
    private router: Router,
    private _services: AuthenticaionService,
    private _dataServices: DataService
  ) { }

  ngOnInit(): void {
  }

  getData(){
    this._dataServices.getData().subscribe({
      next: colors => {
        this.colors = colors;
      }
    })
  }

  logOut(){
    this._services.logout().subscribe({
      next: () => {
        localStorage.removeItem('user');
        this.router.navigate(['/dang-nhap'])
      }
    })
    
  }

}
