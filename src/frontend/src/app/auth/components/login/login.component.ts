import { Component, OnInit, inject } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators,
  ReactiveFormsModule,
} from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { Auth, signInWithEmailAndPassword } from '@angular/fire/auth';
import { FirebaseError } from '@angular/fire/app';
import { toObservable } from '@angular/core/rxjs-interop';
import { Subscription } from 'rxjs';
import { AuthService } from '../../../core/services/auth.service';
import { CommonModule } from '@angular/common';
import { HeaderComponent } from '../../../core/components/header/header.component';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    HeaderComponent,
    ReactiveFormsModule,
    HeaderComponent,
  ],
  templateUrl: './login.component.html',
})
export class LoginComponent implements OnInit {
  loginForm!: FormGroup;
  isLoading = false;
  errorMessage: string | null = null;

  public mainImage = 'assets/ParkingOverview.jpeg';
  private fb = inject(FormBuilder);
  private afAuth = inject(Auth);
  private authService = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  private status$ = toObservable(this.authService.appUserResource.status);
  private syncSub?: Subscription;

  ngOnInit(): void {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
    });
  }

  onSubmit(): void {
    this.loginForm.markAllAsTouched();
    if (this.loginForm.invalid) {
      return;
    }

    this.isLoading = true;
    this.errorMessage = null;
    const { email, password } = this.loginForm.value;

    signInWithEmailAndPassword(this.afAuth, email, password)
      .then(() => {
        this.router.navigate(['/portal']);
      })
      .catch((err: FirebaseError) => {
        this.isLoading = false;
        this.errorMessage = this.mapError(err.code);
      });
  }

  private mapError(code: string): string {
    switch (code) {
      case 'auth/invalid-email':
        return 'Invalid email format.';
      case 'auth/user-disabled':
        return 'This user account has been disabled.';
      case 'auth/user-not-found':
      case 'auth/wrong-password':
      case 'auth/invalid-credential':
        return 'Invalid email or password.';
      default:
        return 'An unexpected error occurred during login. Please try again.';
    }
  }
}
