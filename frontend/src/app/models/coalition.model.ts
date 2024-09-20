import {Party} from "./party.model";

export interface Coalition {
  id: number;
  stringId: string;
  acronym: string;
  englishName: string;
  localName: string;
  parties: Party[];
}
