variable "location" {
  description = "Canonical v1 Azure region."
  type        = string
  default     = "westeurope"
}

variable "resource_group_name" {
  description = "Resource group that hosts the benchmark resources."
  type        = string
  default     = "rg-benchmark"
}

variable "container_app_environment_name" {
  description = "Azure Container Apps managed environment name."
  type        = string
  default     = "aca-benchmark"
}

variable "log_analytics_workspace_name" {
  description = "Log Analytics workspace name."
  type        = string
  default     = "law-benchmark"
}

variable "container_app_name" {
  description = "Container App name."
  type        = string
  default     = "benchmark-app"
}

variable "image" {
  description = "OCI image URI for the shared benchmark app."
  type        = string
}

variable "tags" {
  description = "Optional tags applied to Azure resources."
  type        = map(string)
  default     = {}
}
