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
  admin_enabled       = true
}

#Azure container for apps
resource "azurerm_container_app" "front" {
  name                         = "ca-${local.suffix}-front"
  container_app_environment_id = azurerm_container_app_environment.apps.id
  resource_group_name          = azurerm_resource_group.this.name
  revision_mode                = "Single"

  identity {
    type = "SystemAssigned"
  }

  secret {
    name  = "registrypassword"
    value = var.azure_container_registry_password
  }

  registry {
    server               = "${azurerm_container_registry.acr.name}.azurecr.io"
    username             = azurerm_container_registry.acr.name
    password_secret_name = "registrypassword"
  }

  ingress {
    external_enabled = true
    target_port      = 80
    traffic_weight {
      percentage = 100
    }
  }

  template {
    container {
      name   = "${local.suffix}-front"
      image  = "${azurerm_container_registry.acr.name}.azurecr.io/netcoreconf2023-front:latest"
      cpu    = 0.25
      memory = "0.5Gi"

      env {
        name  = "ApiUrl"
        value = "https://ca-netcoreconfmad2023-api.purplerock-194fdfd6.westeurope.azurecontainerapps.io"
      }
       env{
        name = "ObservabilityOptions__CollectorUrl"
        value = "http://40.114.230.214:4317"
      }
    }
  }
}


resource "azurerm_container_app" "api" {
  name                         = "ca-${local.suffix}-api"
  container_app_environment_id = azurerm_container_app_environment.apps.id
  resource_group_name          = azurerm_resource_group.this.name
  revision_mode                = "Single"

  identity {
    type = "SystemAssigned"
  }

  secret {
    name  = "registrypassword"
    value = var.azure_container_registry_password
  }

  registry {
    server               = "${azurerm_container_registry.acr.name}.azurecr.io"
    username             = azurerm_container_registry.acr.name
    password_secret_name = "registrypassword"
  }


  ingress {
    external_enabled = true
    target_port      = 80
    traffic_weight {
      percentage = 100
    }
  }

  template {
    container {
      name   = "${local.suffix}-api"
      image  = "${azurerm_container_registry.acr.name}.azurecr.io/netcoreconf2023-api:latest"
      cpu    = 0.25
      memory = "0.5Gi"

      env {
        name  = "ConnectionStrings__DefaultConnection"
        value = "Server=tcp:${azurerm_mssql_server.this.name}.database.windows.net,1433;Initial Catalog=${azurerm_mssql_database.this.name};Persist Security Info=False;User ID=${var.sql_server_admin_user};Password=${var.sql_server_admin_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
      }
       env {
        name  = "CacheSettings__ConnectionString"
        value = "${azurerm_redis_cache.this.primary_connection_string}"
      }
      env{
        name = "ObservabilityOptions__CollectorUrl"
        value = "http://40.114.230.214:4317"
      }
    }
  }
}