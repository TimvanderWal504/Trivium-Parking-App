describe("Login Flow", () => {
  beforeEach(() => {
    // Visit the login page before each test
    // Assumes the login page is routed under /auth
    cy.visit("/auth");
  });

  it("should display the login form", () => {
    cy.get("h3").should("contain.text", "Login to your account");
    cy.get("input#email").should("be.visible");
    cy.get("input#password").should("be.visible");
    cy.get('button[type="submit"]').should("be.visible");
  });

  it("should show validation errors for empty fields", () => {
    cy.get("input#email").focus().blur(); // Trigger touched state
    cy.get("div").should("contain.text", "Email is required.");
    cy.get('button[type="submit"]').should("be.disabled");

    cy.get("input#password").focus().blur(); // Trigger touched state
    cy.get("div").should("contain.text", "Password is required.");
    cy.get('button[type="submit"]').should("be.disabled");
  });

  it("should show validation error for invalid email format", () => {
    cy.get("input#email").type("invalid-email").blur();
    cy.get("div").should("contain.text", "Please enter a valid email.");
    cy.get('button[type="submit"]').should("be.disabled");
  });

  it("should show error message for invalid credentials", () => {
    // Use environment variables for credentials in real tests
    cy.get("input#email").type("nonexistent@example.com");
    cy.get("input#password").type("wrongpassword");
    cy.get('button[type="submit"]').click();

    // Check for the error message display
    cy.get("div").should("contain.text", "Invalid email or password.");
    cy.url().should("include", "/auth"); // Should remain on login page
  });

  // --- IMPORTANT ---
  // This test requires a valid test user in your DEV Firebase project
  // AND the corresponding credentials stored securely in cypress.env.json
  it("should login successfully, show dashboard, and logout", () => {
    // Use Cypress environment variables for sensitive data
    // https://docs.cypress.io/guides/guides/environment-variables
    const userEmail = Cypress.env("TEST_USER_EMAIL");
    const userPassword = Cypress.env("TEST_USER_PASSWORD");

    if (!userEmail || !userPassword) {
      throw new Error(
        "TEST_USER_EMAIL and TEST_USER_PASSWORD environment variables must be set"
      );
    }

    cy.get("input#email").type(userEmail);
    cy.get("input#password").type(userPassword);
    cy.get('button[type="submit"]').click();

    // Check for redirection to the portal page
    cy.url().should("include", "/portal");

    // Check for dashboard elements
    cy.get("h1").should("contain.text", "User Dashboard");
    cy.get("button").contains("Logout").should("be.visible");

    // Test logout
    cy.get("button").contains("Logout").click();

    // Check for redirection back to login page
    cy.url().should("include", "/auth");
    cy.get("h3").should("contain.text", "Login to your account"); // Verify back on login page
  });

  it("should return 401 Unauthorized when accessing protected API without token", () => {
    // Directly call the backend API endpoint without authentication
    // We need the full URL because cy.request doesn't automatically use baseUrl for API calls
    // unless configured differently. Backend seems to be running on 7137 locally.
    // We also expect the request to fail, so we use failOnStatusCode: false
    cy.wait(500); // Add a small wait just in case backend needs a moment
    cy.request({
      method: "GET",
      // url: Cypress.env("TEST_URL") + "parkingLots", // Using env variable
      url: "http://127.0.0.1:7137/api/parkingLots", // Try explicit IPv4 loopback and port
      failOnStatusCode: false, // Prevent Cypress from failing the test on non-2xx/3xx status
    }).then((response) => {
      // Assert that the status code is 401
      expect(response.status).to.eq(401);
      // Optional: Assert on the response body if the middleware returns a specific message
      // expect(response.body).to.contain('Unauthorized');
    });
  });
});
