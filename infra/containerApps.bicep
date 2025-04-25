@description('Azure region for the resources.')
param location string

@description('Name for the Container Apps Environment.')
param containerAppsEnvName string

@description('Name for the Log Analytics Workspace associated with the Container Apps Environment.')
param logAnalyticsWorkspaceName string = '${containerAppsEnvName}-logs'

@description('Name for the Frontend Container App.')
param frontendContainerAppName string

@description('Name for the Backend Container App.')
param backendContainerAppName string

@description('The login server for the Azure Container Registry.')
param registryLoginServer string

@description('The name of the Azure Container Registry.')
param registryName string

@description('The image name for the frontend container (e.g., frontend:latest).')
param frontendImage string

@description('The image name for the backend container (e.g., backend:latest).')
param backendImage string

// Placeholder for Log Analytics Workspace
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: logAnalyticsWorkspaceName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

// Placeholder for Container Apps Environment
resource containerAppsEnv 'Microsoft.App/managedEnvironments@2023-05-01' = {
  name: containerAppsEnvName
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalyticsWorkspace.properties.customerId
        sharedKey: logAnalyticsWorkspace.listKeys().primarySharedKey
      }
    }
    // Zone redundancy, VNET integration etc. can be configured here
  }
}

// Placeholder for Frontend Container App
resource frontendContainerApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: frontendContainerAppName
  location: location
  properties: {
    managedEnvironmentId: containerAppsEnv.id
    configuration: {
      ingress: {
        external: true // Expose frontend to the internet
        targetPort: 80 // Assuming Nginx serves on port 80 in the container
        transport: 'http'
      }
      registries: [
        {
          server: registryLoginServer
          identity: 'system' // Use managed identity to pull from ACR
        }
      ]
      // Secrets can be defined here (e.g., API keys for frontend)
    }
    template: {
      containers: [
        {
          image: '${registryLoginServer}/${frontendImage}'
          name: 'frontend'
          resources: {
            cpu: json('0.25') // Example resource allocation
            memory: '0.5Gi'
          }
        }
      ]
      scale: { // Example scaling rules
        minReplicas: 1
        maxReplicas: 3
      }
    }
  }
  identity: {
    type: 'SystemAssigned' // Needed to pull from ACR
  }
}

// Placeholder for Backend Container App (Azure Function)
resource backendContainerApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: backendContainerAppName
  location: location
  properties: {
    managedEnvironmentId: containerAppsEnv.id
    configuration: {
      ingress: {
        external: false // Typically backend API is not directly exposed
        targetPort: 80 // Azure Functions container listens on port 80 by default
        transport: 'http'
      }
      registries: [
        {
          server: registryLoginServer
          identity: 'system' // Use managed identity to pull from ACR
        }
      ]
      secrets: [ // Placeholders for secrets needed by the function app
        { name: 'sql-connection-string', value: '' } // Get from SQL module output or Key Vault
        { name: 'firebase-sdk-creds', value: '' } // Get from Key Vault or secure config
      ]
    }
    template: {
      containers: [
        {
          image: '${registryLoginServer}/${backendImage}'
          name: 'backend'
          resources: {
            cpu: json('0.5') // Example resource allocation
            memory: '1.0Gi'
          }
          env: [ // Map secrets to environment variables
            { name: 'SQLAZURECONNSTR_Database', secretRef: 'sql-connection-string' }
            { name: 'FirebaseAdminSdkCredentialsPath', value: '/path/in/container/to/serviceAccountKey.json' } // Or use secretRef if passing content
            { name: 'AzureWebJobsStorage', value: '' } // Needs connection string for non-HTTP triggers if used
            // Add other necessary environment variables
          ]
        }
      ]
      scale: { // Example scaling rules
        minReplicas: 1
        maxReplicas: 5
      }
    }
  }
  identity: {
    type: 'SystemAssigned' // Needed to pull from ACR and potentially access other resources
  }
}

// Grant backend identity access to ACR
resource backendAcrPull 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: resourceGroup() // Or scope to ACR specifically
  name: guid(backendContainerApp.id, registryName, 'AcrPull')
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d') // AcrPull role ID
    principalId: backendContainerApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

// Grant frontend identity access to ACR
resource frontendAcrPull 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: resourceGroup() // Or scope to ACR specifically
  name: guid(frontendContainerApp.id, registryName, 'AcrPull')
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d') // AcrPull role ID
    principalId: frontendContainerApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}


output frontendFqdn string = frontendContainerApp.properties.configuration.ingress.fqdn
output backendInternalFqdn string = backendContainerApp.properties.latestRevisionFqdn // Internal FQDN for calls within the env
