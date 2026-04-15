output "instance_name" {
  description = "KraftCloud instance name."
  value       = ukc_instance.benchmark.name
}

output "service_url" {
  description = "Public base URL for the benchmark app."
  value       = "https://${ukc_instance.benchmark.fqdn}"
}
