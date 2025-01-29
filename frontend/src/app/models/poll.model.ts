export interface Poll {
  id: string;
  pollster: string;
  media: string[];
  startDate: Date;
  finishDate: Date;
  type: string;
  sample: number | null;
  results: {partyId: string, value: number}[];
  others: number;
  area?: string;
}
