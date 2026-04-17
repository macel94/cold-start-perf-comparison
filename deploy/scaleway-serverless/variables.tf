variable "project_id" {
  description = "Scaleway project ID that owns the benchmark resources."
  type        = string
}

variable "region" {
  description = "Canonical v1 Scaleway region."
  type        = string
  default     = "fr-par"
}

variable "namespace_name" {
  description = "Scaleway Container namespace name."
  type        = string
  default     = "benchmark"
}

variable "container_name" {
  description = "Scaleway serverless container name."
  type        = string
  default     = "benchmark-app"
}

variable "image" {
  description = "Registry image URI for the shared benchmark app."
  type        = string
}

variable "description" {
  description = "Optional namespace description."
  type        = string
  default     = "Cross-cloud benchmark namespace"
}

variable "env" {
  description = "Optional environment variables passed to the Scaleway container."
  type        = map(string)
  default     = {}
}
