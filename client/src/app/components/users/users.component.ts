import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-users',
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.scss']
})
export class UsersComponent implements OnInit {
  title = 'Date me';
  users: any;

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.http.get("https://localhost:5001/api/users").subscribe({
      next: (response) => { this.users = response; },
      error: (error) => { console.log(error); },
      complete: () => { console.log("Request completed!"); }
    });
  }

  // trackByUserId method added within the UsersComponent class
  trackByUserId(index: number, user: any): number {
    return user.id;
  }
}
