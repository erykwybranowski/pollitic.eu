import {Party} from "./party.model";

export interface Poll {
  id: string;
  pollster: string;
  media: string[];
  startDate: Date;
  finishDate: Date;
  type: string;
  sample: number | null;
  results: {party: Party, value: number}[];
  others: number;
  area?: string;
}
