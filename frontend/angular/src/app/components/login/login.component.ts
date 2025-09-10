import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthenticationResponse } from '../../models/authentication-response';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service'; 

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, MatFormFieldModule, MatInputModule, ReactiveFormsModule, MatCardModule, MatButtonModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  loginForm: FormGroup;

  constructor(
    private fb: FormBuilder, 
    private authService: AuthService, 
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      Email: ['', [Validators.required, Validators.email]],
      Password: ['', Validators.required],
    });
  }

  login(): void {
    if (this.loginForm.valid) {
      const { Email, Password } = this.loginForm.value;
      this.authService.login({ email: Email, password: Password }).subscribe({
        next: (response: AuthenticationResponse) => {
          if (response.userID === 'admin_id') {
            this.router.navigate(['admin', 'products']);
          } else {
            this.router.navigate(['products', 'showcase']);
          }
        },
        error: (error: any) => {
          console.log(error);
        }
      });
    }
  }

  get emailFormControl(): FormControl {
    return this.loginForm.get('Email') as FormControl;
  }

  get passwordFormControl(): FormControl {
    return this.loginForm.get('Password') as FormControl;
  }
}