import {
  AfterViewInit, booleanAttribute,
  Component,
  ElementRef,
  EventEmitter,
  HostListener,
  Input, OnChanges,
  Output,
  ViewChild
} from '@angular/core';
import {Party} from "../models/party.model";
import {ViewsGraphComponent} from "../views-graph/views-graph.component";
import {NgClass, NgForOf, NgIf} from "@angular/common";
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
  @Input({transform: booleanAttribute}) showLastPollInfo: boolean = true;
  @ViewChild('supportGraphContainer') supportGraphContainer!: ElementRef;
  @ViewChild('supportGraph') supportGraph!: ElementRef;
  @Output() partySelected = new EventEmitter<string>();

  maxNumber: number = 0;
  sortedParties: (Party & { support?: number, previousSupport?: number, leftIcons: boolean, rightIcons: boolean })[] = []; // Add optional support field
  chesEuParties: (Party & { support?: number })[] = [];  // Array for parties with CHES_EU

  isDesktop: boolean = true;
  showArea: boolean = false;

  constructor() {
  }

  ngOnChanges() {
    this.updateIsDesktop();
    this.maxNumber = 0;
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

    this.supportGraphContainer.nativeElement.style.setProperty('--num-ch-eu-parties', this.chesEuParties.length);
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
        if (this.supportData.length === 2 && this.supportData[1].results.some(oldPoll => oldPoll.partyId == result.partyId)) {
          let previousSupport = this.supportData[1].results.find(oldPoll => oldPoll.partyId == result.partyId)!.value;
          this.maxNumber = Math.max(this.maxNumber, result.value, previousSupport);
          return { ...this.parties.find(p => p.id == result.partyId)!, support: result.value, previousSupport: previousSupport, leftIcons: !this.isDesktop, rightIcons: !this.isDesktop };
        }
        this.maxNumber = Math.max(this.maxNumber, result.value);
        return { ...this.parties.find(p => p.id == result.partyId)!, support: result.value, leftIcons: !this.isDesktop, rightIcons: !this.isDesktop };
      });
      if (this.supportData[0].area) {
        this.showArea = true;
      }
    } else {
      // Use MP numbers for visualization
      this.sortedParties = this.parties.map(party => {
        return { ...party, leftIcons : !this.isDesktop, rightIcons : !this.isDesktop};
      });
      this.maxNumber = Math.max(...this.sortedParties.map(p => p.mp!));
    }

    // Sorting mechanism
    this.sortedParties = this.sortedParties
      .filter(p => p.cheS_Economy !== null && p.cheS_Progress !== null && p.cheS_Liberal !== null && p.cheS_EU !== null)
      .sort((a, b) => this.calculateChesScore(a) - this.calculateChesScore(b))
      .concat(this.sortedParties.filter(p => p.cheS_Economy === null || p.cheS_Progress === null || p.cheS_Liberal === null || p.cheS_EU === null)
        .sort((a, b) => this.supportData && this.supportData.length > 0 ? (b.support ?? 0) - (a.support ?? 0) : b.mp! - a.mp!));

    this.sortedParties[0].leftIcons = true;
    this.sortedParties[this.sortedParties.length-1].rightIcons = true;

    // Filter parties with CHES_EU
    this.chesEuParties = this.sortedParties.filter(p => p.cheS_EU !== null);
    if (this.supportGraphContainer && this.supportGraphContainer.nativeElement) {
      this.supportGraphContainer.nativeElement.style.setProperty('--num-ch-eu-parties', this.chesEuParties.length);
    }
  }

  calculateChesScore(party: Party): number {
    const economy = Array.isArray(party.cheS_Economy) ? (Number(party.cheS_Economy[0]) + Number(party.cheS_Economy[1])) / 2 : Number(party.cheS_Economy);
    const progress = Array.isArray(party.cheS_Progress) ? (Number(party.cheS_Progress[0]) + Number(party.cheS_Progress[1])) / 2 : Number(party.cheS_Progress);
    const liberal = Array.isArray(party.cheS_Liberal) ? (Number(party.cheS_Liberal[0]) + Number(party.cheS_Liberal[1])) / 2 : Number(party.cheS_Liberal);
    const eu = Array.isArray(party.cheS_EU) ? (Number(party.cheS_EU[0]) + Number(party.cheS_EU[1])) / 2 : Number(party.cheS_EU);
    return (economy + progress + liberal) * (4 + eu);
  }

  getPartyHeight(party: Party & { support?: number, previousSupport?: number}, previousPoll: boolean): string {
    let partyValue: number;
    if (previousPoll) {
      partyValue = party.previousSupport || 0;
    } else {
      partyValue = this.supportData.length > 0 ? party.support ?? 0 : party.mp ?? 0;
    }
    // Calculate percentage height relative to the max value
    const heightPercentage = (partyValue / this.maxNumber) * 100;

    return `${heightPercentage}%`;  // Return as a percentage to set in the inline style
  }

  getPartyColor(party: Party, previousPoll: boolean): string {
    if (party.groups && party.groups.size > 0) {
      let colors: string[];
      if (previousPoll) {
        colors = Array.from(party.groups).sort((a, b) => {return a.id - b.id})
          .map(group => `rgba(${group.r}, ${group.g}, ${group.b}, 0.3), rgb(${group.r}, ${group.g}, ${group.b}, 0.3)`);
      } else {
        colors = Array.from(party.groups).sort((a, b) => {return a.id - b.id})
          .map(group => `rgb(${group.r}, ${group.g}, ${group.b}), rgb(${group.r}, ${group.g}, ${group.b})`);
      }
      // If there are multiple colors, return a linear gradient
      return `linear-gradient(to right, ${colors.join(', ')})`;
    }

    // If no group, return gray
    if (previousPoll) {
      return 'lightgray';
    }
    return 'gray';
  }


  getPartyGroupAcronyms(party: Party): string {
    if (party.groups && party.groups.size > 0) {
      return Array.from(party.groups).map(group => group.acronym).join('/');
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
