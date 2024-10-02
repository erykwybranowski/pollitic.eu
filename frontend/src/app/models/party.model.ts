import {Group} from "./group.model";

export interface Party {
  id: number;
  stringId: string;
  acronym: string;
  englishName: string;
  countryCode: string | null;
  localName: string[] | null;
  CHES_EU: number | number[] | null;
  CHES_Economy: number | number[] | null;
  CHES_Progress: number | number[] | null;
  CHES_Liberal: number | number[] | null;
  subParties: Party[] | null;
  group: Set<Group> | null;
  mp: number | null;
  role: Set<string> | null;
}
