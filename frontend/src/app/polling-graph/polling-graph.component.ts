import {Component, Input, OnInit} from '@angular/core';
import {PartyService} from "../services/party.service";
import {Poll} from "../models/poll.model";
import {Party} from "../models/party.model";
import {DatePipe, NgForOf} from "@angular/common";

@Component({
  selector: 'app-polling-graph',
  standalone: true,
  imports: [
    DatePipe,
    NgForOf
  ],
  templateUrl: './polling-graph.component.html',
  styleUrl: './polling-graph.component.scss'
})
export class PollingGraphComponent implements OnInit {
  @Input() countryCode: string = '';
  @Input() parties: Party[] = [];
  polls: Poll[] = [];

  constructor(private partyService: PartyService) {}

  ngOnInit(): void {
    this.loadPolls();
  }

  private loadPolls(): void {
    this.partyService.getPolls(this.countryCode, this.parties).subscribe(polls => {
      this.polls = polls;
    });
  }
}
