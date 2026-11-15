locals {
  full_prefix = "${var.prefix}-${var.environment}"

  # Storage account names: lowercase alphanumeric, max 24 chars
  storage_account_name = substr(replace("${var.prefix}${var.environment}storage", "-", ""), 0, 24)

  common_tags = merge(var.tags, {
    environment = var.environment
    project     = "teambrain"
    managed_by  = "terraform"
  })
}

# ── Resource Group ─────────────────────────────────────────────────────────

resource "azurerm_resource_group" "main" {
  name     = "${local.full_prefix}-rg"
  location = var.location
  tags     = local.common_tags
}

# ── Monitoring ─────────────────────────────────────────────────────────────

resource "azurerm_log_analytics_workspace" "main" {
  name                = "${local.full_prefix}-logs"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  sku                 = "PerGB2018"
  retention_in_days   = 90
  tags                = local.common_tags
}

resource "azurerm_application_insights" "main" {
  name                = "${local.full_prefix}-ai"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  workspace_id        = azurerm_log_analytics_workspace.main.id
  application_type    = "web"
  tags                = local.common_tags
}

# ── Key Vault ──────────────────────────────────────────────────────────────

resource "azurerm_key_vault" "main" {
  name                       = "${local.full_prefix}-kv"
  location                   = azurerm_resource_group.main.location
  resource_group_name        = azurerm_resource_group.main.name
  tenant_id                  = var.tenant_id
  sku_name                   = "standard"
  enable_rbac_authorization  = true
  soft_delete_retention_days = 90
  # Set purge_protection_enabled = true for staging and prod
  tags = local.common_tags
}

# ── Storage ────────────────────────────────────────────────────────────────

resource "azurerm_storage_account" "main" {
  name                            = local.storage_account_name
  location                        = azurerm_resource_group.main.location
  resource_group_name             = azurerm_resource_group.main.name
  account_tier                    = "Standard"
  account_replication_type        = "LRS"
  min_tls_version                 = "TLS1_2"
  allow_nested_items_to_be_public = false
  https_traffic_only_enabled      = true
  tags                            = local.common_tags
}

resource "azurerm_storage_container" "artifacts" {
  name                  = "artifacts"
  storage_account_id    = azurerm_storage_account.main.id
  container_access_type = "private"
}

resource "azurerm_storage_container" "ingestion" {
  name                  = "ingestion"
  storage_account_id    = azurerm_storage_account.main.id
  container_access_type = "private"
}

# ── Azure SQL ──────────────────────────────────────────────────────────────

resource "azurerm_mssql_server" "main" {
  name                         = "${local.full_prefix}-sql"
  location                     = azurerm_resource_group.main.location
  resource_group_name          = azurerm_resource_group.main.name
  version                      = "12.0"
  administrator_login          = var.sql_admin_login
  administrator_login_password = var.sql_admin_password
  minimum_tls_version          = "1.2"
  tags                         = local.common_tags
}

