import {Component, OnInit} from '@angular/core';
import {Country} from "../models/country.model";
import {PartyService} from "../services/party.service";
import {Router} from "@angular/router";
import {NgForOf} from "@angular/common";

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    NgForOf
  ],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent implements OnInit {
  countries: Country[] = [];

  constructor(private partyService: PartyService, private router: Router) {}

  ngOnInit(): void {
    this.loadCountries();
  }

  loadCountries(): void {
    this.partyService.getCountries().subscribe({
      next: (countries: Country[]) => {
        this.countries = countries;
      },
      error: (error) => {
        console.error('Error loading countries', error);
      },
      complete: () => {
        console.log('Countries loading complete');
      }
    });
  }

  goToCountry(countryCode: string): void {
    this.router.navigate(['/country', countryCode]);
  }
}
