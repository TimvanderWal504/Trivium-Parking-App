@description('Azure region for the resources.')
param location string

@description('Name for the Azure Container Registry.')
param containerRegistryName string

@description('The SKU for the Container Registry.')
param containerRegistrySku string = 'Basic' // Options: Basic, Standard, Premium

// Placeholder for Azure Container Registry resource definition
resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: containerRegistryName
  location: location
  sku: {
    name: containerRegistrySku
  }
  properties: {
    adminUserEnabled: false // Recommend using token-based auth or managed identity for CI/CD
  }
}

output registryId string = containerRegistry.id
output registryLoginServer string = containerRegistry.properties.loginServer
