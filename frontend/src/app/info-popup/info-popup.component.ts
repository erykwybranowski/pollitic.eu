import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { PartyService } from '../services/party.service';
import { Group } from '../models/group.model';
import {NgForOf, NgIf, NgStyle} from "@angular/common";
import {ViewsGraphComponent} from "../views-graph/views-graph.component";

@Component({
  selector: 'app-info-popup',
  standalone: true,
  templateUrl: './info-popup.component.html',
  imports: [
    NgStyle,
    NgForOf,
    NgIf,
    ViewsGraphComponent
  ],
  styleUrl: './info-popup.component.scss'
})
export class InfoPopupComponent implements OnInit {
  @Output() closed = new EventEmitter<void>();
  groups: Group[] = [];
  exampleParty: any;

  constructor(private partyService: PartyService) { }

  ngOnInit(): void {
    this.partyService.getGroups().subscribe((groups) => {
      this.groups = groups;
    });

    this.partyService.getParties("pl").subscribe(parties => {
      this.exampleParty = parties.find(p => p.acronym === "KO");
    });
  }

  close(): void {
    this.closed.emit();
  }

  getRgbString(group: Group): string {
    return `rgb(${group.r}, ${group.g}, ${group.b})`;
  }

  getGroupDescription(acronym: string): string {
    switch (acronym) {
      case "LEFT":
        return '"Lewica" - grupa partii lewicowych i skrajnie lewicowych. Zawiera partie socjaldemokratyczne, socjalistyczne, a także komunistyczne.';
      case "S&D":
        return '"Postępowy Sojusz Socjalistów i Demokratów" - grupa partii centro-lewicowych. Zawiera partie socjalliberalne i socjaldemokratyczne.';
      case "GREENS":
        return '"Zieloni – Wolny Sojusz Europejski" - grupa partii Zielonych i regionalistycznych. Zawiera głównie partie socjalliberalne i socjaldemokratyczne.';
      case "RE":
        return '"Odnówmy Europę" - grupa partii liberalnych i centrowych. Zawiera głównie partie socjalliberalne i liberalne.';
      case "EPP":
        return '"Europejska Partia Ludowa" - grupa partii centro-prawicowych. Zawiera głównie partie liberalno-konserwatywne i chrześcijańsko-demokratyczne.';
      case "ECR":
        return '"Europejscy Konserwatyści i Reformatorzy" - grupa partii prawicowych. Zawiera głównie partie narodowo-konserwatywne.';
      case "PfE":
        return '"Patrioci za Europą" - grupa partii prawicowych i skrajnie prawicowych. Zawiera partie nacjonalistyczne i narodowo-konserwatywne.';
      case "ESN":
        return '"Europa Suwerennych Narodów" - grupa partii skrajnie prawicowych. Zawiera partie ultranacjonalistyczne, ultrakonserwatywne, a także neofaszystowskie.';
      default:
        return '';
    }
  }
}
