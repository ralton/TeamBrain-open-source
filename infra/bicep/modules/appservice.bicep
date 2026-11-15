param prefix string
param location string
param tags object
param tenantId string
param clientId string
param sqlConnectionString string
param openAiEndpoint string
param searchEndpoint string
param storageUri string
param appInsightsConnectionString string
param keyVaultUri string

resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: '${prefix}-plan'
  location: location
  tags: tags
  kind: 'linux'
  sku: { name: 'B2', tier: 'Basic' }
  properties: { reserved: true }
}

resource apiApp 'Microsoft.Web/sites@2023-12-01' = {
  name: '${prefix}-api'
  location: location
  tags: tags
  identity: { type: 'SystemAssigned' }
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|10.0'
      appSettings: [
        { name: 'APPLICATIONINSIGHTS_CONNECTION_STRING', value: appInsightsConnectionString }
        { name: 'AzureAd__TenantId', value: tenantId }
        { name: 'AzureAd__ClientId', value: clientId }
        { name: 'AzureAd__Instance', value: 'https://login.microsoftonline.com/' }
        { name: 'AzureAd__Audience', value: 'api://${clientId}' }
        { name: 'AzureOpenAi__Endpoint', value: openAiEndpoint }
        { name: 'AzureOpenAi__ChatDeployment', value: 'gpt-4o' }
        { name: 'AzureOpenAi__EmbeddingDeployment', value: 'text-embedding-ada-002' }
        { name: 'AzureAiSearch__Endpoint', value: searchEndpoint }
        { name: 'AzureAiSearch__IndexName', value: 'teambrain-knowledge-v1' }
        { name: 'BlobStorage__AccountUri', value: storageUri }
        { name: 'KeyVaultUri', value: keyVaultUri }
        { name: 'AllowedOrigins', value: 'https://${prefix}-web.azurewebsites.net' }
      ]
      connectionStrings: [
        { name: 'DefaultConnection', connectionString: sqlConnectionString, type: 'SQLAzure' }
      ]
      alwaysOn: true
      http20Enabled: true
    }
    httpsOnly: true
  }
}

output apiUrl string = 'https://${apiApp.properties.defaultHostName}'
output principalId string = apiApp.identity.principalId
