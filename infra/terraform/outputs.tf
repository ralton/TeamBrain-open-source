output "resource_group_name" {
  description = "Name of the provisioned resource group"
  value       = azurerm_resource_group.main.name
}

output "api_url" {
  description = "TeamBrain API base URL"
  value       = "https://${azurerm_linux_web_app.api.default_hostname}"
}

output "key_vault_uri" {
  description = "Azure Key Vault URI"
  value       = azurerm_key_vault.main.vault_uri
}

output "search_endpoint" {
  description = "Azure AI Search endpoint"
  value       = "https://${azurerm_search_service.main.name}.search.windows.net"
}

output "openai_endpoint" {
  description = "Azure OpenAI endpoint"
  value       = azurerm_cognitive_account.openai.endpoint
}

output "storage_blob_endpoint" {
  description = "Azure Blob Storage primary endpoint"
  value       = azurerm_storage_account.main.primary_blob_endpoint
}

output "sql_server_fqdn" {
  description = "SQL Server fully qualified domain name"
  value       = azurerm_mssql_server.main.fully_qualified_domain_name
}

output "servicebus_endpoint" {
  description = "Service Bus namespace endpoint"
  value       = azurerm_servicebus_namespace.main.endpoint
}

output "app_insights_connection_string" {
  description = "Application Insights connection string (sensitive)"
  value       = azurerm_application_insights.main.connection_string
  sensitive   = true
}

output "api_principal_id" {
  description = "Managed identity principal ID of the App Service (for additional RBAC assignments)"
  value       = azurerm_linux_web_app.api.identity[0].principal_id
}
