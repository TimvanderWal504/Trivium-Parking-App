@description('Azure region for the resources.')
param location string

@description('Name for the Function App.')
param functionAppName string

@description('Name for the associated App Insights instance.')
param appInsightsName string

@description('Name for the associated Storage Account.')
param storageAccountName string // Function App requires a storage account

@description('Name for the App Service Plan (Consumption Plan).')
param hostingPlanName string

@description('Runtime stack for the Function App.')
param functionWorkerRuntime string = 'dotnet-isolated' // As per plan

// Placeholder for Storage Account resource definition
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
}

// Placeholder for App Insights resource definition
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location // Often 'eastus' or 'westus2' regardless of function app location
  kind: 'web'
  properties: {
    Application_Type: 'web'
  }
}

// Placeholder for Consumption Plan resource definition
resource hostingPlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: hostingPlanName
  location: location
  sku: {
    name: 'Y1' // Consumption plan SKU
    tier: 'Dynamic'
  }
  kind: 'functionapp'
}

// Placeholder for Function App resource definition
resource functionApp 'Microsoft.Web/sites@2022-09-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned' // Useful for accessing other Azure resources securely
  }
  properties: {
    serverFarmId: hostingPlan.id
    siteConfig: {
      appSettings: [
        { name: 'AzureWebJobsStorage', value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${listKeys(storageAccount.id, storageAccount.apiVersion).keys[0].value}' }
        { name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING', value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${listKeys(storageAccount.id, storageAccount.apiVersion).keys[0].value}' }
        { name: 'WEBSITE_CONTENTSHARE', value: toLower(functionAppName) }
        { name: 'FUNCTIONS_EXTENSION_VERSION', value: '~4' }
        { name: 'FUNCTIONS_WORKER_RUNTIME', value: functionWorkerRuntime }
        { name: 'APPLICATIONINSIGHTS_CONNECTION_STRING', value: appInsights.properties.ConnectionString }
        // Add placeholders for DB connection string, Firebase secrets (ideally from Key Vault)
        { name: 'SQLAZURECONNSTR_Database', value: '' } // Placeholder - get from SQL module output or Key Vault
        { name: 'FirebaseAdminSdkCredentials', value: '' } // Placeholder - ideally Key Vault reference
      ]
      ftpsState: 'FtpsOnly' // Enhance security
      minTlsVersion: '1.2' // Enhance security
    }
    httpsOnly: true // Enhance security
  }
}

output functionAppId string = functionApp.id
output functionAppHostname string = functionApp.properties.defaultHostName
output functionAppPrincipalId string = functionApp.identity.principalId // For granting access
