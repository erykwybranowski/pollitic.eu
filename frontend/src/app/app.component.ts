import {Component, OnInit} from '@angular/core';
import {Router, RouterOutlet} from '@angular/router';
import {PartyService} from "./services/party.service";
import {Country} from "./models/country.model";
import {Party} from "./models/party.model";

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'pge-frontend';

  constructor(private router: Router, private partyService: PartyService) {
    partyService.getCountries().subscribe((countries: Country[]) => {
      const selectedCountry = countries[0];

      // Get the parties for the selected country
      const parties = partyService.getParties(selectedCountry);

      parties.subscribe((partyList: Party[]) => {
        console.log(partyList); // Log the actual parties
      });

    });

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

  // ngOnInit() {
  //   // Check if there is a logged-in user
  //   const currentUser = this.authService.currentUserValue;
  //   if (!currentUser || this.authService.isTokenExpired(currentUser.token)) {
  //     this.authService.logout(); // Clear invalid token
  //     this.router.navigate(['/login']);
  //   }
  // }
}
