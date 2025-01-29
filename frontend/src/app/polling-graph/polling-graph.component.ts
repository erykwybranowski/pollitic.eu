import { Component, Input, OnInit } from '@angular/core';
import { Poll } from '../models/poll.model';
import { Party } from '../models/party.model';
import { Chart, registerables } from 'chart.js';
import { NgZone } from '@angular/core';
import {Group} from "../models/group.model";

@Component({
  selector: 'app-polling-graph',
  standalone: true,
  templateUrl: './polling-graph.component.html',
  styleUrls: ['./polling-graph.component.scss']
})
export class PollingGraphComponent implements OnInit {
  @Input() countryCode: string = '';
  @Input() parties: Party[] = [];
  @Input() polls: Poll[] = [];
  chart: Chart | null = null;
  colorsUsed: Record<string, number> = {};
  partyColors: Record<string, string> = {};

  constructor(private ngZone: NgZone) {
    Chart.register(...registerables);
  }

  ngOnInit(): void {
    this.createGraph();
  }

  private createGraph(): void {
    if (this.chart) {
      this.chart.destroy();
    }

    // Dynamically adjust canvas height
    const canvasElement = document.getElementById('pollingGraph') as HTMLCanvasElement;
    if (canvasElement) {
      const screenWidth = window.innerWidth;
      canvasElement.height = Math.max(500, Math.min(screenWidth / 2, 500)); // Adjust these values as needed
    }

    const partySupportOverTime = this.processPolls();
    const sortedParties = Object.keys(partySupportOverTime)
      .filter((party) => partySupportOverTime[party].some((entry) => entry.value > 0)) // Exclude parties with no non-zero values
      .sort((a, b) => {
        const groupCountA = this.getPartyByAcronym(a)?.groups?.size || 0;
        const groupCountB = this.getPartyByAcronym(b)?.groups?.size || 0;
        return groupCountA - groupCountB; // Sort descending by group size
      });

    this.getPartyColor(sortedParties);

    const datasets = sortedParties
      .sort((a, b) => {
        const averageA = this.average(partySupportOverTime[a].map((entry) => entry.value).filter(entry => entry > 0));
        const averageB = this.average(partySupportOverTime[b].map((entry) => entry.value).filter(entry => entry > 0));
        return averageA - averageB;
      })
      .map((party) => ({
        label: party,
        data: partySupportOverTime[party].map((entry) => (entry.value > 0 ? entry.value : null)),
        borderColor: this.partyColors[party],
        spanGaps: true,
        fill: false,
      }));

    const labels = this.generateMonthLabels();

    this.ngZone.runOutsideAngular(() => {
      this.chart = new Chart('pollingGraph', {
        type: 'line',
        data: {
          labels,
          datasets,
        },
        options: {
          responsive: true,
          plugins: {
            legend: {
              position: 'bottom',
              reverse: true,
            },
          },
          scales: {
            x: {
              title: { display: true, text: 'Miesiąc' },
            },
            y: {
              title: { display: true, text: 'Poparcie (%)' },
              beginAtZero: true,
            },
          },
        },
      });
    });
  }

  private processPolls(): Record<string, { month: string; value: number }[]> {
    const startDate = new Date();
    startDate.setFullYear(startDate.getFullYear() - 5);
    startDate.setDate(1);

    const partySupport: Record<string, { [month: string]: number[] }> = {};

    this.polls.forEach((poll) => {
      const pollMonth = poll.finishDate.toISOString().slice(0, 7); // Format: YYYY-MM
      poll.results.forEach((result) => {
        // Generate the key for the partySupport record
        const partyKey = this.parties.find(p => p.id == result.partyId)?.acronym;

        if (partyKey) {
          // Initialize nested structures if they don't exist
          if (!partySupport[partyKey]) {
            partySupport[partyKey] = {};
          }
          if (!partySupport[partyKey][pollMonth]) {
            partySupport[partyKey][pollMonth] = [];
          }

          // Add the value to the corresponding month
          partySupport[partyKey][pollMonth].push(result.value);
        }
      });
    });

    // Average values for each month and prepare final data
    const result: Record<string, { month: string; value: number }[]> = {};
    Object.keys(partySupport).forEach(party => {
      result[party] = this.generateMonthLabels().map(month => ({
        month,
        value: this.average(partySupport[party][month] || [])
      }));
    });
    return result;
  }

  private generateMonthLabels(): string[] {
    const labels: string[] = [];
    const startDate = new Date();
    startDate.setFullYear(startDate.getFullYear() - 5);
    startDate.setDate(1);

    const currentDate = new Date();
    while (startDate <= currentDate) {
      labels.push(startDate.toISOString().slice(0, 7)); // Format: YYYY-MM
      startDate.setMonth(startDate.getMonth() + 1);
    }
    return labels;
  }

  private average(values: number[]): number {
    if (values.length === 0) return 0;
    const rawAverage = values.reduce((sum, value) => sum + value, 0) / values.length;
    return Math.round(rawAverage * 10) / 10;
  }


  private getPartyColor(parties: string[]): void {
    for (let party of parties) {
      let groups = new Set<Group>();
      if (party.includes("+")) {
        party.split("+").forEach((acronym) => {
          let groupSet = this.parties.find((party) => party.acronym === acronym)?.groups;
          if (groupSet) {
            groupSet.forEach(group => {
              if (!groups.has(group)) groups.add(group);
            })
          }
        });
      } else {
        let groupSet = this.parties.find((p) => p.acronym === party)?.groups;
        if (groupSet) {
          groupSet.forEach(group => {
            if (!groups.has(group)) groups.add(group);
          })
        }
      }

      if (groups && groups.size > 0) {
        let selectedGroup: Group = groups.values().next().value;
        if (groups.size > 1) {
          selectedGroup = Array.from(groups).reduce((leastUsedGroup : Group, currentGroup : Group) => {
            const currentUsage = this.colorsUsed[currentGroup.acronym] || 0;
            const leastUsage = leastUsedGroup ? this.colorsUsed[leastUsedGroup.acronym] || 0 : Infinity;

            return currentUsage < leastUsage ? currentGroup : leastUsedGroup;
          });
        }
        const R = selectedGroup.r;
        const G = selectedGroup.g;
        const B = selectedGroup.b;
        this.colorsUsed[selectedGroup.acronym] = this.colorsUsed[selectedGroup.acronym] ? this.colorsUsed[selectedGroup.acronym] + 1 : 1;
        this.partyColors[party] = `rgb(${R}, ${G}, ${B})`;
      } else {
        this.partyColors[party] = 'gray'; // Default color for parties with no group
      }
    }
  }

  private getPartyByAcronym(acronym: string): Party | undefined {
    return this.parties.find((party) => party.acronym === acronym);
  }

}
