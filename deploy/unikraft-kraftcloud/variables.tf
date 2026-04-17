variable "metro" {
  description = "Canonical v1 KraftCloud metro."
  type        = string
  default     = "fra"
}

variable "image" {
  description = "Published KraftCloud image for the benchmark app."
  type        = string
}

variable "memory_mb" {
  description = "Instance memory allocation in MB."
  type        = number
  default     = 512
}
