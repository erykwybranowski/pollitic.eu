/// <reference types="cypress" />
describe('Home Page', () => {
  beforeEach(() => {
    // Visit the home page before each test.
    cy.visit('/');
  });

  it('should load the homepage with logo and header', () => {
    // Verify that the homepage contains the logo image.
    cy.get('header .logo img')
      .should('be.visible')
      .and(($img) => {
        // Optionally, check that the src attribute contains "logo1.png"
        expect($img.attr('src')).to.contain('logo1.png');
      });
    // Verify that the header contains expected text (if any)
    cy.get('header').should('contain', ''); // Adjust text as needed.
  });

  it('should refresh the page when logo is clicked', () => {
    // Click the logo.
    cy.get('header .logo a').click();
    // Verify that after clicking the logo, the URL is still the home page.
    cy.url().should('eq', Cypress.config().baseUrl + '/');
    // You could also check for a specific element reloading.
  });
});
