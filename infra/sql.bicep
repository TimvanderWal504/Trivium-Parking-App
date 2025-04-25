@description('Azure region for the resources.')
param location string

@description('Name for the SQL Server.')
param sqlServerName string

@description('Name for the SQL Database.')
param sqlDatabaseName string

@description('The SKU name for the SQL Database (e.g., Basic, S0, GP_S_Gen5_1).')
param sqlDatabaseSkuName string = 'Basic' // Defaulting to lowest cost tier

@description('The administrator login username for the SQL Server.')
@secure()
param sqlAdminLogin string

@description('The administrator login password for the SQL Server.')
@secure()
param sqlAdminPassword string

// Placeholder for SQL Server resource definition
resource sqlServer 'Microsoft.Sql/servers@2023-05-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
    // Add firewall rules, etc. as needed
  }
}

// Placeholder for SQL Database resource definition
resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-05-01-preview' = {
  parent: sqlServer
  name: sqlDatabaseName
  location: location
  sku: {
    name: sqlDatabaseSkuName
    tier: split(sqlDatabaseSkuName, '_')[0] // Basic, Standard, Premium, GeneralPurpose, etc.
  }
  properties: {
    // collation: 'SQL_Latin1_General_CP1_CI_AS' // Example
  }
}

output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName
output sqlDatabaseId string = sqlDatabase.id
