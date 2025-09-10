import { AfterViewInit, Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Auth } from '../service/auth.service';
import { CommonModule } from '@angular/common';
import { EmployeeService } from '../service/employee.service';
import { Employee } from '../model/empoloyee';
import { Department, DepartmentService } from '../service/department.service';
import { msalInstance, isBrowserEnv } from '../service/msal-instance';
import { AppConfig } from '../config';

declare const google: any;

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, ReactiveFormsModule, CommonModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.css']
})
export class Login implements AfterViewInit {
  loginForm: FormGroup;
  showRegisterForm = false;
  isLoggedIn = false;
  EmployeeAry: Employee[] = [];
  Departments: Department[] = [];

  constructor(
    private empservice: EmployeeService,
    private depService: DepartmentService,
    private fb: FormBuilder,
    private auth: Auth,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  ngAfterViewInit(): void {
    if (!isBrowserEnv) return;

    // Google Sign-In
    google.accounts.id.initialize({
      client_id: AppConfig.googleClientId,
      callback: this.handleGoogleResponse.bind(this)
    });
    google.accounts.id.renderButton(
      document.getElementById('googleSignInButton'),
      { theme: 'outline', size: 'large' }
    );
    google.accounts.id.prompt();
  }

  // --- JWT Login ---
  onLogin() {
    if (this.loginForm.invalid) return;

    const { email, password } = this.loginForm.value;
    const loginRequest = { EmailOrMobile: email, Password: password };

    this.auth.login(loginRequest).subscribe({
      next: (res) => {
        if (res?.token) {
          localStorage.setItem('jwt', res.token);
          this.auth.saveToken(res.token);
          this.isLoggedIn = true;
          this.refreshData();
          this.router.navigate(['/dashboard']);
        }
      },
      error: (err) => alert('Login failed: ' + (err.error?.message || 'Invalid credentials'))
    });
  }

  // --- Microsoft SSO ---
  // --- Microsoft SSO ---
async loginWithMicrosoft() {
  if (!isBrowserEnv || !msalInstance) {
    alert('Microsoft login is not available in this environment.');
    return;
  }

  try {
    const loginResp = await msalInstance.loginPopup({
      scopes: ['openid', 'profile', 'email'],
      prompt: "select_account" // forces account picker
    });

    const account = loginResp.account || msalInstance.getAllAccounts()[0];
    if (!account) throw new Error('No Microsoft account selected');

    // Try to acquire token silently, fallback to popup if needed
    const tokenResp = await msalInstance.acquireTokenSilent({
      account,
      scopes: ['openid', 'profile', 'email']
    }).catch(() =>
      msalInstance ? msalInstance.acquireTokenPopup({ scopes: ['openid', 'profile', 'email'] }) : Promise.reject(new Error('msalInstance is null'))
    );

    const idToken = tokenResp.idToken;

    // Send Microsoft ID token to your backend
    this.auth.microsoftLogin(idToken).subscribe({
      next: (res) => {
        if (res.token) {
          localStorage.setItem('jwt', res.token);
          this.auth.saveToken(res.token);
          alert('Microsoft Login Successful');
          this.router.navigate(['/dashboard']);
        } else {
          alert('Microsoft Login Failed: No token received');
        }
      },
      error: (err) => {
        console.error('Backend Microsoft login error', err);
        alert('Microsoft Login Failed: ' + (err.error?.message || 'Unknown error'));
      }
    });

  } catch (e: any) {
    // Handle specific MSAL errors
    if (e.errorCode === "user_cancelled") {
      console.warn("User cancelled Microsoft login");
      alert("You cancelled Microsoft login. Please try again.");
      return;
    }
    console.error('MSAL login error', e);
    alert('Microsoft Login Failed: ' + (e?.message || 'Unknown error'));
  }
}


  // --- Google SSO ---
  handleGoogleResponse(response: any) {
    this.auth.googleLogin(response.credential).subscribe({
      next: (res) => {
        if (res.token) {
          localStorage.setItem('jwt', res.token);
          this.auth.saveToken(res.token);
          alert('Google Login Successful');
          this.router.navigate(['/dashboard']);
        } else {
          alert('Google Login Failed: No token received');
        }
      },
      error: (err) => alert('Google Login Failed: ' + (err.error?.message || 'Unknown error'))
    });
  }

  refreshData() {
    this.getEmployees();
    this.loadDepartments();
  }

  getEmployees() {
    this.empservice.GetEmployees().subscribe({
      next: (res) => this.EmployeeAry = res,
      error: (err) => console.error('Error fetching employees:', err)
    });
  }

  loadDepartments() {
    this.depService.getDepartments().subscribe({
      next: (res) => this.Departments = res,
      error: (err) => console.error('Error loading departments:', err)
    });
  }

  onLogout() {
    localStorage.removeItem('jwt');
    this.isLoggedIn = false;
    this.loginForm.reset();
    this.EmployeeAry = [];
  }
}
