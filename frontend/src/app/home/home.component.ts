import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Country } from '../models/country.model';
import { NewestPoll } from '../models/poll.model';
import { Party } from '../models/party.model';
import { PartyService } from '../services/party.service';
import { environment } from '../../environments/environment';
import { forkJoin, Subscription } from 'rxjs';
import { ViewsGraphComponent } from '../views-graph/views-graph.component';
import { SupportGraphComponent } from '../support-graph/support-graph.component';
import { NgForOf, NgIf, NgStyle } from '@angular/common';

@Component({
  selector: 'app-home',
  standalone: true,
  templateUrl: './home.component.html',
  imports: [
    ViewsGraphComponent,
    SupportGraphComponent,
    NgStyle,
    NgForOf,
    NgIf
  ],
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit, OnDestroy {
  newestPolls: NewestPoll[] = [];
  countries: Country[] = [];
  parties: Party[] = [];
  governmentBlocks: { country: Country, government: Party | null }[] = [];
  includeSupportParties = false;

  currentNewestPollIndex: number = 0;
  autoSwitchTimer: any;
  subscriptions: Subscription[] = [];

  constructor(private partyService: PartyService, private router: Router) {}

  ngOnInit(): void {
    const sub = forkJoin({
      parties: this.partyService.getAllParties(),
      countries: this.partyService.getCountries()
    }).subscribe({
      next: result => {
        this.parties = result.parties;
        this.countries = result.countries;
        this.computeGovernmentBlocks();
      },
      error: error => console.error('Error loading parties and countries', error)
    });
    this.subscriptions.push(sub);

    this.loadNewestPolls();
    this.resetAutoSwitchTimer();
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(s => s.unsubscribe());
    if (this.autoSwitchTimer) {
      clearTimeout(this.autoSwitchTimer);
    }
  }

  loadNewestPolls(): void {
    const sub = this.partyService.getNewestPollsPerCountry().subscribe({
      next: (polls: NewestPoll[]) => {
        this.newestPolls = polls;
        this.currentNewestPollIndex = 0;
        this.resetAutoSwitchTimer();
      },
      error: (error: any) => {
        console.error('Error loading newest polls', error);
      }
    });
    this.subscriptions.push(sub);
  }

  computeGovernmentBlocks(): void {
    this.governmentBlocks = this.countries.map(country => {
      const countryParties = this.parties.filter(p => p.countryCode === country.countryCode && p.englishName != "Composite-party");
      const government = this.computeGovernment(countryParties);
      return { country, government };
    }).filter(block => block.government != null)
      .sort((a, b) => a.country.name.localeCompare(b.country.name));
  }

  computeGovernment(parties: Party[]): Party | null {
    const governmentRoles = ["Rząd"];
    if (this.includeSupportParties) {
      governmentRoles.push("Wsparcie");
    }
    const govParties = parties.filter(p =>
      p.role && Array.from(p.role).some(role => governmentRoles.includes(role))
    );
    if (govParties.length === 0) {
      return null;
    }
    let govGroups = new Set<any>();
    let govGroupsNames = new Set<string>();
    govParties.forEach(p => {
      if (p.groups) {
        p.groups.forEach(group => {
          if (!govGroupsNames.has(group.acronym)) {
            govGroups.add(group);
            govGroupsNames.add(group.acronym);
          }
        });
      }
    });
    let CHESData = this.partyService.getSubPartiesCHESData(govParties);
    const government: Party = {
      id: "0",
      acronym: "Koalicja: " + govParties
        .filter(p => p.role && Array.from(p.role).includes("Rząd"))
        .sort((a, b) => (b.mp || 0) - (a.mp || 0))
        .map(g => g.acronym)
        .join(", "),
      stringId: "Wsparcie: " + govParties
        .filter(p => p.role && Array.from(p.role).includes("Wsparcie"))
        .sort((a, b) => (b.mp || 0) - (a.mp || 0))
        .map(g => g.acronym)
        .join(", "),
      englishName: "Government",
      groups: govGroups,
      role: null,
      subParties: null,
      countryCode: govParties[0].countryCode,
      mp: govParties.reduce((sum, p) => sum + (p.mp || 0), 0),
      localName: null,
      cheS_EU: CHESData[0],
      cheS_Economy: CHESData[1],
      cheS_Progress: CHESData[2],
      cheS_Liberal: CHESData[3]
    };
    return government;
  }

  getCountryCodeForPoll(poll: NewestPoll): string {
    return poll.countryCode;
  }

  getCountryName(countryCode: string): string {
    const country = this.countries.find(c => c.countryCode.toLowerCase() === countryCode.toLowerCase());
    return country ? country.name : countryCode;
  }

  getFlagPath(countryCode: string): string {
    return `${environment.localDataPath}flags/${countryCode.toLowerCase()}.png`;
  }

  getPartiesForCountry(countryCode: string): Party[] {
    return this.parties.filter(p => p.countryCode === countryCode);
  }

  goToCountry(countryCode: string): void {
    this.router.navigate(['/country', countryCode]);
  }

  getPartyColorGradient(party: Party | null): string {
    if (party && party.groups && party.groups.size > 0) {
      const colors = Array.from(party.groups)
        .sort((a, b) => a.id - b.id)
        .map(group => `rgb(${group.r}, ${group.g}, ${group.b}), rgb(${group.r}, ${group.g}, ${group.b})`);
      return `linear-gradient(${colors.join(', ')})`;
    }
    return 'gray';
  }

  goToNextNewestPoll(): void {
    if (this.newestPolls.length > 0) {
      this.currentNewestPollIndex = (this.currentNewestPollIndex + 1) % this.newestPolls.length;
      this.resetAutoSwitchTimer();
    }
  }

  goToPreviousNewestPoll(): void {
    if (this.newestPolls.length > 0) {
      this.currentNewestPollIndex = (this.currentNewestPollIndex - 1 + this.newestPolls.length) % this.newestPolls.length;
      this.resetAutoSwitchTimer();
    }
  }

  resetAutoSwitchTimer(): void {
    if (this.autoSwitchTimer) {
      clearTimeout(this.autoSwitchTimer);
    }
    this.autoSwitchTimer = setTimeout(() => {
      this.goToNextNewestPoll();
    }, 5000);
  }
}
