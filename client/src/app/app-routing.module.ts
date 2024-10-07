import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './components/home/home.component';
import { MemberListComponent } from './components/member-list/member-list.component';
import { ListsComponent } from './components/lists/lists.component';

const routes: Routes = [
 {path:"",component:HomeComponent},
 {path:"members",component:MemberListComponent},
 {path:"lists",component:ListsComponent},

 {path:"**", component:HomeComponent,pathMatch:"full" }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
