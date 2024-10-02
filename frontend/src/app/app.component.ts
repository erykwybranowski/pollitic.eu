import {Component, OnInit} from '@angular/core';
import {Router, RouterOutlet} from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'pge-frontend';

  constructor(private router: Router) {

  }

  onLogoClick(event: Event) {
    event.preventDefault();  // Prevent the default anchor behavior

    if (this.router.url === '/') {
      // Force component reload
      this.router.navigateByUrl('/refresh', { skipLocationChange: true }).then(() => {
        this.router.navigate(['/']);
      });
    } else {
      // Navigate to home page
      this.router.navigate(['/']);
    }
  }
}
