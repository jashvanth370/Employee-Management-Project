import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Department, DepartmentService } from '../service/department.service';

@Component({
    selector: 'app-departments',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './departments.component.html'
})
export class DepartmentsComponent implements OnInit {
    departments: Department[] = [];
    constructor(private depService: DepartmentService) { }
    ngOnInit(): void {
        this.depService.getDepartments().subscribe({
            next: (res) => this.departments = res,
            error: (err) => console.error(err)
        });
    }
}


