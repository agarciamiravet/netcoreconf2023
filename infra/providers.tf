terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.80.0"
    }
    azapi = {
      source  = "Azure/azapi"
      version = "~> 1.9.0"
    }
  }
  required_version = ">= 1.5.2"

  backend "azurerm" {}
}

provider "azurerm" {
  features {}
}

provider "azapi" {
}