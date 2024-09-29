import {Component, OnInit} from '@angular/core';
import {Party} from "../models/party.model";
import {ActivatedRoute} from "@angular/router";
import {PartyService} from "../services/party.service";
import {NgForOf, NgIf} from "@angular/common";
import {PartyGraphComponent} from "../party-graph/party-graph.component";

@Component({
  selector: 'app-country',
  standalone: true,
  imports: [
    NgForOf,
    NgIf,
    PartyGraphComponent
  ],
  templateUrl: './country.component.html',
  styleUrl: './country.component.scss'
})
export class CountryComponent implements OnInit {
  countryCode: string = '';
  parties: Party[] = [];

  constructor(private route: ActivatedRoute, private partyService: PartyService) {}

  ngOnInit(): void {
    this.countryCode = this.route.snapshot.paramMap.get('countryCode') || '';
    this.loadParties();
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
}
