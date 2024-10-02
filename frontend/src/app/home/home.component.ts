import { Component, OnInit } from '@angular/core';
import { Country } from "../models/country.model";
import { PartyService } from "../services/party.service";
import { Router } from "@angular/router";
import { NgForOf } from "@angular/common";
import { environment } from "../../environments/environment"; // Assuming you have an environment file

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [NgForOf],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {
  countries: Country[] = [];
  countryRows: Country[][] = [];

  constructor(private partyService: PartyService, private router: Router) {}

  ngOnInit(): void {
    this.loadCountries();
  }

  loadCountries(): void {
    this.partyService.getCountries().subscribe({
      next: (countries: Country[]) => {
        this.countries = countries;
        this.organizeCountriesInRows();
      },
      error: (error) => {
        console.error('Error loading countries', error);
      },
      complete: () => {
        console.log('Countries loading complete');
      }
    });
  }

  // Organize countries into rows of 6
  organizeCountriesInRows(): void {
    const rowSize = 6;
    for (let i = 0; i < this.countries.length; i += rowSize) {
      this.countryRows.push(this.countries.slice(i, i + rowSize));
    }
  }

  // Get the local flag path for each country
  getFlagPath(countryCode: string): string {
    return `${environment.localDataPath}flags/${countryCode}.png`;
  }

  // Navigate to the country's detail page
  goToCountry(countryCode: string): void {
    this.router.navigate(['/country', countryCode]);
  }
}
