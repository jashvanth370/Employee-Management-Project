export interface Employee {
  id?: string;  
  name: string;
  mobileNo?: string;
  emailId: string;
  departmentId?: string;
  department?: { id: string; name: string }; 
  passwordHash?: string; 
  createdAt?: Date;      
  updatedAt?: Date;
  photoUrl?: string; // URL for employee photo
}
