@description('Environment name (dev, staging, prod)')
param environment string = 'dev'

@description('Azure region')
param location string = resourceGroup().location

@description('Entra ID tenant ID for authentication')
param tenantId string

@description('Entra ID client ID for the TeamBrain API app registration')
param clientId string

@description('SQL Server administrator login')
param sqlAdminLogin string

@secure()
@description('SQL Server administrator password')
param sqlAdminPassword string

var prefix = 'teambrain-${environment}'
var tags = { Application: 'TeamBrain', Environment: environment }

module monitoring 'modules/monitoring.bicep' = {
  name: 'monitoring'
  params: { prefix: prefix, location: location, tags: tags }
}

module keyvault 'modules/keyvault.bicep' = {
  name: 'keyvault'
  params: { prefix: prefix, location: location, tags: tags, tenantId: tenantId }
}

module storage 'modules/storage.bicep' = {
  name: 'storage'
  params: { prefix: prefix, location: location, tags: tags }
}

module sql 'modules/sql.bicep' = {
  name: 'sql'
  params: { prefix: prefix, location: location, tags: tags, adminLogin: sqlAdminLogin, adminPassword: sqlAdminPassword }
}

module search 'modules/search.bicep' = {
  name: 'search'
  params: { prefix: prefix, location: location, tags: tags }
}

module openai 'modules/openai.bicep' = {
  name: 'openai'
  params: { prefix: prefix, location: location, tags: tags }
}

module servicebus 'modules/servicebus.bicep' = {
  name: 'servicebus'
  params: { prefix: prefix, location: location, tags: tags }
}

module appservice 'modules/appservice.bicep' = {
  name: 'appservice'
  params: {
    prefix: prefix
    location: location
    tags: tags
    tenantId: tenantId
    clientId: clientId
    sqlConnectionString: sql.outputs.connectionString
    openAiEndpoint: openai.outputs.endpoint
    searchEndpoint: search.outputs.endpoint
    storageUri: storage.outputs.blobEndpoint
    appInsightsConnectionString: monitoring.outputs.appInsightsConnectionString
    keyVaultUri: keyvault.outputs.uri
  }
}

output apiUrl string = appservice.outputs.apiUrl
output keyVaultUri string = keyvault.outputs.uri
