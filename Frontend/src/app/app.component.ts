import { Component, OnInit } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { EmployeeService } from './service/employee.service';
import { Employee } from './model/empoloyee';
import { DepartmentService, Department } from './service/department.service';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
// import { msalInstance } from './service/auth.interceptor';
import { Login } from './login/login.component';
import { MsalService, MsalBroadcastService } from '@azure/msal-angular';
import { AuthenticationResult } from '@azure/msal-browser';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, CommonModule, ReactiveFormsModule, Login],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  EmployeeAry: Employee[] = [];
  Departments: Department[] = [];
  Employeeformgroup: FormGroup;
  loginForm: FormGroup;
  isLoggedIn = false;
  title = 'angular';
  registerForm: FormGroup;
  showRegisterForm = false;

  // For Microsoft SSO
  msalUser?: AuthenticationResult;


  constructor(
    private empservice: EmployeeService,
    private fb: FormBuilder,
    private depService: DepartmentService,
    private msalService: MsalService // Microsoft
  ) {
    this.Employeeformgroup = this.fb.group({
      id: [null],
      name: ['', Validators.required],
      mobileNo: ['', Validators.required],
      emailId: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required],
      departmentId: ['']
    });

    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
    // ✅ Register form
    this.registerForm = this.fb.group({
      name: ['', Validators.required],
      emailId: ['', [Validators.required, Validators.email]],
      mobileNo: ['', Validators.required],
      departmentId: [null],
      password: ['', Validators.required]

    });
  }



  ngOnInit(): void {
    const token = localStorage.getItem('jwt');
    this.isLoggedIn = !!token;
    if (this.isLoggedIn) {
      this.refreshData();
    }


    // this.loadDepartments();
    // msalInstance.initialize().then(() => {
    //   console.log('MSAL initialized ');
    // }).catch(err => {
    //   console.error('MSAL initialization failed', err);
    // });
  }

  refreshData() {
    this.getEmployees();
    this.loadDepartments();
  }

  // Fetch all employees
  getEmployees() {
    this.empservice.GetEmployees().subscribe(
      response => this.EmployeeAry = response,
      error => console.error('Error fetching employees:', error)
    );
  }

  // Load departments
  loadDepartments() {
    this.depService.getDepartments().subscribe(
      res => this.Departments = res,
      err => console.error('Error loading departments:', err)
    );
  }

  // Register or update employee
  onSubmit() {
    if (!this.isLoggedIn) { return; }
    if (this.Employeeformgroup.invalid) { return; }

    const emp = this.Employeeformgroup.value;
    emp.passwordHash = emp.password;
    if (emp.departmentId === '') { emp.departmentId = null; }

    if (emp.id) {
      this.empservice.UpdateEmployees(emp).subscribe(
        _ => { this.refreshData(); this.Employeeformgroup.reset(); },
        err => alert('Error updating employee')
      );
    } else {
      this.empservice.CreateEmployees(emp).subscribe(
        _ => { this.refreshData(); this.Employeeformgroup.reset(); },
        err => alert('Error creating employee')
      );
    }
  }

  // ✅ Register method
  // onRegister() {
  //   if (this.registerForm.invalid) { return; }
  //   const emp = this.registerForm.value;

  //   // You might already have a backend API to register users
  //   this.empservice.RegisterEmployee(emp).subscribe(
  //     _ => {
  //       alert('Registration successful! Please login.');
  //       this.showRegisterForm = false; // switch back to login
  //       this.registerForm.reset();
  //     },
  //     _ => alert('Registration failed')
  //   );
  // }


  fillForm(emp: Employee) {
    if (!this.isLoggedIn) { return; }
    this.Employeeformgroup.setValue({
      id: emp.id,
      name: emp.name,
      mobileNo: emp.mobileNo,
      emailId: emp.emailId,
      password: '',
      departmentId: emp.departmentId ?? ''
    });
  }

  deleteEmp(id: string) {
    if (!this.isLoggedIn) { return; }
    if (confirm('Are you sure you want to delete this employee?')) {
      this.empservice.DeleteEmployees(id).subscribe(_ => this.refreshData());
    }
  }




  // Login and SSO
  //   handleMicrosoftLogin() {
  //   // Call this after redirect to handle token
  //   this.msalService.instance.handleRedirectPromise().then(res => {
  //     if (res) {
  //       console.log('MSAL User:', res);
  //       this.msalUser = res;
  //       // Send accessToken to backend for JWT
  //       this.empservice.LoginWithMicrosoft({ accessToken: res.accessToken }).subscribe({
  //         next: r => {
  //           localStorage.setItem('jwt', r.token);
  //           this.isLoggedIn = true;
  //           this.refreshData();
  //         },
  //         error: err => alert('Microsoft login failed')
  //       });
  //     }
  //   }).catch(err => console.error(err));
  // }

  // onLogout() {
  //   msalInstance.logoutPopup();
  // }

  onLogout() {
    // Remove JWT token from localStorage
    localStorage.removeItem('jwt');
    this.isLoggedIn = false;

    // If using MSAL (Microsoft SSO), sign out from Microsoft as well
    // msalInstance.logoutPopup();

    // Optionally, reset forms and clear user data
    this.loginForm.reset();
    this.registerForm.reset();
    this.Employeeformgroup.reset();
    // You may also want to clear EmployeeAry and other sensitive data
    this.msalService.logoutRedirect();
  }

  getDepartmentName(depId: string | undefined): string {
    const dep = this.Departments.find(d => d.id === depId);
    return dep ? dep.name : '-';
  }

  clearForm() { this.Employeeformgroup.reset(); }

  // edit-employee.component.ts
  selectedFile?: File;
  previewUrl?: string | ArrayBuffer | null;

  // onFileSelected(ev: Event) {
  //   const input = ev.target as HTMLInputElement;
  //   if (!input.files || input.files.length === 0) return;
  //   const file = input.files[0];

  //   // Basic client-side validation
  //   const allowed = ['image/jpeg', 'image/png', 'image/webp'];
  //   if (!allowed.includes(file.type) || file.size > 2 * 1024 * 1024) {
  //     alert('Only JPG/PNG/WEBP up to 2MB allowed.');
  //     return;
  //   }

  //   this.selectedFile = file;

  //   const reader = new FileReader();
  //   reader.onload = () => this.previewUrl = reader.result;
  //   reader.readAsDataURL(file);
  // }

  upload(id: string | undefined) {
    if (!this.selectedFile) return;
    if (!id) return;

    const employee = this.EmployeeAry.find(e => e.id === id);
    if (!employee) return;
    this.empservice.uploadPhoto(id, this.selectedFile).subscribe({
      next: res => {
        employee.photoUrl = res.photoUrl.startsWith('http')
          ? res.photoUrl :
          'https://localhost:7024/' + res.photoUrl;
        this.previewUrl = undefined;
        this.selectedFile = undefined;
      },
      error: () => alert('Upload failed')
    });
  }
  getInitials(name: string = ''): string {
    return name
      .split(' ')
      .map(n => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  }
  onFileSelected(ev: Event) {
    const input = ev.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) return;
    const file = input.files[0];

    const allowed = ['image/jpeg', 'image/png', 'image/webp'];
    if (!allowed.includes(file.type) || file.size > 20 * 1024 * 1024) {
      alert('Only JPG/PNG/WEBP up to 2MB allowed.');
      return;
    }

    this.selectedFile = file;

    const reader = new FileReader();
    reader.onload = () => this.previewUrl = reader.result;
    reader.readAsDataURL(file);
  }



  remove(id: string | undefined) {
    const employee = this.EmployeeAry.find(e => e.id === id);
    if (!id) return;
    if (!employee) return;
    this.empservice.deletePhoto(id).subscribe({
      next: () => employee.photoUrl = undefined,
      error: () => alert('Delete failed')
    });
  }

  // getInitials(name: string = '') {
  //   return name.split(' ').map(n => n[0]).join('').toUpperCase().slice(0,2);
  // }


}



