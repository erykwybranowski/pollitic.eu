/// <reference types="cypress" />

describe('Country Component', () => {
  // Intercept the API calls and return fixture data.
  beforeEach(() => {
    // Intercept call to get countries
    cy.intercept('GET', '/api/public/countries', { fixture: 'countries.json' }).as('getCountries');
    // Intercept call to get parties for country "pl"
    cy.intercept('GET', '/api/public/parties/pl', { fixture: 'parties_pl.json' }).as('getParties');
    // Intercept call to get polls for country "pl"
    cy.intercept('GET', '/api/public/polls/pl', { fixture: 'polls_pl.json' }).as('getPolls');

    // Now visit the country page. Adjust the URL if needed.
    cy.visit('/country/pl');
  });

  it('displays the country flag and name correctly', () => {
    // Wait for the countries API call.
    cy.wait('@getCountries');

    // Check that the poll-header contains an image with class "country-flag" and a header with the country name.
    cy.get('.poll-header img.country-flag').should('be.visible');
    cy.get('.poll-header .country-name').should('contain.text', 'Polska'); // Fixture countries.json should return Polska for pl
  });

  it('displays the parliament composition (support graph) if parties are present', () => {
    // Wait for parties data to load.
    cy.wait('@getParties');

    // Check that the support graph component is rendered (it has a selector like "app-support-graph")
    cy.get('app-support-graph').should('exist');
  });

  it('displays newest polls section', () => {
    // Wait for the newest polls API call.
    cy.wait('@getPolls');

    // Verify that the newest polls section is rendered.
    cy.get('.support-graph-container').should('exist');
    // Optionally check that at least one newest poll element is visible.
    cy.get('.support-graph-container .arrows').should('exist');
  });
});
