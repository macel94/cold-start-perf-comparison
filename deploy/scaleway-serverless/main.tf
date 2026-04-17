provider "scaleway" {
  region = var.region
}

resource "scaleway_container_namespace" "benchmark" {
  name        = var.namespace_name
  description = var.description
  project_id  = var.project_id
  region      = var.region
}

resource "scaleway_container" "benchmark" {
  name           = var.container_name
  namespace_id   = scaleway_container_namespace.benchmark.id
  region         = var.region
  registry_image = var.image
  min_scale      = 0
  max_scale      = 1
  memory_limit   = 1024
  cpu_limit      = 1000
  port           = 8080
  privacy        = "public"
  http_option    = "enabled"

  environment_variables = var.env
}
