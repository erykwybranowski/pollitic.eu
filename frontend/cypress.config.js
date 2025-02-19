const { defineConfig } = require('cypress');

module.exports = defineConfig({
  e2e: {
    baseUrl: 'https://localhost',
    specPattern: 'cypress/integration/**/*.spec.{js,ts}',
    supportFile: false
  }
});
