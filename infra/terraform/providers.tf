terraform {
  required_version = ">= 1.8.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
  }

  # Uncomment and configure to store state in Azure Blob Storage:
  # backend "azurerm" {
  #   resource_group_name  = "teambrain-tfstate-rg"
  #   storage_account_name = "teambraintfstate"
  #   container_name       = "tfstate"
  #   key                  = "teambrain.tfstate"
  # }
}

provider "azurerm" {
  features {
    key_vault {
      # Keeps soft-deleted vaults recoverable rather than purging on destroy.
      purge_soft_delete_on_destroy    = false
      recover_soft_deleted_key_vaults = true
    }
  }
}
