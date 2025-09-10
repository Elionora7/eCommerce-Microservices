import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthenticationResponse } from '../../models/authentication-response';
import { UsersService } from '../../services/users.service';
import { MatButtonModule } from "@angular/material/button";
import { MatIconModule } from "@angular/material/icon";
import { MatToolbarModule } from "@angular/material/toolbar";
import { MatCardModule } from "@angular/material/card";
import { Router, RouterModule } from "@angular/router";
import { MatOptionModule } from '@angular/material/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from "@angular/material/select";
import { AuthService } from '../../services/auth.service'; // Import AuthService

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatButtonModule, MatIconModule, MatToolbarModule, RouterModule, MatCardModule, MatOptionModule, MatFormFieldModule, MatInputModule, MatSelectModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {
  registerForm: FormGroup;

  constructor(
    private fb: FormBuilder, 
    private usersService: UsersService, 
    private authService: AuthService, 
    private router: Router
  ) {
    this.registerForm = this.fb.group({
      Name: ['', Validators.required],
      Email: ['', [Validators.required, Validators.email]],
      Password: ['', [Validators.required, Validators.minLength(3)]],
      Gender: [''],
    });
  }

  register(): void {
    if (this.registerForm.valid) {
      const user = this.registerForm.value;
      this.usersService.register(user).subscribe({
        next: (response: AuthenticationResponse) => {
          // Use AuthService instead of UsersService for authentication
          this.authService.setAuthData(response, response.token);
          this.router.navigate(['products', 'showcase']);
        },
        error: (error: any) => {
          console.log(error);
        }
      });
    }
  }

  get emailFormControl(): FormControl {
    return this.registerForm.get('Email') as FormControl;
  }

  get passwordFormControl(): FormControl {
    return this.registerForm.get('Password') as FormControl;
  }

  get nameFormControl(): FormControl {
    return this.registerForm.get('Name') as FormControl;
  }

  get genderFormControl(): FormControl {
    return this.registerForm.get('Gender') as FormControl;
  }
}