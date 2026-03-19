import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { LoadingOverlayComponent } from './shared/components/loading-overlay/loading-overlay.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, CommonModule, LoadingOverlayComponent],
  template: `
    <app-loading-overlay></app-loading-overlay>
    <router-outlet></router-outlet>
  `,
  styles: []
})
export class AppComponent {
  title = 'Design Review System';
}
