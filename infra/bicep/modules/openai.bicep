param prefix string
param location string
param tags object

resource openAi 'Microsoft.CognitiveServices/accounts@2023-10-01-preview' = {
  name: '${prefix}-openai'
  location: location
  tags: tags
  kind: 'OpenAI'
  sku: { name: 'S0' }
  properties: {
    customSubDomainName: '${prefix}-openai'
    publicNetworkAccess: 'Enabled'
  }
}

resource chatDeployment 'Microsoft.CognitiveServices/accounts/deployments@2023-10-01-preview' = {
  parent: openAi
  name: 'gpt-4o'
  sku: { name: 'Standard', capacity: 10 }
  properties: {
    model: { format: 'OpenAI', name: 'gpt-4o', version: '2024-11-20' }
  }
}

resource embeddingDeployment 'Microsoft.CognitiveServices/accounts/deployments@2023-10-01-preview' = {
  parent: openAi
  name: 'text-embedding-ada-002'
  sku: { name: 'Standard', capacity: 10 }
  properties: {
    model: { format: 'OpenAI', name: 'text-embedding-ada-002', version: '2' }
  }
  dependsOn: [chatDeployment]
}

output endpoint string = openAi.properties.endpoint
output name string = openAi.name
