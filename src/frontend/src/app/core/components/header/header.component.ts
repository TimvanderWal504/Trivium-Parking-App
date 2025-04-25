import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { MatIconModule, MatIconRegistry } from '@angular/material/icon';
import { DomSanitizer } from '@angular/platform-browser';

@Component({
  selector: 'app-header',
  imports: [CommonModule, MatIconModule],
  templateUrl: './header.component.html',
})
export class HeaderComponent {
  private matIconRegistry = inject(MatIconRegistry);
  private sanitizer = inject(DomSanitizer);
  public authService = inject(AuthService);

  constructor() {
    this.matIconRegistry.addSvgIcon(
      'parkPilotLogo',
      this.sanitizer.bypassSecurityTrustResourceUrl('assets/trivium-logo.svg')
    );
  }

  logout(): void {
    this.authService.signOut();
  }
}
