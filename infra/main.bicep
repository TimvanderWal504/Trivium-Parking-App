@description('The target environment for the deployment (e.g., dev, test, stg, prod).')
param environment string = 'dev'

@description('The base name for resources, often the application name.')
param baseName string = 'triviumparking'

@description('The Azure region for resource deployment.')
param location string = resourceGroup().location

// Define variables based on environment and baseName
var resourceGroupName = resourceGroup().name
var sqlServerName = '${baseName}-sql-${environment}'
var sqlDatabaseName = '${baseName}-db-${environment}'
var functionAppName = '${baseName}-func-${environment}'
var appInsightsName = '${baseName}-ai-${environment}'
var containerRegistryName = '${replace(baseName, '-', '')}acr${environment}' // ACR names have restrictions
var containerAppsEnvName = '${baseName}-cae-${environment}'
var frontendContainerAppName = '${baseName}-app-frontend-${environment}'
var backendContainerAppName = '${baseName}-app-backend-${environment}'

// --- Modules ---
// Placeholder: Modules for each resource type will be called here.
// Example:
// module sql 'sqlServer.bicep' = {
//   name: 'sqlDeployment'
//   params: {
//     location: location
//     sqlServerName: sqlServerName
//     // ... other params
//   }
// }

// --- Outputs ---
// Placeholder: Outputs like Function App hostname, SQL Server FQDN, etc.
output functionAppHostname string = '' // Example: functionAppModule.outputs.hostname
output sqlServerFqdn string = '' // Example: sqlModule.outputs.fullyQualifiedDomainName
