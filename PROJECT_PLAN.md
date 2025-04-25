# Trivium Parking App - Project Plan

**Date:** 2025-04-04

**Version:** 1.0

## 1. Project Goal

Develop an Angular 19 PWA with Tailwind CSS, Firebase Authentication, Azure Functions (using .NET C#) backend (Azure SQL DB), SpecFlow (xUnit runner) backend BDD testing, xUnit/FluentAssertions backend unit testing, Cypress E2E testing, Bicep/YAML for infrastructure, and Docker/YAML CI/CD for deployment, to manage parking space allocation for employees (Visitors, Management, Employees) based on priority and FCFS for the upcoming week.

## 2. High-Level Architecture

```mermaid
graph TD
    subgraph User Device
        PWA[Angular 19 PWA / Tailwind CSS]
    end

    subgraph Firebase
        Auth[Firebase Authentication]
        FCM[Firebase Cloud Messaging]
    end

    subgraph Azure
        subgraph "Azure Function App (.NET C# Isolated)"
            API[HTTP Triggers (REST API)]
            Timer[Timer Trigger (Weekly Allocation)]
            Logic[Business Logic / Allocation Algorithm]
            FAdmin[Firebase Admin SDK Integration]
            DBAccess[Data Access Layer (EF Core / Azure SQL)]
        end
        SQL[Azure SQL Database]
    end

    subgraph Infrastructure & Deployment
        Bicep[Bicep Templates]
        YAML_Infra[YAML Pipeline (Infra Orchestration)]
        Docker[Docker Containers]
        Cypress[Cypress E2E Tests]
        AngularCLI[Angular CLI (Unit Tests)]
        SpecFlow[SpecFlow BDD Tests (.NET/xUnit)]
        XUnit[xUnit/FluentAssertions Unit Tests (.NET)]
        YAML_CICD[YAML CI/CD Pipeline (App Deployment)]
    end

    User[User/Admin] --> PWA
    PWA -- Login/Auth Requests --> Auth
    Auth -- JWT Token --> PWA
    PWA -- API Calls (with JWT) --> API
    PWA -- Register/Receive --> FCM
    API -- Verify JWT --> FAdmin
    API -- CRUD Operations --> Logic
    Timer -- Runs Weekly --> Logic
    Logic -- Allocation Results/Reminders --> FAdmin
    FAdmin -- Send Push --> FCM
    Logic -- Data Operations --> DBAccess
    DBAccess -- Reads/Writes --> SQL
    YAML_Infra -- Deploys --> Bicep
    Bicep -- Creates/Updates --> SQL
    Bicep -- Creates/Updates --> Azure Function App
    YAML_CICD -- Builds/Tests/Deploys --> Docker
    YAML_CICD -- Runs --> SpecFlow
    YAML_CICD -- Runs --> XUnit
    YAML_CICD -- Runs --> AngularCLI
    YAML_CICD -- Runs --> Cypress


    style PWA fill:#f9f,stroke:#333,stroke-width:2px
    style API fill:#bdf,stroke:#333,stroke-width:2px
    style Timer fill:#bdf,stroke:#333,stroke-width:2px
    style SQL fill:#e7e7e7,stroke:#333,stroke-width:2px
    style Auth fill:#fca,stroke:#333,stroke-width:2px
    style FCM fill:#fca,stroke:#333,stroke-width:2px
    style Bicep fill:#9cf,stroke:#333,stroke-width:1px
    style YAML_Infra fill:#ddd,stroke:#333,stroke-width:1px
    style YAML_CICD fill:#ddd,stroke:#333,stroke-width:1px
    style SpecFlow fill:#9f9,stroke:#333,stroke-width:1px
    style XUnit fill:#9f9,stroke:#333,stroke-width:1px

```

## 3. Development Plan

**Phase 1: Setup & Configuration (Foundation)**

1.  **Version Control:** Initialize Git repository. Structure folders (e.g., `/infra`, `/src/backend`, `/src/frontend`, `/tests/e2e`, `/tests/bdd`, `/tests/unit-backend`).
2.  **Firebase Project:** Create project, enable Auth/FCM, get Admin SDK credentials, note frontend keys.
3.  **Azure Resources (Infrastructure as Code):**
    - **Define Bicep Templates (`/infra`):** Create modular Bicep files for Resource Group, Azure SQL Server & DB, Azure Function App (.NET Isolated), App Insights, ACR, (Optional) Key Vault, (Optional) Container Apps/App Service.
    - **Define YAML Orchestration (`/azure-pipelines-infra.yml` or similar):** Create YAML pipeline definition for deploying Bicep templates (definition only).
4.  **Local Development Environment:** Ensure Node.js, npm/yarn, Angular CLI, .NET SDK, Azure Functions Core Tools, Docker Desktop, Bicep CLI are installed. Clone repo.

**Phase 2: Backend Development (Azure Functions - .NET C#)**

1.  **Project Setup:** Initialize Azure Functions project (`func init --worker-runtime dotnet-isolated`). Create solution/project structure.
2.  **Database:** Define C# models, setup EF Core `DbContext`, implement Repository pattern, manage schema with EF Core Migrations.
3.  **API Endpoints (HTTP Triggers):** Implement required API functions in C#.
4.  **Authentication:** Implement Firebase JWT validation middleware/filter.
5.  **Allocation Logic (Timer Trigger):** Implement allocation algorithm in C#, store results, trigger notifications.
6.  **Notification Logic:** Integrate Firebase Admin SDK for .NET to send FCM messages.
7.  **Configuration:** Use `local.settings.json` and .NET configuration system.
8.  **Backend BDD Testing (SpecFlow):** Setup test project (`/tests/bdd`), write `.feature` files, implement step definitions using **xUnit** as the runner.
9.  **Backend Unit Testing (xUnit):** Setup test project (`/tests/unit-backend`), write unit tests for services, helpers, etc., using **xUnit** and **FluentAssertions**.

**Phase 3: Frontend Development (Angular PWA)**

- Setup Angular, Tailwind, Firebase, PWA. Implement modules, components, services, routing, guards, notification handling, styling, environments, and Angular unit tests (Karma/Jasmine).

**Phase 4: Integration & End-to-End Testing**

- Connect Frontend & Backend. Write Cypress E2E tests (`/tests/e2e`). Perform manual testing.

**Phase 5: Containerization & Deployment Strategy**

1.  **Dockerization:** Create `Dockerfile` for Angular PWA (Nginx) and .NET Azure Functions. (Optional) `docker-compose.yml`.
2.  **Deployment (Container-based):** Target Azure Container Apps or App Service for Containers (provisioned via Bicep).
3.  **CI/CD Pipeline Definition (YAML):**
    - **Define YAML Pipeline (`/azure-pipelines-app.yml` or similar):**
    - **CI Trigger:** On commits/PRs.
    - **Build Stage:** Build/Test Frontend (ng test, ng build), Build/Test Backend (dotnet build, dotnet test - running **SpecFlow/xUnit** and **xUnit** tests), Build Docker images, Push to ACR.
    - **Deploy Stage(s):** Deploy containers from ACR, Run EF Core Migrations, (Optional) Run Cypress E2E tests.
    - Parameterize for environments. (Definition only).
