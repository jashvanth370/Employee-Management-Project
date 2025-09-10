import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { EmployeeService } from '../service/employee.service';
import { Router } from '@angular/router';
import { Department, DepartmentService } from '../service/department.service';

@Component({
    selector: 'app-register',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule],
    templateUrl: './register.component.html'
})
export class RegisterComponent implements OnInit {
    form: FormGroup;
    departments: Department[] = [];
    submitting = false;

    constructor(
        private fb: FormBuilder,
        private employeesService: EmployeeService,
        private router: Router,
        private depService: DepartmentService
    ) {
        this.form = this.fb.group({
            name: ['', Validators.required],
            emailId: ['', [Validators.required, Validators.email]],
            mobileNo: ['', Validators.required],
            password: ['', Validators.required],
            departmentId: ['']
        });
    }

    ngOnInit(): void {
        this.loadDepartments();
    }

    private loadDepartments() {
        this.depService.getDepartments().subscribe({
            next: (res) => this.departments = res,
            error: (err) => console.error('Failed to load departments', err)
        });
    }

    photoFile: File | null = null;

    onFileSelected(event: Event) {
        const input = event.target as HTMLInputElement;
        if (input.files && input.files.length) {
            this.photoFile = input.files[0];
        }
    }

    onSubmit() {
        if (this.form.invalid) return;

        this.submitting = true;

        const formData = new FormData();
        formData.append('name', this.form.value.name);
        formData.append('emailId', this.form.value.emailId);
        formData.append('mobileNo', this.form.value.mobileNo);
        formData.append('password', this.form.value.password);
        formData.append('departmentId', this.form.value.departmentId || '');

        if (this.photoFile) {
            formData.append('photo', this.photoFile, this.photoFile.name);
        }

        this.employeesService.CreateEmployees(formData).subscribe({
            next: (res) => {
                alert('Employee registered successfully');
                this.router.navigate(['/login']);
            },
            error: (err) => {
                alert('Registration failed: ' + (err.error?.message || 'Unknown error'));
            }
        });
    }

}
