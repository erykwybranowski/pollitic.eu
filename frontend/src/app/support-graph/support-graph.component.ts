import {
  AfterViewInit,
  Component,
  ElementRef,
  EventEmitter,
  HostListener,
  Input, OnChanges,
  OnInit,
  Output,
  ViewChild
} from '@angular/core';
import {Party} from "../models/party.model";
import {ViewsGraphComponent} from "../views-graph/views-graph.component";
import {NgClass, NgForOf, NgIf} from "@angular/common";
import {Group} from "../models/group.model";
import {PartyService} from "../services/party.service";
import {Poll} from "../models/poll.model";

@Component({
  selector: 'app-support-graph',
  standalone: true,
  imports: [
    ViewsGraphComponent,
    NgForOf,
    NgIf,
    NgClass
  ],
  templateUrl: './support-graph.component.html',
  styleUrl: './support-graph.component.scss'
})
export class SupportGraphComponent implements OnChanges, AfterViewInit {
  @Input() parties: Party[] = []; // List of Party objects
  @Input() supportData: Poll[] = []; // List of support data pairs [acronym - support number]
  @ViewChild('supportGraphContainer') supportGraphContainer!: ElementRef;
  @ViewChild('supportGraph') supportGraph!: ElementRef;
  @Output() partySelected = new EventEmitter<string>();

  maxNumber: number = 0;
  sortedParties: (Party & { support?: number, leftIcons: boolean, rightIcons: boolean })[] = []; // Add optional support field
  chesEuParties: (Party & { support?: number })[] = [];  // Array for parties with CHES_EU

  isDesktop: boolean = true;

  constructor(private partyService: PartyService) {
  }

  ngOnChanges() {
    this.updateIsDesktop();
    this.processData();
  }

  ngAfterViewInit() {
    const containerWidth = this.supportGraphContainer.nativeElement.offsetWidth;
    const graphWidth = this.supportGraph.nativeElement.scrollWidth;

    // If the graph is smaller than the container, center it
    if (graphWidth < containerWidth) {
      this.supportGraphContainer.nativeElement.style.justifyContent = 'center';
    } else {
      this.supportGraphContainer.nativeElement.style.justifyContent = 'flex-start';
    }

    const numChesEuParties = this.chesEuParties.length;
    this.supportGraphContainer.nativeElement.style.setProperty('--num-ch-eu-parties', numChesEuParties);
  }

  @HostListener('window:resize', [])
  onResize() {
    this.updateIsDesktop();
  }

  updateIsDesktop() {
    this.isDesktop = window.innerWidth >= 768; // Define 768px as the desktop threshold
  }

