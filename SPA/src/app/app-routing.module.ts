import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AppComponent } from './app.component';
import { LoginComponent } from './login/login.component';
import { LayoutComponent } from './layout/layout.component';
import { HomeComponent } from './home/home.component';

const routes: Routes = [
  {
    path: '',
    component: LayoutComponent, // this is the component with the <router-outlet> in the template
    children: [
      {
        path: '', // child route path
        component: HomeComponent, // child route component that the router renders
      },
    ],
  },
  {
    path: 'dang-nhap',
    component: LoginComponent
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
