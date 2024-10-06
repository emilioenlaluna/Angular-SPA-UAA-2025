import { Component, inject, Input, Output, EventEmitter } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AccountService } from 'src/app/services/account.service';

@Component({
  selector: 'app-register',
  imports: [FormsModule],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss'] // Nota que es `styleUrls` para listas de estilos
})
export class RegisterComponent {
  private accountService = inject(AccountService);

  // Propiedad input para recibir usuarios desde el componente padre
  @Input() usersFromHomeComponent: any[] = [];

  // Output para emitir eventos al componente padre
  @Output() cancelRegister = new EventEmitter<boolean>();

  model: any = {};

  register(): void {
    this.accountService.register(this.model).subscribe({
      next: (response) => {
        console.log(response);
        this.cancel(); // Llamamos al método cancel en caso de éxito
      },
      error: (error) => {
        console.log(error); // Manejo de errores
      }
    });
  }

  cancel(): void {
    this.cancelRegister.emit(false); // Emitimos false para cancelar el registro
  }
}
