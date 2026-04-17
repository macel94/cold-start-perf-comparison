output "service_name" {
  description = "Cloud Run service name."
  value       = google_cloud_run_v2_service.benchmark.name
}

output "service_url" {
  description = "Public base URL for the benchmark app."
  value       = google_cloud_run_v2_service.benchmark.uri
}
