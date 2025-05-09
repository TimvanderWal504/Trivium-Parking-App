describe("Login Flow", () => {
  beforeEach(() => {
    cy.visit("/auth");
  });

  it("should display the login form", () => {
    cy.get("h1").should("contain.text", "Trivium Benelux Parking");
    cy.get("h2").should("contain.text", "Sign in");

    cy.get("input#email").should("be.visible");
    cy.get("input#password").should("be.visible");
    cy.get('button[type="submit"]')
      .should("be.visible")
      .and("contain.text", "Sign in");
  });

  it("should show validation errors for empty fields", () => {
    cy.get("input#email").focus().blur();
    cy.get("div.text-red-600").should("contain.text", "Email is required.");
    cy.get('button[type="submit"]').should("be.disabled");

    cy.get("input#password").focus().blur();
    cy.get("div.text-red-600").should("contain.text", "Password is required.");
    cy.get('button[type="submit"]').should("be.disabled");
  });

  it("should show validation error for invalid email format", () => {
    cy.get("input#email").type("invalid-email").blur();
    cy.get("div.text-red-600").should("contain.text", "Invalid email format.");
    cy.get('button[type="submit"]').should("be.disabled");
  });

  it("should show validation error for short password", () => {
    cy.get("input#password").type("123").blur();
    cy.get("div.text-red-600").should(
      "contain.text",
      "Min. 6 characters required."
    );
    cy.get('button[type="submit"]').should("be.disabled");
  });

  it("should show error message for invalid credentials", () => {
    cy.get("input#email").type("nonexistent@example.com");
    cy.get("input#password").type("wrongpassword");
    cy.get('button[type="submit"]').click();

    cy.get("div.p-3").should("contain.text", "Invalid email or password.");
    cy.url().should("include", "/auth");
  });

  it("should login successfully, show dashboard, and logout", () => {
    const email = Cypress.env("TEST_USER_EMAIL");
    const pass = Cypress.env("TEST_USER_PASSWORD");

    if (!email || !pass) {
      throw new Error(
        "TEST_USER_EMAIL and TEST_USER_PASSWORD must be set in cypress.env.json"
      );
    }

    cy.get("input#email").type(email);
    cy.get("input#password").type(pass);
    cy.get('button[type="submit"]').click();

    cy.url({ timeout: 10000 }).should("include", "/portal");

    cy.contains("Manage your parking requests below.", {
      timeout: 10000,
    }).should("be.visible");
    cy.contains("My Allocations").should("be.visible");
    cy.contains("Request Parking").should("be.visible");

    cy.get("button").contains("Logout").should("be.visible");

    cy.get("button").contains("Logout").click();
    cy.url().should("include", "/auth");

    cy.get("h2").should("contain.text", "Sign in");
  });

  it("should return 401 Unauthorized when accessing protected API without token", () => {
    // Directly call the backend API endpoint without authentication
    // We need the full URL because cy.request doesn't automatically use baseUrl for API calls
    // unless configured differently. Backend seems to be running on 7137 locally.
    // We also expect the request to fail, so we use failOnStatusCode: false
    cy.wait(500);
    cy.request({
      method: "GET",
      url: "http://127.0.0.1:7137/api/parkingLots",
      failOnStatusCode: false,
    }).then((response) => {
      expect(response.status).to.eq(401);
    });
  });
});
