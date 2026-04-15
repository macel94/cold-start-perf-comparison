provider "azurerm" {
  features {}
}

resource "azurerm_resource_group" "benchmark" {
  name     = var.resource_group_name
  location = var.location
  tags     = var.tags
}

resource "azurerm_log_analytics_workspace" "benchmark" {
  name                = var.log_analytics_workspace_name
  location            = azurerm_resource_group.benchmark.location
  resource_group_name = azurerm_resource_group.benchmark.name
  sku                 = "PerGB2018"
  retention_in_days   = 30
  tags                = var.tags
}

resource "azurerm_container_app_environment" "benchmark" {
  name                       = var.container_app_environment_name
  location                   = azurerm_resource_group.benchmark.location
  resource_group_name        = azurerm_resource_group.benchmark.name
  log_analytics_workspace_id = azurerm_log_analytics_workspace.benchmark.id
  tags                       = var.tags
}

resource "azurerm_container_app" "benchmark" {
  name                         = var.container_app_name
  container_app_environment_id = azurerm_container_app_environment.benchmark.id
  resource_group_name          = azurerm_resource_group.benchmark.name
  revision_mode                = "Single"
  tags                         = var.tags

  template {
    min_replicas = 0
    max_replicas = 1

    container {
      name   = var.container_app_name
      image  = var.image
      cpu    = 1.0
      memory = "2Gi"
    }
  }

  ingress {
    external_enabled = true
    target_port      = 8080
    transport        = "auto"

    traffic_weight {
      latest_revision = true
      percentage      = 100
    }
  }
}
