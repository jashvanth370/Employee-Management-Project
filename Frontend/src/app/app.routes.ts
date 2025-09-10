import { Routes } from '@angular/router';
import { Login } from '../app/login/login.component';
import { HomeComponent } from './home/home.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { DepartmentsComponent } from './departments/departments.component';
import { RegisterComponent } from './register/register.component';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'login', component: Login },
  { path: 'dashboard', component: DashboardComponent },
  { path: 'departments', component: DepartmentsComponent },
  { path: 'register', component: RegisterComponent }
];
