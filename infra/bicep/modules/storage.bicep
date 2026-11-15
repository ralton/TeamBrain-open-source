param prefix string
param location string
param tags object

var safeName = replace(replace('${prefix}storage', '-', ''), '_', '')

resource storage 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: take(safeName, 24)
  location: location
  tags: tags
  sku: { name: 'Standard_LRS' }
  kind: 'StorageV2'
  properties: {
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
    supportsHttpsTrafficOnly: true
  }
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {
  parent: storage
  name: 'default'
}

resource artifactsContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  parent: blobService
  name: 'artifacts'
  properties: { publicAccess: 'None' }
}

resource ingestionContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  parent: blobService
  name: 'ingestion'
  properties: { publicAccess: 'None' }
}

output blobEndpoint string = storage.properties.primaryEndpoints.blob
output storageAccountName string = storage.name
