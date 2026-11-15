param prefix string
param location string
param tags object
param adminLogin string
@secure()
param adminPassword string

resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' = {
  name: '${prefix}-sql'
  location: location
  tags: tags
  properties: {
    administratorLogin: adminLogin
    administratorLoginPassword: adminPassword
    minimalTlsVersion: '1.2'
  }
}

resource sqlFirewallAzure 'Microsoft.Sql/servers/firewallRules@2023-08-01-preview' = {
  parent: sqlServer
  name: 'AllowAzureServices'
  properties: { startIpAddress: '0.0.0.0', endIpAddress: '0.0.0.0' }
}

resource sqlDb 'Microsoft.Sql/servers/databases@2023-08-01-preview' = {
  parent: sqlServer
  name: 'teambrain'
  location: location
  tags: tags
  sku: { name: 'S1', tier: 'Standard' }
  properties: { collation: 'SQL_Latin1_General_CP1_CI_AS' }
}

output connectionString string = 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Database=teambrain;Authentication=Active Directory Default;Encrypt=yes;TrustServerCertificate=no;'
output serverName string = sqlServer.name
