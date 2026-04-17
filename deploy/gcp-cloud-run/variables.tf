variable "project_id" {
  description = "Google Cloud project that hosts the benchmark service."
  type        = string
}

variable "region" {
  description = "Canonical v1 Cloud Run region."
  type        = string
  default     = "europe-west1"
}

variable "service_name" {
  description = "Cloud Run service name."
  type        = string
  default     = "benchmark-app"
}

variable "image" {
  description = "OCI image URI for the shared benchmark app."
  type        = string
}

variable "allow_unauthenticated" {
  description = "Whether to allow unauthenticated invocations for the public benchmark endpoint."
  type        = bool
  default     = true
}

variable "labels" {
  description = "Optional labels applied to the Cloud Run service."
  type        = map(string)
  default     = {}
}
