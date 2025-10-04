import { Component } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html'
})
export class LoginComponent {
  loginForm: FormGroup;

  constructor(private fb: FormBuilder, private http: HttpClient, private router: Router) {
    this.loginForm = this.fb.group({ username: [''], password: [''] });
  }

  login() {
    this.http.post<any>('https://localhost:5001/api/auth/login', this.loginForm.value)
      .subscribe({
        next: res => {
          localStorage.setItem('jwt', res.token);
          const payload = JSON.parse(atob(res.token.split('.')[1]));
          switch(payload.role) {
            case 'Customer': this.router.navigate(['/customer-dashboard']); break;
            case 'Vendor': this.router.navigate(['/vendor-dashboard']); break;
            case 'IT': this.router.navigate(['/it-dashboard']); break;
            case 'Admin': this.router.navigate(['/admin-dashboard']); break;
          }
        },
        error: err => alert('Login failed')
      });
  }
}
