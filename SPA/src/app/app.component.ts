import { Component, ViewChild } from '@angular/core';
import { ProcessbarService } from './_core/services/processbar.service';
import { AuthenticaionService } from './services/authenticaion.service';
import { Router } from '@angular/router';
import { MatDrawer } from '@angular/material/sidenav';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  @ViewChild("drawer") drawer!: MatDrawer;

  title = 'DEMO Angular Cookies Jwt';

  panelOpenState = false;
  constructor(
    private _services: AuthenticaionService,
    private router: Router,
    public progressBarService: ProcessbarService) {
  }

  onToggleSidebar(){
    this.drawer.toggle();
  }

  logOut() {
    this._services.logout().subscribe({
      next: () => {
        this.onToggleSidebar();
        localStorage.removeItem('user');
        this.router.navigate(['/dang-nhap'])
      }
    })

  }
}
