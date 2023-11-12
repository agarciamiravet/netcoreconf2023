locals {
  environment = "prod"
  region      = "westeurope"
  suffix      = "netcoreconfmad2023"
}

#Resource Group
resource "azurerm_resource_group" "this" {
  name     = "rg-${local.suffix}-${local.environment}"
  location = local.region

  tags = {
    Environment = "Production"
  }
}

#Azure Redis cache
resource "azurerm_redis_cache" "this" {
  name                = "cache-${local.suffix}-${local.environment}"
  location            = azurerm_resource_group.this.location
  resource_group_name = azurerm_resource_group.this.name
  capacity            = 1
  family              = "C"
  sku_name            = "Basic"
  enable_non_ssl_port = false
  minimum_tls_version = "1.2"

  redis_configuration {
  }
  tags = {
    Environment = "Production"
  }
}

#Azure SQL Server
resource "azurerm_mssql_server" "this" {
  name                         = "sql-${local.suffix}-${local.environment}"
  resource_group_name          = azurerm_resource_group.this.name
  location                     = azurerm_resource_group.this.location
  version                      = "12.0"
  administrator_login          = var.sql_server_admin_user
  administrator_login_password = var.sql_server_admin_password
}

#Azure SQL Database
resource "azurerm_mssql_database" "this" {
  name           = "sqldb-${local.suffix}-${local.environment}"
  server_id      = azurerm_mssql_server.this.id
  collation      = "SQL_Latin1_General_CP1_CI_AS"
  license_type   = "LicenseIncluded"
  max_size_gb    = 1
  read_scale     = false
  sku_name       = "Basic"
  zone_redundant = false
  tags = {
    Environment = "Production"
  }
}

#Log analytics for container apps
resource "azurerm_log_analytics_workspace" "this" {
  name                = "la-${local.suffix}-${local.environment}"
  location            = azurerm_resource_group.this.location
  resource_group_name = azurerm_resource_group.this.name
  sku                 = "PerGB2018"
  retention_in_days   = 30
}

resource "azurerm_container_app_environment" "apps" {
  name                       = "cae-apps-${local.suffix}-${local.environment}"
  location                   = azurerm_resource_group.this.location
  resource_group_name        = azurerm_resource_group.this.name
  log_analytics_workspace_id = azurerm_log_analytics_workspace.this.id
}


resource "azurerm_container_app_environment" "obsevability" {
  name                       = "cae-observability-${local.suffix}-${local.environment}"
  location                   = azurerm_resource_group.this.location
  resource_group_name        = azurerm_resource_group.this.name
  log_analytics_workspace_id = azurerm_log_analytics_workspace.this.id
}

#Azure container registry
resource "azurerm_container_registry" "acr" {
  name                = "acr${local.suffix}${local.environment}"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  sku                 = "Basic"
  admin_enabled       = false
}