import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { Injectable, Injector } from "@angular/core";
import { MatSnackBar } from "@angular/material/snack-bar";
import { Router } from "@angular/router";
import { catchError, Observable, of, switchMap, throwError } from "rxjs";
import { AuthenticaionService, RefreshTokenResponse } from "./authenticaion.service";


@Injectable()
export class JwtCookiesInterceptor implements HttpInterceptor {

    time = 0;
    /**
     *
     */
    constructor(private _injector: Injector, private _router: Router, private _snackbar: MatSnackBar) {

    }

    /**
     * Xử lý dữ liệu khi không kết nối được với Token phía BE
     * @param {HttpRequest<any>} req
     * @param {HttpHandler} next
     * @return {*}  {Observable<HttpEvent<any>>}
     * @memberof CookieInterceptor
     */
    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        debugger
        console.log(`url: ${req.url}`, req.url.indexOf('refresh'))
        // nếu là refresh token thì bỏ qua để chạy
        if (req.url.indexOf('refresh') != -1)
            return next.handle(req);

        return next.handle(req).pipe(
            catchError((error) => {
                if (error.status === 401) {
                    return this.handleExceptionErrorAuthorize401(req, next, error)
                }
                else return throwError(() => error)
            })
        )
    }


    /**
     * Xử lý lỗi không lấy được Token
     * @private
     * @param {HttpRequest<any>} request
     * @param {HttpHandler} next
     * @param {*} originalError
     * @return {*} 
     * @memberof CookieInterceptor
     */
    private handleExceptionErrorAuthorize401(request: HttpRequest<any>, next: HttpHandler, originalError: any) {
        // Chạy về BE lấy token từ server
        let services = this._injector.get(AuthenticaionService);
        return services.refreshToken().pipe(
            switchMap((res: RefreshTokenResponse) => {
                debugger
                if (res && res.message)
                    if (['ExpiredTimeCookie', 'Unthorize'].indexOf(res.message)) {
                        this._router.navigate(['/dang-nhap']);
                        return throwError(() => originalError)
                    }

                return next.handle(request);
            }),
            catchError((error: any) => {
                debugger
                // Xử lý thông tin User
                this._router.navigate(['/dang-nhap']);
                return throwError(() => originalError);
            })
        );
    }

    // intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    //     return next.handle(req).pipe(
    //         catchError(x => this.handleAuthError(x)
    //         )
    //     )
    // }

    // private handleAuthError(error: HttpErrorResponse): Observable<any> {
    //     if (error && error.status == 401 && this.time != 1) {
    //         this.time++;

    //         let services = this._injector.get(AuthenticaionService);
    //         services.refreshToken().subscribe({
    //             next: () => {
    //                 this._snackbar.open('Token has been refresh, try again');
    //                 return of("Đã refresh token ");
    //             },
    //             error: error => {
    //                 // Thu hồi token

    //                 // Quay về trang đăng nhập
    //                 this._router.navigate(['/dang-nhap']);
    //                 return of(error)
    //             }
    //         })

    //         return of("Attempting to Refresh Token ");
    //     }
    //     else {
    //         this.time = 0;
    //         return throwError(() => new Error("None Authenticaion Error"))
    //     }
    // }

}