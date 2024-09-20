import { Injectable } from '@angular/core';
import { Country } from '../models/country.model';
import { Party } from '../models/party.model';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import {Observable, of, switchMap} from 'rxjs';
import { map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class PartyService {

  constructor(private http: HttpClient) {}

  // Method to get countries
  getCountries(): Observable<Country[]> {
    if (environment.production) {
      // Production: You will later implement this with HTTP calls
      return this.http.get<Country[]>(`${environment.apiUrl}/countries`);
    } else {
      // Development version: hardcoded countries list
      return of([
        {id: 1, countryCode: 'pl', name: 'Polska'},
        {id: 2, countryCode: 'de', name: 'Niemcy'},
        {id: 3, countryCode: 'at', name: 'Austria'},
        {id: 4, countryCode: 'be', name: 'Belgia'},
        {id: 5, countryCode: 'bg', name: 'Bułgaria'},
        {id: 6, countryCode: 'hr', name: 'Chorwacja'},
        {id: 7, countryCode: 'cy', name: 'Cypr'},
        {id: 8, countryCode: 'cz', name: 'Czechy'},
        {id: 9, countryCode: 'dk', name: 'Dania'},
        {id: 10, countryCode: 'ee', name: 'Estonia'},
        {id: 11, countryCode: 'fi', name: 'Finlandia'},
        {id: 12, countryCode: 'fr', name: 'Francja'},
        {id: 13, countryCode: 'gr', name: 'Grecja'},
        {id: 14, countryCode: 'es', name: 'Hiszpania'},
        {id: 15, countryCode: 'nl', name: 'Holandia'},
        {id: 16, countryCode: 'ie', name: 'Irlandia'},
        {id: 17, countryCode: 'lt', name: 'Litwa'},
        {id: 18, countryCode: 'lu', name: 'Luksemburg'},
        {id: 19, countryCode: 'lv', name: 'Łotwa'},
        {id: 20, countryCode: 'mt', name: 'Malta'},
        {id: 21, countryCode: 'pt', name: 'Portugalia'},
        {id: 22, countryCode: 'ro', name: 'Rumunia'},
        {id: 23, countryCode: 'sk', name: 'Słowacja'},
        {id: 24, countryCode: 'si', name: 'Słowenia'},
        {id: 25, countryCode: 'se', name: 'Szwecja'},
        {id: 26, countryCode: 'hu', name: 'Węgry'},
        {id: 27, countryCode: 'it', name: 'Włochy'}
      ]);
    }
  }

  // Method to get parties based on the country
  getParties(country: string): Observable<Party[]> {
    if (environment.production) {
      return this.http.get<Party[]>(`${environment.apiUrl}/parties?countryCode=${country}`);
    } else {
      const ropfFilePath = `${environment.localDataPath}${country}.ropf`;

      return this.readLocalFile(ropfFilePath).pipe(
        switchMap((partyLines: string[]) => {
          let partyId = 1; // Initialize party ID counter
          const parties: Party[] = partyLines.map(line => this.parsePartyLine(line, country, partyId++));

          // Read CHES data from the new CSV file
          return this.readCHESData().pipe( // Call the CHES method here without using country code
            map((chesData: any[]) => {
              this.updateCHESDataForParties(parties, chesData);
              return parties; // Return the list of parties as an Observable
            })
          );
        })
      );
    }
  }


  // Helper to read local .ropf file (simulate this)
  private readLocalFile(filePath: string): Observable<string[]> {
    const fileUrl = `${filePath}`;
    return this.http.get(fileUrl, { responseType: 'text' }).pipe(
      map((fileContent: string) => {
        let lines = this.extractPartyLinesFromRopf(fileContent);
        return lines;
      })
    );
  }

  // Extract party lines from the file content (ignore the lines with "•PF" and "&")
  private extractPartyLinesFromRopf(content: string): string[] {
    const partyLines = [];
    const lines = content.split('\n');

    let partiesStarted = false;
    for (const line of lines) {
      if (line.trim() === '') {
        if (partiesStarted) {
          break; // End extraction after the second empty line
        } else {
          partiesStarted = true; // Start collecting after the first empty line
        }
      } else if (partiesStarted) {
        partyLines.push(line); // Collect party lines between two empty lines
      }
    }

    return partyLines;
  }

  // Parse a single line from the .ropf file and create a Party object
  private parsePartyLine(line: string, country: string, id: number): Party {
    const stringId = line.split(':')[0].trim(); // Example: "N"
    const acronym = this.extractField(line, '•A:'); // Example: ".N"
    const englishName = this.extractField(line, '•EN:'); // Example: "Modern"

    // Look for any "•" marker that is not "•EN" and use it for the localName
    const localName = this.extractLocalName(line) || '';

    return {
      id: id,
      stringId: stringId,
      acronym: acronym,
      englishName: englishName,
      localName: localName,
      countryCode: country,
      CHES_EU: null,
      CHES_Economy: null,
      CHES_Progress: null,
      CHES_Liberal: null
    };
  }

  private extractLocalName(line: string): string | null {
    const regex = /•([A-Z]{2,3}):\s*(.*?)(?=•|$)/g;
    let match;

    // Loop through all matches and ensure it's not "EN"
    while ((match = regex.exec(line)) !== null) {
      const marker = match[1];
      const value = match[2].trim();

      if (marker !== 'EN') {
        return value; // Return the value of the local name if not "EN"
      }
    }

    return null; // Return null if no local name is found
  }


  private extractField(line: string, marker: string): string | null {
    const index = line.indexOf(marker);
    if (index !== -1) {
      const start = index + marker.length;
      const end = line.indexOf('•', start); // Find the next marker or the end of the string
      return line.substring(start, end !== -1 ? end : undefined).trim();
    }
    return null;
  }

  // Method to update CHES data for the Party objects
  private updateCHESDataForParties(parties: Party[], chesData: any[]): void {
    parties.forEach(party => {
      const matchingCHESData = chesData.find(data => {
        if (data.countryCode !== party.countryCode) {
          return false;
        }
        // Split the acronym field from the Excel file by "/"
        const acronyms = data.acronym.split('/').map((acr: string) => acr.trim());

        // Check if either part of the acronym matches the party's properties
        return acronyms.some((acronym: string | null) => {
          return acronym === party.acronym ||
            acronym === party.englishName ||
            acronym === party.localName ||
            acronym === party.stringId;
        });
      });

      // If matching CHES data is found, update the Party object with CHES values
      if (matchingCHESData) {
        party.CHES_EU = matchingCHESData.CHES_EU;
        party.CHES_Economy = matchingCHESData.CHES_Economy;
        party.CHES_Progress = matchingCHESData.CHES_Progress;
        party.CHES_Liberal = matchingCHESData.CHES_Liberal;
      }
    });
  }


  private readCHESData(): Observable<any[]> {
    const filePath = `${environment.localDataPath}CHES.txt`; // Adjust this if needed
    return this.http.get(filePath, { responseType: 'text' }).pipe(
      map((fileContent: string) => {
        return this.parseCHESData(fileContent);
      })
    );
  }


  private parseCHESData(content: string): any[] {
    const chesData: any[] = [];
    const lines = content.split('\n');

    let isDataSection = true; // Flag to track data section
    for (const line of lines) {
      if (line.trim() === '') {
        isDataSection = false; // End of data section
        continue;
      }

      if (isDataSection) {
        const columns = line.split(';');
        if (columns.length > 1) { // Ensure there are enough columns
          chesData.push({
            countryCode: columns[2],
            acronym: columns[3],
            CHES_EU: columns[7],
            CHES_Economy: columns[11],
            CHES_Progress: columns[15],
            CHES_Liberal: columns[20]
          });
        }
      }
    }

    return chesData;
  }




}
