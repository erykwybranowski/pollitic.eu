export interface Party {
  id: number;
  stringId: string | null;
  acronym: string | null;
  englishName: string | null;
  localName: string | null;
  countryCode: string | null;
  CHES_EU: number | null;
  CHES_Economy: number | null;
  CHES_Progress: number | null;
  CHES_Liberal: number | null;
}
