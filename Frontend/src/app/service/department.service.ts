import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

export interface Department {
    id: string;
    name: string;
}

@Injectable({ providedIn: 'root' })
export class DepartmentService {
    constructor(private httpClient: HttpClient) { }
    baseUrl = "https://localhost:5224/api/Departments";

    getDepartments(): Observable<Department[]> {
        return this.httpClient.get<Department[]>(this.baseUrl);
    }

    createDepartment(dep: Department): Observable<Department> {
        return this.httpClient.post<Department>(this.baseUrl, dep);
    }

    updateDepartment(dep: Department): Observable<void> {
        return this.httpClient.put<void>(`${this.baseUrl}/${dep.id}`, dep);
    }

    deleteDepartment(id: string): Observable<void> {
        return this.httpClient.delete<void>(`${this.baseUrl}/${id}`);
    }
}
