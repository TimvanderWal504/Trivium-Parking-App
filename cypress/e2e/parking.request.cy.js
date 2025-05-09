describe("Parking Request Flow", () => {
  beforeEach(() => {
    // Use Cypress environment variables for sensitive data
    const userEmail = Cypress.env("TEST_USER_EMAIL");
    const userPassword = Cypress.env("TEST_USER_PASSWORD");

    if (!userEmail || !userPassword) {
      throw new Error(
        "TEST_USER_EMAIL and TEST_USER_PASSWORD environment variables must be set"
      );
    }

    // Visit login page
    cy.visit("/auth");

    // Login
    cy.get("input#email").type(userEmail);
    cy.get("input#password").type(userPassword);
    cy.get('button[type="submit"]').click();

    // Wait for redirection to the portal and dashboard to load
    cy.url().should("include", "/portal");
    cy.contains("h2", "Request Parking").should("be.visible"); // Check for dashboard header
    // Wait for the loading state in the carousel to disappear
    cy.contains("Loading available days...").should("not.exist");
  });

  it("should display the parking request carousel", () => {
    cy.get("app-parking-carousel").should("be.visible");
    cy.get("swiper-container").should("be.visible");
    // Check if there are slides (days) available
    cy.get("swiper-slide").should("have.length.greaterThan", 0);
  });

  it("should allow requesting and canceling parking for a day with correct swipe order", () => {
    cy.get("swiper-container").should("exist").as("container");

    cy.get("swiper-slide")
      .first()
      .within(() => {       
        cy.get('input[type="checkbox"]').as("cb");

        cy.get("@cb").should("not.be.checked");
        cy.get("@cb")
          .parent()
          .parent()
          .should("have.class", "border-secondary/20");

        cy.get("@cb").check();
        cy.get("@cb").should("be.checked");
        cy.get("@cb").parent().parent().should("have.class", "border-primary");
      });

    cy.get("@container").then(($ctr) => {
      $ctr[0].swiper.slidePrev();
    });

    cy.get("swiper-slide")
      .first()
      .within(() => {
        cy.get('input[type="checkbox"]').uncheck();
        cy.get('input[type="checkbox"]').should("not.be.checked");
        cy.get('input[type="checkbox"]')
          .parent()
          .parent()
          .should("have.class", "border-secondary/20");
      });
  });

  // TODO: Add test for 'No requestable days found' scenario if possible/needed
  // TODO: Add test for error state if possible/needed
});
