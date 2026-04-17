output "instance_uuid" {
  description = "KraftCloud instance UUID."
  value       = ukc_instance.benchmark.uuid
}

output "service_url" {
  description = "Public base URL for the benchmark app."
  value       = "https://${ukc_instance.benchmark.fqdn}"
}
