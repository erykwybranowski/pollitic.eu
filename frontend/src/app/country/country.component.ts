import {Component, OnInit} from '@angular/core';
import {Party} from "../models/party.model";
import {ActivatedRoute} from "@angular/router";
import {PartyService} from "../services/party.service";
import {NgForOf, NgIf} from "@angular/common";
import {PartyGraphComponent} from "../party-graph/party-graph.component";
import {SupportGraphComponent} from "../support-graph-component/support-graph-component.component";

@Component({
  selector: 'app-country',
  standalone: true,
  imports: [
    NgForOf,
    NgIf,
    PartyGraphComponent,
    SupportGraphComponent
  ],
  templateUrl: './country.component.html',
  styleUrl: './country.component.scss'
})
export class CountryComponent implements OnInit {
  countryCode: string = '';
  countryName: string = '';
  parties: Party[] = [];

  constructor(private route: ActivatedRoute, private partyService: PartyService) {}

  ngOnInit(): void {
    this.countryCode = this.route.snapshot.paramMap.get('countryCode') || '';
    this.loadParties();
    this.loadCountryName();
  }

  loadParties(): void {
    this.partyService.getParties(this.countryCode).subscribe({
      next: (parties: Party[]) => {
        this.parties = parties;
      },
      error: (error) => {
        console.error('Error loading parties', error);
      },
      complete: () => {
        console.log('Parties loading complete');
      }
    });
  }

  getPartiesWithMPs(): Party[] {
    const subPartyIds = new Set<number>();

    let partiesWithMPs = this.parties.filter(p => p.mp != null);

    // Recursively collect all subparties' IDs
    const collectSubPartyIds = (party: Party) => {
      if (party.subParties && party.subParties.length > 0) {
        party.subParties.forEach(subParty => {
          subPartyIds.add(subParty.id); // Add the subparty ID to the set
          collectSubPartyIds(subParty); // Recursively collect subparties of subparties
        });
      }
    };

    // Iterate over each top-level party and collect subparty IDs
    partiesWithMPs.forEach(party => {
      collectSubPartyIds(party);
    });

    // Filter out parties that are subparties (i.e., their ID is in the subPartyIds set)
    return partiesWithMPs.filter(party => !subPartyIds.has(party.id));
  }

  private loadCountryName() {
    this.partyService.getCountries().subscribe(countries => {
      this.countryName = countries.find(c => c.countryCode === this.countryCode)!.name;
    });
  }
}
