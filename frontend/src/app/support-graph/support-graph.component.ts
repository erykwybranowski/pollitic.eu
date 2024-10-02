import {AfterViewInit, Component, ElementRef, Input, OnInit, ViewChild} from '@angular/core';
import {Party} from "../models/party.model";
import {PartyGraphComponent} from "../party-graph/party-graph.component";
import {NgForOf} from "@angular/common";

@Component({
  selector: 'app-support-graph',
  standalone: true,
  imports: [
    PartyGraphComponent,
    NgForOf
  ],
  templateUrl: './support-graph.component.html',
  styleUrl: './support-graph.component.scss'
})
export class SupportGraphComponent implements OnInit, AfterViewInit {
  @Input() parties: Party[] = []; // List of Party objects
  @Input() supportData: { acronym: string, support: number }[] = []; // List of support data pairs [acronym - support number]
  @ViewChild('supportGraphContainer') supportGraphContainer!: ElementRef;
  @ViewChild('supportGraph') supportGraph!: ElementRef;

  maxNumber: number = 0;
  sortedParties: (Party & { support?: number, leftIcons: boolean, rightIcons: boolean })[] = []; // Add optional support field

  ngOnInit() {
    console.log("START")
    console.log(this.parties)
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
  }

  processData() {
    if (this.supportData && this.supportData.length > 0) {
      // Use support numbers for visualization
      this.sortedParties = this.parties.map(party => {
        const supportEntry = this.supportData.find(support => support.acronym === party.acronym);
        return { ...party, support: supportEntry ? supportEntry.support : 0, leftIcons : false, rightIcons : false};
      });
      this.maxNumber = Math.max(...this.sortedParties.map(p => p.support ?? 0));
    } else {
      // Use MP numbers for visualization
      this.sortedParties = this.parties.map(party => {
        return { ...party, leftIcons : false, rightIcons : false};
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
  }

  calculateChesScore(party: Party): number {
    const economy = Array.isArray(party.CHES_Economy) ? (Number(party.CHES_Economy[0]) + Number(party.CHES_Economy[1])) / 2 : Number(party.CHES_Economy);
    const progress = Array.isArray(party.CHES_Progress) ? (Number(party.CHES_Progress[0]) + Number(party.CHES_Progress[1])) / 2 : Number(party.CHES_Progress);
    const liberal = Array.isArray(party.CHES_Liberal) ? (Number(party.CHES_Liberal[0]) + Number(party.CHES_Liberal[1])) / 2 : Number(party.CHES_Liberal);
    const eu = Array.isArray(party.CHES_EU) ? (Number(party.CHES_EU[0]) + Number(party.CHES_EU[1])) / 2 : Number(party.CHES_EU);
    return (economy + progress) * (8 + liberal + eu);
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
      const colors = Array.from(party.group).map(group => `rgb(${group.color.R}, ${group.color.G}, ${group.color.B})`);

      // If there is only one color, return that color as a solid background
      if (colors.length === 1) {
        return colors[0];
      }

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
    return 'Opozycja';
  }
}
