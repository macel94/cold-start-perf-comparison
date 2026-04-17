output "container_name" {
  description = "Scaleway container name."
  value       = scaleway_container.benchmark.name
}

output "service_url" {
  description = "Public base URL for the benchmark app."
  value       = "https://${scaleway_container.benchmark.domain_name}"
}
