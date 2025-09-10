import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Employee } from '../model/empoloyee';

@Injectable({
  providedIn: 'root'
})
export class EmployeeService {

  constructor(private httpClient: HttpClient) { }

  baseUrl = "https://localhost:5224/api";

  // Get all employees
  GetEmployees(): Observable<Employee[]> {
    return this.httpClient.get<Employee[]>(`${this.baseUrl}/Employees`);
  }

  //Upload a photo for an employee
  uploadPhoto(id: string, file: File): Observable<{ photoUrl: string }> {
    const formData = new FormData();
    formData.append('file', file);
    return this.httpClient.post<{ photoUrl: string }>(`${this.baseUrl}/Employees/${id}/photo`, formData);
  }

  //Delete a photo for an employee
  deletePhoto(id: string): Observable<void> {
    return this.httpClient.delete<void>(`${this.baseUrl}/Employees/${id}/photo`);
  }

  // Create a new employee
  CreateEmployees(data: FormData): Observable<any> {
    return this.httpClient.post(`${this.baseUrl}/Employee/register`, data);
  }



  // Update employee
  UpdateEmployees(emp: Employee): Observable<Employee> {
    const payload = {
      Name: emp.name,
      MobileNo: emp.mobileNo,
      EmailId: emp.emailId,
      DepartmentId: emp.departmentId && emp.departmentId !== '' ? emp.departmentId : null,
      PhotoUrl: emp.photoUrl,
    };
    return this.httpClient.put<Employee>(`${this.baseUrl}/Employees/${emp.id}`, payload);
  }

  // Delete employee
  DeleteEmployees(id: string): Observable<void> {
    return this.httpClient.delete<void>(`${this.baseUrl}/Employees/${id}`);
  }

  // Register employee (alias for CreateEmployees)
  // RegisterEmployee(emp: Employee): Observable<Employee> {
  //   return this.CreateEmployees(emp);
  // }

  // Login employee
  LoginEmployee(email: string, password: string): Observable<any> {
    const payload = {
      EmailOrMobile: email,
      Password: password
    };
    return this.httpClient.post<any>(`${this.baseUrl}/Auth/login`, payload, {
      headers: { 'Content-Type': 'application/json' }
    });
  }
}
