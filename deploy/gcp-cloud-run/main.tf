provider "google" {
  project = var.project_id
  region  = var.region
}

resource "google_cloud_run_v2_service" "benchmark" {
  name                = var.service_name
  location            = var.region
  ingress             = "INGRESS_TRAFFIC_ALL"
  deletion_protection = false
  labels              = var.labels

  template {
    timeout                           = "30s"
    max_instance_request_concurrency = 1

    scaling {
      min_instance_count = 0
      max_instance_count = 1
    }

    containers {
      image = var.image

      ports {
        container_port = 8080
      }

      resources {
        limits = {
          cpu    = "1"
          memory = "512Mi"
        }
      }
    }
  }

  traffic {
    percent         = 100
    latest_revision = true
  }
}

resource "google_cloud_run_v2_service_iam_member" "unauthenticated_invoker" {
  count = var.allow_unauthenticated ? 1 : 0

  project  = var.project_id
  location = google_cloud_run_v2_service.benchmark.location
  name     = google_cloud_run_v2_service.benchmark.name
  role     = "roles/run.invoker"
  member   = "allUsers"
}
