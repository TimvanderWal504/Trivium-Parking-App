const { defineConfig } = require("cypress");

module.exports = defineConfig({
  e2e: {
    baseUrl: "http://localhost:4200", // Set the base URL for cy.visit()
    setupNodeEvents(on, config) {
      // implement node event listeners here
    },
  },
});
