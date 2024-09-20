import {Party} from "./party.model";

export interface Poll {
  id: number;
  pollster: string;
  media: string[];
  startDate: Date;
  finishDate: Date;
  type: string;
  sample: number;
  results: {party: Party, value: number}[];
}
