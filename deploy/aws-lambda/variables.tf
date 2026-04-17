variable "region" {
  description = "Canonical v1 Lambda region."
  type        = string
  default     = "eu-west-1"
}

variable "function_name" {
  description = "Lambda function name."
  type        = string
  default     = "benchmark-app"
}

variable "package_source_directory" {
  description = "Published Lambda shim directory that Terraform zips before deployment."
  type        = string
  default     = "../../src/BenchmarkApp.AwsLambdaHost/bin/Release/net10.0/publish"
}

variable "memory_size_mb" {
  description = "Lambda memory size in MB."
  type        = number
  default     = 1024
}

variable "timeout_seconds" {
  description = "Lambda timeout in seconds."
  type        = number
  default     = 30
}

variable "architecture" {
  description = "Lambda architecture."
  type        = string
  default     = "x86_64"
}

variable "tags" {
  description = "Optional tags applied to AWS resources."
  type        = map(string)
  default     = {}
}
