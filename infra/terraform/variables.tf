variable "environment" {
  description = "Environment name (dev, staging, prod)"
  type        = string
  default     = "dev"

  validation {
    condition     = contains(["dev", "staging", "prod"], var.environment)
    error_message = "environment must be dev, staging, or prod"
  }
}

variable "location" {
  description = "Azure region for all resources"
  type        = string
  default     = "australiaeast"
}

variable "prefix" {
  description = "Resource name prefix, combined with environment (e.g. teambrain-dev)"
  type        = string
  default     = "teambrain"
}

variable "tenant_id" {
  description = "Azure AD tenant ID for Entra ID authentication"
  type        = string
}

variable "client_id" {
  description = "App registration client ID for the API's Entra ID JWT bearer auth"
  type        = string
}

variable "sql_admin_login" {
  description = "SQL Server administrator login username"
  type        = string
  default     = "tbadmin"
}

variable "sql_admin_password" {
  description = "SQL Server administrator password"
  type        = string
  sensitive   = true
}

variable "tags" {
  description = "Additional tags merged with the default project/environment tags"
  type        = map(string)
  default     = {}
}
