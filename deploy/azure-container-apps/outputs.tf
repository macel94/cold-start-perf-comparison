output "container_app_name" {
  description = "Azure Container App name."
  value       = azurerm_container_app.benchmark.name
}

output "service_url" {
  description = "Public base URL for the benchmark app."
  value       = "https://${azurerm_container_app.benchmark.ingress[0].fqdn}"
}
