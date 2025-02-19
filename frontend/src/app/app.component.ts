import {Component, OnInit} from '@angular/core';
import {Router, RouterOutlet} from '@angular/router';
import {InfoPopupComponent} from "./info-popup/info-popup.component";
import {NgIf} from "@angular/common";

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, InfoPopupComponent, NgIf],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  showInfoPopup = false;

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

  openInfoPopup(event: Event): void {
    event.preventDefault();
    this.showInfoPopup = true;
  }

  closeInfoPopup(): void {
    this.showInfoPopup = false;
  }
}
