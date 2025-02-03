import {ChangeDetectorRef, Component, HostListener, OnInit} from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { PartyService } from '../services/party.service';
import { Party } from '../models/party.model';
import {NgForOf, NgIf, NgStyle} from '@angular/common';
import { ViewsGraphComponent } from '../views-graph/views-graph.component';
import { SupportGraphComponent } from '../support-graph/support-graph.component';
import {PollingGraphComponent} from "../polling-graph/polling-graph.component";
import {Group} from "../models/group.model";
import {FormsModule} from "@angular/forms";
import {Poll} from "../models/poll.model";
import {environment} from "../../environments/environment";

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
    FormsModule,
  ],
  templateUrl: './country.component.html',
  styleUrls: ['./country.component.scss'],
})
export class CountryComponent implements OnInit {
  countryCode: string = '';
  countryName: string = '';
  parties: Party[] = [];
  screenWidth: number = window.innerWidth;
  government: Party | null = null;
  includeSupportParties = false;
  showExcludedParties = false;
  polls: Poll[] = [];
  currentPollIndex = 0;

  constructor(private route: ActivatedRoute, private partyService: PartyService, private cdr: ChangeDetectorRef) {
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
        this.loadPolls();
        this.determineGovernment();
      },
      error: (error) => {
        console.error('Error loading parties', error);
      },
      complete: () => {
        console.log('Parties loading complete');
      },
    });
  }

  private loadPolls() {
    this.partyService.getPolls(this.countryCode, this.parties).subscribe((polls) => {
      this.polls = polls;
      console.log("Polls loading complete")

    });
  }

  protected determineGovernment() {
    const governmentRoles = ["Rząd"];
    if (this.includeSupportParties) {
      governmentRoles.push("Wsparcie");
    }

    const govParties = this.getPartiesForGraph().filter((p) => governmentRoles.some((role) => p.role?.has(role)));
    if (govParties.length > 0) {
      let govGroups = new Set<Group>();
      let govGroupsNames = new Set<string>();

      govParties.forEach((p) => {
        if (p.groups) {
          p.groups.forEach((group) => {
            if (!govGroupsNames.has(group.acronym)) {
              govGroups.add(group);
              govGroupsNames.add(group.acronym);
            }
          });
        }
      });

      let CHESData = this.partyService.getSubPartiesCHESData(govParties);

      this.government = {
        id: "0",
        acronym: "Koalicja: " + govParties
          .filter(p => p.role?.has("Rząd"))
          .sort((a, b) => (b.mp || 0) - (a.mp || 0))
          .map((g) => g.acronym)
          .join(", "),
        stringId: "Wsparcie: " + govParties
          .filter(p => p.role?.has("Wsparcie"))
          .sort((a, b) => (b.mp || 0) - (a.mp || 0))
          .map((g) => g.acronym)
          .join(", "),
        englishName: "Government",
        groups: govGroups,
        role: null,
        subParties: null,
        countryCode: "null",
        mp: null,
        localName: null,
        cheS_EU: CHESData[0],
        cheS_Economy: CHESData[1],
        cheS_Progress: CHESData[2],
        cheS_Liberal: CHESData[3],
      };
    }
  }

  getPartiesForGraph(): Party[] {
    const subPartyIds = new Set<string>();

    const partiesWithMPs = this.parties.filter(p => p.mp != null  && p.englishName != "Composite-party");

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

  getPolls(area?: string): Poll[] {
    if (area) {
      return this.polls.filter(p => p.area == area);
    }
    return this.polls;
  }

  getPartiesForList(excluded: boolean = false): (Party & { subLevel: number })[] {
    const subPartyIds = new Set<string>();

    let partiesWithMPs: Party[] = [];
    if (excluded) {
      partiesWithMPs = this.parties.filter(p => p.mp == null || p.mp == 0  && p.englishName != "Composite-party");
    } else {
      partiesWithMPs = this.parties.filter(p => p.mp != null && p.englishName != "Composite-party");
    }

    const collectSubPartyIds = (party: Party) => {
      if (party.subParties && party.subParties.length > 0) {
        party.subParties.forEach(subParty => {
          subPartyIds.add(subParty.id);
          collectSubPartyIds(this.parties.find(p => p.id == subParty.id)!); // Recursively collect subparties
        });
      }
    };

    partiesWithMPs.forEach(party => {
      collectSubPartyIds(party);
    });

    const filteredParties = partiesWithMPs.filter(party => !subPartyIds.has(party.id));
    let topLevelParties: Party[] = [];
    if (excluded) {
      topLevelParties = filteredParties.sort((a, b) => ((b.cheS_Liberal != null ? 2 : 0) + (b.groups != null && b.groups.size > 0 ? 1 : 0)) - ((a.cheS_Liberal != null ? 2 : 0) + (a.groups && a.groups.size > 0 != null ? 1 : 0)))
    } else {
      topLevelParties = filteredParties.sort((a, b) => (b.mp || 0) - (a.mp || 0));
    }

    const sortedParties: (Party & { subLevel: number })[] = [];

    // Recursive function to traverse parties and subparties
    const collectParties = (party: Party, subLevel: number) => {
      // Push current party with its subLevel
      sortedParties.push({ ...party, subLevel });

      // If the party has subparties, sort them by MP and recurse
      if (party.subParties && party.subParties.length > 0) {
        const sortedSubParties = party.subParties.map(sub => this.parties.find(p => p.id == sub.id)!).sort((a, b) => (b.mp || 0) - (a.mp || 0));
        sortedSubParties.forEach(subParty => collectParties(subParty, subLevel + 1));
      }
    };

    topLevelParties.forEach(party => collectParties(party, 0));

    return sortedParties;
  }

  getPartyColorGradient(party: Party): string {
    if (party.groups && party.groups.size > 0) {
      const colors = Array.from(party.groups).sort((a, b) => {return a.id - b.id})
        .map(group => `rgb(${group.r}, ${group.g}, ${group.b}), rgb(${group.r}, ${group.g}, ${group.b})`);

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
    return party.groups ? "Grupa: " + Array.from(party.groups).sort((a, b) => {return a.id - b.id}).map(g => g.acronym).join(', ') : '-';
  }

  getRoles(party: Party): string {
    if (party.role && party.role.size > 0) {
      return Array.from(party.role).join('/');
    }
    return 'Opozycja';
  }

  scrollToParty(partyAcronym: string): void {
    let targetElement  = document.getElementById(partyAcronym);
    if (targetElement) {
      this.goToTargetElement(targetElement);
    } else if (this.getPartiesForList(true).some(p => p.acronym == partyAcronym)) {
      this.showExcludedParties = true;
      this.cdr.detectChanges();

      targetElement = document.getElementById(partyAcronym);
      if (targetElement) {
        this.goToTargetElement(targetElement);
        }
    }
  }

  goToTargetElement(targetElement: HTMLElement): void {
    const elementPosition = targetElement.getBoundingClientRect().top;
    const offsetPosition = elementPosition + window.pageYOffset - (window.innerHeight / 2 - targetElement.offsetHeight / 2);

    window.scrollTo({
      top: offsetPosition,
      behavior: 'smooth'
    });
  }

  // Calculate margin-left based on sublevel and screen size
  calculateMarginLeft(subLevel: number): string {
    const baseMargin = this.screenWidth <= 768 ? 30 : 50; // Use a smaller margin for mobile
    return `${subLevel * baseMargin}px`;
  }

  hasSupport(): boolean {
    return this.parties.some(p => p.role?.has("Wsparcie")) || false;
  }

  getSupportData(number: number): Poll[] {
    let currentPoll = this.polls[number];
    let previousPoll: Poll | null = null;
    for (let i = number + 1; i < this.polls.length; i++) {
      if (this.polls[i].pollster == currentPoll.pollster && this.polls[i].area == currentPoll.area) {
        previousPoll = this.polls[i];
        break;
      }
    }
    let result: Poll[] = [currentPoll];
    if (previousPoll) {
      result.push(previousPoll);
    }
    return result;
    // return this.polls[number].results.map((result) => ({
    //   acronym: result.party.map(p => p.stringId).join("+"),
    //   support: Math.round(result.value * 10) / 10}))
  }

  goToPreviousPoll(): void {
    if (this.currentPollIndex > 0) {
      this.currentPollIndex--;
    }
  }

  goToNextPoll(): void {
    if (this.currentPollIndex < this.polls.length - 1) {
      this.currentPollIndex++;
    }
  }

  getFlagPath(countryCode: string): string {
    return `${environment.localDataPath}flags/${countryCode.toLowerCase()}.png`;
  }
}
