param prefix string
param location string
param tags object

resource serviceBus 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: '${prefix}-sb'
  location: location
  tags: tags
  sku: { name: 'Standard', tier: 'Standard' }
}

resource ingestionQueue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  parent: serviceBus
  name: 'ingestion-jobs'
  properties: { maxDeliveryCount: 5, lockDuration: 'PT5M' }
}

resource indexingQueue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  parent: serviceBus
  name: 'indexing-jobs'
  properties: { maxDeliveryCount: 5, lockDuration: 'PT5M' }
}

output namespaceName string = serviceBus.name
output endpoint string = serviceBus.properties.serviceBusEndpoint
