import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { Injectable, Injector } from "@angular/core";
import { Router } from "@angular/router";
import { catchError, Observable, switchMap, throwError } from "rxjs";
import { AuthenticaionService, RefreshTokenResponse } from "./authenticaion.service";
@Injectable()
export class JwtCookiesInterceptor implements HttpInterceptor {
    constructor(private _injector: Injector, private _router: Router) {}

    /**
     * Xử lý dữ liệu khi không kết nối được với Token phía BE
     * @param {HttpRequest<any>} req
     * @param {HttpHandler} next
     * @return {*}  {Observable<HttpEvent<any>>}
     * @memberof CookieInterceptor
     */
    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
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
                if (res && res.message)
                    if (['ExpiredTimeCookie', 'Unthorize'].indexOf(res.message)) {
                        this._router.navigate(['/dang-nhap']);
                        return throwError(() => originalError)
                    }
                return next.handle(request);
            }),
            catchError((error: any) => {
                // Xử lý thông tin User
                this._router.navigate(['/dang-nhap']);
                return throwError(() => error);
            })
        );
    }
}