  processData() {
    if (this.supportData && this.supportData.length > 0) {
      this.sortedParties = this.supportData[0].results.map(result => {
        if (result.party.length === 1) {
          let party = result.party[0];
          return { ...party, support: result.value, leftIcons: !this.isDesktop, rightIcons: !this.isDesktop };
        }

        let subParties: Party[] = [];
        let groups: Set<Group> = new Set<Group>();
        let roles: Set<string> = new Set<string>();

        for (let party of result.party) {
          subParties.push(party);
          if (party.group && party.group.size > 0) {
            party.group.forEach(g => groups.add(g));
          }
          if (party.role && party.role.size > 0) {
            party.role.forEach(r => roles.add(r));
          }
        }

        let CHESData = this.partyService.getSubPartiesCHESData(subParties);

        return {
          id: 0,
          acronym: subParties.map(p => p.acronym).join("/"),
          stringId: subParties.map(p => p.acronym).join("/"),
          englishName: subParties.map(p => p.acronym).join("/"),
          group: groups,
          role: roles,
          subParties: null,
          countryCode: null,
          mp: null,
          localName: null,
          CHES_EU: CHESData[0],
          CHES_Economy: CHESData[1],
          CHES_Progress: CHESData[2],
          CHES_Liberal: CHESData[3],
          support: result.value,
          leftIcons: !this.isDesktop,
          rightIcons: !this.isDesktop,
        };
      });
      this.maxNumber = Math.max(...this.sortedParties.map(p => p.support ?? 0));
    } else {
      // Use MP numbers for visualization
      this.sortedParties = this.parties.map(party => {
        return { ...party, leftIcons : !this.isDesktop, rightIcons : !this.isDesktop};
      });
      this.maxNumber = Math.max(...this.sortedParties.map(p => p.mp!));
    }

    // Sorting mechanism
    this.sortedParties = this.sortedParties
      .filter(p => p.CHES_Economy !== null && p.CHES_Progress !== null && p.CHES_Liberal !== null && p.CHES_EU !== null)
      .sort((a, b) => this.calculateChesScore(a) - this.calculateChesScore(b))
      .concat(this.sortedParties.filter(p => p.CHES_Economy === null || p.CHES_Progress === null || p.CHES_Liberal === null || p.CHES_EU === null)
        .sort((a, b) => this.supportData && this.supportData.length > 0 ? (b.support ?? 0) - (a.support ?? 0) : b.mp! - a.mp!));

    this.sortedParties[0].leftIcons = true;
    this.sortedParties[this.sortedParties.length-1].rightIcons = true;

    // Filter parties with CHES_EU
    this.chesEuParties = this.sortedParties.filter(p => p.CHES_EU !== null);
  }

  calculateChesScore(party: Party): number {
    const economy = Array.isArray(party.CHES_Economy) ? (Number(party.CHES_Economy[0]) + Number(party.CHES_Economy[1])) / 2 : Number(party.CHES_Economy);
    const progress = Array.isArray(party.CHES_Progress) ? (Number(party.CHES_Progress[0]) + Number(party.CHES_Progress[1])) / 2 : Number(party.CHES_Progress);
    const liberal = Array.isArray(party.CHES_Liberal) ? (Number(party.CHES_Liberal[0]) + Number(party.CHES_Liberal[1])) / 2 : Number(party.CHES_Liberal);
    const eu = Array.isArray(party.CHES_EU) ? (Number(party.CHES_EU[0]) + Number(party.CHES_EU[1])) / 2 : Number(party.CHES_EU);
    return (economy + progress + liberal) * (4 + eu);
  }

  getPartyHeight(party: Party  & { support?: number }): string {
    const maxValue = Math.max(...this.sortedParties.map(p => this.supportData.length > 0 ? p.support ?? 0 : p.mp ?? 0));
    const partyValue = this.supportData.length > 0 ? party.support ?? 0 : party.mp ?? 0;

    // Calculate percentage height relative to the max value
    const heightPercentage = (partyValue / maxValue) * 100;

    return `${heightPercentage}%`;  // Return as a percentage to set in the inline style
  }

  getPartyColor(party: Party): string {
    if (party.group && party.group.size > 0) {
      const colors = Array.from(party.group).sort((a,b) => {return a.id - b.id})
        .map(group => `rgb(${group.color.R}, ${group.color.G}, ${group.color.B}), rgb(${group.color.R}, ${group.color.G}, ${group.color.B})`);

      // If there are multiple colors, return a linear gradient
      return `linear-gradient(to right, ${colors.join(', ')})`;
    }

    // If no group, return gray
    return 'gray';
  }


  getPartyGroupAcronyms(party: Party): string {
    if (party.group && party.group.size > 0) {
      return Array.from(party.group).map(group => group.acronym).join('/');
    }
    return '-';
  }

  getRoles(party: Party): string {
    if (party.role && party.role.size > 0) {
      return Array.from(party.role).join('/');
    }
    return '-';
  }

  onPartyClick(partyAcronym: string): void {
    this.partySelected.emit(partyAcronym);
  }

  formatDate(date: Date): string {
    const day = date.getDate().toString().padStart(2, '0');
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const year = date.getFullYear();
    return `${day}.${month}.${year}`;
  }
}
