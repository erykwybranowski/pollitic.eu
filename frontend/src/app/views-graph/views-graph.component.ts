import {Component, Input, OnChanges, OnInit, SimpleChanges} from '@angular/core';
import {Party} from "../models/party.model";
import {NgClass, NgForOf, NgIf} from "@angular/common";

@Component({
  selector: 'app-views-graph',
  standalone: true,
  imports: [
    NgClass,
    NgForOf,
    NgIf
  ],
  templateUrl: './views-graph.component.html',
  styleUrl: './views-graph.component.scss'
})
export class ViewsGraphComponent implements OnChanges {
  @Input() party!: Party;
  @Input() leftIcons!: boolean;
  @Input() rightIcons!: boolean;

  economySquares: any[] = [];
  economyConnectors: any[] = [];

  liberalSquares: any[] = [];
  liberalConnectors: any[] = [];

  progressSquares: any[] = [];
  progressConnectors: any[] = [];

  euSquares: any[] = [];
  euConnectors: any[] = [];

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['party']) {
      this.generateGraph();
    }
  }

  generateGraph(): void {
    // Clear existing data
    this.economySquares = [];
    this.economyConnectors = [];
    this.liberalSquares = [];
    this.liberalConnectors = [];
    this.progressSquares = [];
    this.progressConnectors = [];
    this.euSquares = [];
    this.euConnectors = [];

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

    if (Array.isArray(chesValue)) {
      // If CHES value is a range
      this.addSquare(squaresArray, chesValue[0], chesValue[0] < 0 ? 'yellow' : (chesValue[0] == 0 ? "center" : "blue"));  // First value
      this.addSquare(squaresArray, chesValue[1], chesValue[1] < 0 ? 'yellow' : (chesValue[1] == 0 ? "center" : "blue"));  // Second value
      this.addRangeConnector(connectorsArray, chesValue[0], chesValue[1]);  // Updated range connector logic
    } else if (chesValue && chesValue != 0) {
      // If CHES value is a single value
      this.addSquare(squaresArray, chesValue, chesValue > 0 ? 'blue' : 'yellow');
    } else if (chesValue == 0) {
      // If CHES value is 0
      this.addSquare(squaresArray, 0, 'center');
    }
  }

  addSquare(squaresArray: any[], value: number, colorClass: string): void {
    const position = 150 + value * 50;  // 150 is the center, move left/right based on value
    squaresArray.push({ class: colorClass, position });
  }

  addRangeConnector(connectorsArray: any[], value1: number, value2: number): void {
    const startPosition = value1 * 50 + 200 - 25;
    const endPosition = Math.max(value1, value2) * 50 + 150 + 25;
    let color: string;

    if (endPosition <= 175) color = "yellow";
    else if (startPosition <= 125) color = "gradient";
    else color = "blue";

    connectorsArray.push({
      class: color,
      startPosition,
      width: endPosition - startPosition
    });
  }
}
