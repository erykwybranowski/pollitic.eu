import {Component, Input, OnInit} from '@angular/core';
import {PartyService} from "../services/party.service";
import {Poll} from "../models/poll.model";
import {Party} from "../models/party.model";
import {Chart, registerables} from 'chart.js';
import { NgZone } from '@angular/core';

@Component({
  selector: 'app-polling-graph',
  standalone: true,
  templateUrl: './polling-graph.component.html',
  styleUrls: ['./polling-graph.component.scss']
})
export class PollingGraphComponent implements OnInit {
  @Input() countryCode: string = '';
  @Input() parties: Party[] = [];
  polls: Poll[] = [];
  chart: Chart | null = null;

  constructor(private partyService: PartyService, private ngZone: NgZone) {
    Chart.register(...registerables);
  }

  ngOnInit(): void {
    this.loadPolls();
  }

  private loadPolls(): void {
    this.partyService.getPolls(this.countryCode, this.parties).subscribe(polls => {
      this.polls = polls;
      this.createGraph();
    });
  }

  private createGraph(): void {
    if (this.chart) {
      this.chart.destroy();
    }

    const partySupportOverTime = this.processPolls();
    const datasets = Object.keys(partySupportOverTime).map((party, index) => ({
      label: party,
      data: partySupportOverTime[party].map(entry => entry.value),
      borderColor: this.getPartyColor(index),
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
            },
          },
          scales: {
            x: {
              title: { display: true, text: 'Months' },
            },
            y: {
              title: { display: true, text: 'Support (%)' },
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

    this.polls.forEach(poll => {
      const pollMonth = poll.finishDate.toISOString().slice(0, 7); // Format: YYYY-MM
      poll.results.forEach(result => {
        if (!partySupport[result.party.acronym]) {
          partySupport[result.party.acronym] = {};
        }
        if (!partySupport[result.party.acronym][pollMonth]) {
          partySupport[result.party.acronym][pollMonth] = [];
        }
        partySupport[result.party.acronym][pollMonth].push(result.value);
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
    return values.reduce((sum, value) => sum + value, 0) / values.length;
  }

  private getPartyColor(index: number): string {
    const colors = ['#FF6384', '#36A2EB', '#FFCE56', '#4BC0C0', '#9966FF', '#FF9F40'];
    return colors[index % colors.length];
  }
}
