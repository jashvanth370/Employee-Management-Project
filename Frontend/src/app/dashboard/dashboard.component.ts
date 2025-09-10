import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EmployeeService } from '../service/employee.service';
import { Employee } from '../model/empoloyee';

@Component({
    selector: 'app-dashboard',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {
    employees: Employee[] = [];
    constructor(private employeesService: EmployeeService) { }
    ngOnInit(): void {
        this.employeesService.GetEmployees().subscribe({
            next: (res) => this.employees = res,
            error: (err) => console.error(err)
        });
    }
}


