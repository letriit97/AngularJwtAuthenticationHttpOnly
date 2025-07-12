import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, CanLoad, Route, Router, RouterStateSnapshot, UrlSegment, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AppGuardGuard implements CanActivate, CanLoad {

  /**
   *
   */
  constructor(private _router: Router) {
    
  }
  canLoad(route: Route, segments: UrlSegment[]): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {
    let info = localStorage.getItem('user');
    if (info == null || info == undefined)
      return this._router.navigate(['dang-nhap'])

    return true
  }
  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {

      let info = localStorage.getItem('user');
      if(info == null || info == undefined)
        return this._router.navigate(['/dang-nhap'])

    return true;
  }
}