# Allows Azure-hosted services (including the App Service) to reach SQL
resource "azurerm_mssql_firewall_rule" "allow_azure_services" {
  name             = "AllowAzureServices"
  server_id        = azurerm_mssql_server.main.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

resource "azurerm_mssql_database" "main" {
  name      = "teambrain"
  server_id = azurerm_mssql_server.main.id
  collation = "SQL_Latin1_General_CP1_CI_AS"
  sku_name  = "S1"
  tags      = local.common_tags
}

# ── Azure AI Search ────────────────────────────────────────────────────────

resource "azurerm_search_service" "main" {
  name                = "${local.full_prefix}-search"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  sku                 = "standard"
  replica_count       = 1
  partition_count     = 1
  semantic_search_sku = "standard"
  tags                = local.common_tags
}

# ── Azure OpenAI ───────────────────────────────────────────────────────────

resource "azurerm_cognitive_account" "openai" {
  name                  = "${local.full_prefix}-openai"
  location              = azurerm_resource_group.main.location
  resource_group_name   = azurerm_resource_group.main.name
  kind                  = "OpenAI"
  sku_name              = "S0"
  custom_subdomain_name = "${local.full_prefix}-openai"
  tags                  = local.common_tags
}

resource "azurerm_cognitive_deployment" "gpt4o" {
  name                 = "gpt-4o"
  cognitive_account_id = azurerm_cognitive_account.openai.id

  model {
    format  = "OpenAI"
    name    = "gpt-4o"
    version = "2024-11-20"
  }

  sku {
    name     = "Standard"
    capacity = 10
  }
}

# Only one deployment can be provisioned at a time per OpenAI account
resource "azurerm_cognitive_deployment" "embedding" {
  name                 = "text-embedding-ada-002"
  cognitive_account_id = azurerm_cognitive_account.openai.id

  model {
    format  = "OpenAI"
    name    = "text-embedding-ada-002"
    version = "2"
  }

  sku {
    name     = "Standard"
    capacity = 10
  }

  depends_on = [azurerm_cognitive_deployment.gpt4o]
}

# ── Service Bus ────────────────────────────────────────────────────────────

resource "azurerm_servicebus_namespace" "main" {
  name                = "${local.full_prefix}-sb"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  sku                 = "Standard"
  tags                = local.common_tags
}

resource "azurerm_servicebus_queue" "ingestion_jobs" {
  name         = "ingestion-jobs"
  namespace_id = azurerm_servicebus_namespace.main.id

  max_delivery_count = 5
  lock_duration      = "PT5M"
}

resource "azurerm_servicebus_queue" "indexing_jobs" {
  name         = "indexing-jobs"
  namespace_id = azurerm_servicebus_namespace.main.id

  max_delivery_count = 5
  lock_duration      = "PT5M"
}

# ── App Service ────────────────────────────────────────────────────────────

resource "azurerm_service_plan" "main" {
  name                = "${local.full_prefix}-plan"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  os_type             = "Linux"
  sku_name            = "B2"
  tags                = local.common_tags
}

resource "azurerm_linux_web_app" "api" {
  name                = "${local.full_prefix}-api"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  service_plan_id     = azurerm_service_plan.main.id
  https_only          = true
  tags                = local.common_tags

  identity {
    type = "SystemAssigned"
  }

  site_config {
    always_on     = true
    http2_enabled = true

    application_stack {
      dotnet_version = "10.0"
    }
  }

  app_settings = {
    APPLICATIONINSIGHTS_CONNECTION_STRING = azurerm_application_insights.main.connection_string

    "AzureAd__TenantId"  = var.tenant_id
    "AzureAd__ClientId"  = var.client_id
    "AzureAd__Instance"  = "https://login.microsoftonline.com/"
    "AzureAd__Audience"  = "api://${var.client_id}"

    "AzureOpenAi__Endpoint"            = azurerm_cognitive_account.openai.endpoint
    "AzureOpenAi__ChatDeployment"      = azurerm_cognitive_deployment.gpt4o.name
    "AzureOpenAi__EmbeddingDeployment" = azurerm_cognitive_deployment.embedding.name

    "AzureAiSearch__Endpoint"  = "https://${azurerm_search_service.main.name}.search.windows.net"
    "AzureAiSearch__IndexName" = "teambrain-knowledge-v1"

    "BlobStorage__AccountUri" = azurerm_storage_account.main.primary_blob_endpoint
    "KeyVaultUri"             = azurerm_key_vault.main.vault_uri
    "AllowedOrigins"          = "https://${local.full_prefix}-web.azurewebsites.net"
  }

  connection_string {
    name  = "DefaultConnection"
    type  = "SQLAzure"
    value = "Server=tcp:${azurerm_mssql_server.main.fully_qualified_domain_name},1433;Database=teambrain;Authentication=Active Directory Default;Encrypt=yes;TrustServerCertificate=no;"
  }
}

# ── RBAC Role Assignments ──────────────────────────────────────────────────
# All permissions scoped to specific resources — no subscription-wide access.

resource "azurerm_role_assignment" "api_keyvault_secrets_user" {
  scope                = azurerm_key_vault.main.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_linux_web_app.api.identity[0].principal_id
}

resource "azurerm_role_assignment" "api_openai_user" {
  scope                = azurerm_cognitive_account.openai.id
  role_definition_name = "Cognitive Services OpenAI User"
  principal_id         = azurerm_linux_web_app.api.identity[0].principal_id
}

resource "azurerm_role_assignment" "api_search_contributor" {
  scope                = azurerm_search_service.main.id
  role_definition_name = "Search Index Data Contributor"
  principal_id         = azurerm_linux_web_app.api.identity[0].principal_id
}

resource "azurerm_role_assignment" "api_storage_blob_contributor" {
  scope                = azurerm_storage_account.main.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azurerm_linux_web_app.api.identity[0].principal_id
}

resource "azurerm_role_assignment" "api_servicebus_owner" {
  scope                = azurerm_servicebus_namespace.main.id
  role_definition_name = "Azure Service Bus Data Owner"
  principal_id         = azurerm_linux_web_app.api.identity[0].principal_id
}
