import {
  Directive,
  ElementRef,
  HostListener,
  inject,
  Renderer2,
} from '@angular/core';
import { RefreshService } from '../services/refresh.service';

@Directive({
  selector: '[appPullToRefresh]',
  standalone: true,
})
export class PullToRefreshDirective {
  private refreshService = inject(RefreshService);
  private el = inject(ElementRef<HTMLElement>);
  private renderer = inject(Renderer2);

  private startY = 0;
  private currentY = 0;
  private threshold = 60;
  private pulling = false;

  constructor() {
    this.renderer.setStyle(this.el.nativeElement, 'overflowY', 'auto');
    this.renderer.setStyle(this.el.nativeElement, 'touchAction', 'pan-y');
  }

  @HostListener('touchstart', ['$event'])
  onTouchStart(event: TouchEvent): void {
    if (this.el.nativeElement.scrollTop === 0) {
      this.startY = event.touches[0].clientY;
      this.pulling = true;
    }
  }

  @HostListener('touchmove', ['$event'])
  onTouchMove(event: TouchEvent): void {
    if (!this.pulling) return;
    this.currentY = event.touches[0].clientY;
    const delta = this.currentY - this.startY;
    if (delta > 0) {
      event.preventDefault();
      const pullDistance = Math.min(delta, this.threshold * 1.5);
      this.renderer.setStyle(
        this.el.nativeElement,
        'transform',
        `translateY(${pullDistance}px)`
      );
    }
  }

  @HostListener('touchend')
  onTouchEnd(): void {
    if (!this.pulling) return;

    const delta = this.currentY - this.startY;
    this.renderer.setStyle(
      this.el.nativeElement,
      'transition',
      'transform 0.3s ease'
    );
    this.renderer.setStyle(this.el.nativeElement, 'transform', 'translateY(0)');

    if (delta > this.threshold) {
      this.refreshService.requestReload();
    }

    setTimeout(() => {
      this.renderer.removeStyle(this.el.nativeElement, 'transition');
    }, 300);

    this.pulling = false;
  }
}
