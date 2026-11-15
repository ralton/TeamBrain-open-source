param prefix string
param location string
param tags object

resource search 'Microsoft.Search/searchServices@2023-11-01' = {
  name: '${prefix}-search'
  location: location
  tags: tags
  sku: { name: 'standard' }
  properties: {
    replicaCount: 1
    partitionCount: 1
    hostingMode: 'default'
    semanticSearch: 'standard'
  }
}

output endpoint string = 'https://${search.name}.search.windows.net'
output name string = search.name
