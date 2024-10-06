import { HttpClient } from '@angular/common/http';
import { Component, inject, OnInit } from '@angular/core';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {
  http = inject(HttpClient);
  registerMode = false;
  users: any;

  ngOnInit(): void {
    this.getUsers();
  }

  registerToggle(): void {
    this.registerMode = !this.registerMode;
  }

  cancelRegisterMode(registerMode: boolean) {
    this.registerMode = registerMode;
  }
  

  getUsers() {
    this.http.get<Object>("https://localhost:5001/api/users").subscribe({
      next: (response) => { this.users = response; },
      error: (error) => { console.log(error); },
      complete: () => { console.log("Request completed!"); }
    });
  }
}