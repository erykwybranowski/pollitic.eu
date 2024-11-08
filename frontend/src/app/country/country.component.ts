import {Component, HostListener, OnInit} from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { PartyService } from '../services/party.service';
import { Party } from '../models/party.model';
import {NgForOf, NgIf, NgStyle} from '@angular/common';
import { ViewsGraphComponent } from '../views-graph/views-graph.component';
import { SupportGraphComponent } from '../support-graph/support-graph.component';
import {PollingGraphComponent} from "../polling-graph/polling-graph.component";

@Component({
  selector: 'app-country',
  standalone: true,
  imports: [
    NgForOf,
    NgIf,
    ViewsGraphComponent,
    SupportGraphComponent,
    NgStyle,
    PollingGraphComponent,
  ],
  templateUrl: './country.component.html',
  styleUrls: ['./country.component.scss'],
})
export class CountryComponent implements OnInit {
  countryCode: string = '';
  countryName: string = '';
  parties: Party[] = [];
  screenWidth: number = window.innerWidth;

  constructor(private route: ActivatedRoute, private partyService: PartyService) {
    this.onResize();
  }

  ngOnInit(): void {
    this.countryCode = this.route.snapshot.paramMap.get('countryCode') || '';
    this.loadParties();
    this.loadCountryName();
  }

  @HostListener('window:resize', [])
  onResize() {
    this.screenWidth = window.innerWidth;
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
      },
    });
  }

  getPartiesForGraph(): Party[] {
    const subPartyIds = new Set<number>();

    const partiesWithMPs = this.parties.filter(p => p.mp != null);

    const collectSubPartyIds = (party: Party) => {
      if (party.subParties && party.subParties.length > 0) {
        party.subParties.forEach(subParty => {
          subPartyIds.add(subParty.id);
          collectSubPartyIds(subParty); // Recursively collect subparties
        });
      }
    };

    partiesWithMPs.forEach(party => {
      collectSubPartyIds(party);
    });

    return partiesWithMPs.filter(party => !subPartyIds.has(party.id));
  }

  getPartiesForPolls(): Party[] {
    return this.parties;
  }

  getPartiesForList(): (Party & { subLevel: number })[] {
    const subPartyIds = new Set<number>();

    const partiesWithMPs = this.parties.filter(p => p.mp != null);

    const collectSubPartyIds = (party: Party) => {
      if (party.subParties && party.subParties.length > 0) {
        party.subParties.forEach(subParty => {
          subPartyIds.add(subParty.id);
          collectSubPartyIds(subParty); // Recursively collect subparties
        });
      }
    };

    partiesWithMPs.forEach(party => {
      collectSubPartyIds(party);
    });

    const topLevelParties = partiesWithMPs.filter(party => !subPartyIds.has(party.id)).sort((a, b) => (b.mp || 0) - (a.mp || 0));

    const sortedParties: (Party & { subLevel: number })[] = [];

    // Recursive function to traverse parties and subparties
    const collectParties = (party: Party, subLevel: number) => {
      // Push current party with its subLevel
      sortedParties.push({ ...party, subLevel });

      // If the party has subparties, sort them by MP and recurse
      if (party.subParties && party.subParties.length > 0) {
        const sortedSubParties = party.subParties.sort((a, b) => (b.mp || 0) - (a.mp || 0));
        sortedSubParties.forEach(subParty => collectParties(subParty, subLevel + 1));
      }
    };

    topLevelParties.forEach(party => collectParties(party, 0));

    return sortedParties;
  }

  getPartyColorGradient(party: Party): string {
    if (party.group && party.group.size > 0) {
      const colors = Array.from(party.group).map(group => `rgb(${group.color.R}, ${group.color.G}, ${group.color.B})`);

      // If there is only one color, return that color as a solid background
      if (colors.length === 1) {
        return colors[0];
      }

      // If there are multiple colors, return a linear gradient
      return `linear-gradient(${colors.join(', ')})`;
    }

    // If no group, return gray
    return 'gray';
  }

  loadCountryName() {
    this.partyService.getCountries().subscribe(countries => {
      this.countryName = countries.find(c => c.countryCode === this.countryCode)?.name || '';
    });
  }

  getPartyGroups(party: Party): string {
    return party.group ? "Grupa: " + Array.from(party.group).map(g => g.acronym).join(', ') : '-';
  }

  getRoles(party: Party): string {
    if (party.role && party.role.size > 0) {
      return Array.from(party.role).join('/');
    }
    return 'Opozycja';
  }

  scrollToParty(partyAcronym: string): void {
    const targetElement  = document.getElementById(partyAcronym);
    if (targetElement) {
      const elementPosition = targetElement.getBoundingClientRect().top;
      const offsetPosition = elementPosition + window.pageYOffset - (window.innerHeight / 2 - targetElement.offsetHeight / 2);

      window.scrollTo({
        top: offsetPosition,
        behavior: 'smooth'
      });
    }
  }

  // Calculate margin-left based on sublevel and screen size
  calculateMarginLeft(subLevel: number): string {
    const baseMargin = this.screenWidth <= 768 ? 30 : 50; // Use a smaller margin for mobile
    return `${subLevel * baseMargin}px`;
  }
}
