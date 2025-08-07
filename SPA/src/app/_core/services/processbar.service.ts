import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ProcessbarService {

  private _isLoading = new BehaviorSubject<boolean>(false);
  private _progressValue = new BehaviorSubject<number>(0);

  isLoading$: Observable<boolean> = this._isLoading.asObservable();
  progressValue$: Observable<number> = this._progressValue.asObservable();

  show(): void {
    this._isLoading.next(true);
    this._progressValue.next(0); // Reset progress when showing
  }

  hide(): void {
    this._isLoading.next(false);
    this._progressValue.next(0);
  }

  setProgress(value: number): void {
    this._progressValue.next(value);
  }

}
