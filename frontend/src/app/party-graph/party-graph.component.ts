import {Component, Input, OnInit} from '@angular/core';
import {Party} from "../models/party.model";
import {NgClass, NgForOf} from "@angular/common";

@Component({
  selector: 'app-party-graph',
  standalone: true,
  imports: [
    NgClass,
    NgForOf
  ],
  templateUrl: './party-graph.component.html',
  styleUrl: './party-graph.component.scss'
})
export class PartyGraphComponent implements OnInit {
  @Input() party!: Party;

  economySquares: any[] = [];
  economyConnectors: any[] = [];

  liberalSquares: any[] = [];
  liberalConnectors: any[] = [];

  progressSquares: any[] = [];
  progressConnectors: any[] = [];

  euSquares: any[] = [];
  euConnectors: any[] = [];

  ngOnInit(): void {
    console.log(this.party.acronym)
    this.generateGraph();
  }

  generateGraph(): void {
    this.createValueGraph('CHES_Economy', this.economySquares, this.economyConnectors);
    this.createValueGraph('CHES_Liberal', this.liberalSquares, this.liberalConnectors);
    this.createValueGraph('CHES_Progress', this.progressSquares, this.progressConnectors);
    this.createValueGraph('CHES_EU', this.euSquares, this.euConnectors);
  }

  createValueGraph(field: string, squaresArray: any[], connectorsArray: any[]): void {
    let chesValue = null;
    if (field === 'CHES_Economy') chesValue = this.party.CHES_Economy;
    else if (field === 'CHES_Liberal') chesValue = this.party.CHES_Liberal;
    else if (field === 'CHES_Progress') chesValue = this.party.CHES_Progress;
    else if (field === 'CHES_EU') chesValue = this.party.CHES_EU;

    console.log("   " + field + " " + chesValue)
    if (Array.isArray(chesValue)) {
      console.log("          1")
      // If CHES value is a range
      this.addSquare(squaresArray, chesValue[0], chesValue[0] < 0 ? 'blue' : (chesValue[0] == 0 ? "center" : "yellow"));  // First value
      this.addSquare(squaresArray, chesValue[1], chesValue[1] < 0 ? 'blue' : (chesValue[1] == 0 ? "center" : "yellow"));  // Second value
      this.addRangeConnector(connectorsArray, chesValue[0], chesValue[1]);  // Updated range connector logic
    } else if (chesValue && chesValue != 0) {
      console.log("          2")
      // If CHES value is a single value
      this.addSquare(squaresArray, chesValue, chesValue > 0 ? 'yellow' : 'blue');
      this.addSingleConnector(connectorsArray, 0, chesValue);  // Updated connector logic
    } else if (chesValue == 0) {
      console.log("          3")
      // If CHES value is 0
      this.addSquare(squaresArray, 0, 'center');
    }
  }

  addSquare(squaresArray: any[], value: number, colorClass: string): void {
    const position = 300 + value * 100;  // 300 is the center, move left/right based on value
    squaresArray.push({ class: colorClass, position });
  }

  addSingleConnector(connectorsArray: any[], value1: number, value2: number): void {
    if (Math.abs(value1 - value2) === 1 || value1 - value2 === 0) return; // No connector needed if touching
    const startPosition = Math.min(value1, value2) * 100 + 400;
    const endPosition = Math.max(value1, value2) * 100 + 300;
    const connectorWidth = endPosition - startPosition;
    connectorsArray.push({
      class: value1 * value2 < 0 ? 'gradient' : (value2 > 0 ? 'yellow' : 'blue'), // Adjust class based on value range
      startPosition,
      width: connectorWidth
    });
  }

  addRangeConnector(connectorsArray: any[], value1: number, value2: number): void {
    if (Math.abs(value1 - value2) === 1) {
      this.addSingleConnector(connectorsArray, 0, Math.abs(value1) < Math.abs(value2) ? value1 : value2);
      return;
    } // No connector if values touch
    const startPosition = (value1 < 0 ? value1 : 0) * 100 + 300;
    const endPosition = Math.max(value1, value2) * 100 + 300;
    let color: string;

    if (endPosition <= 300) color = "blue";
    else if (startPosition <= 300) color = "gradient";
    else color = "yellow";

    connectorsArray.push({
      class: color,
      startPosition,
      width: endPosition - startPosition
    });
  }
}
