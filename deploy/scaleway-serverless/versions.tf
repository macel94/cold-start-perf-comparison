terraform {
  required_version = ">= 1.7.0"

  required_providers {
    scaleway = {
      source  = "scaleway/scaleway"
      version = ">= 2.34.0"
    }
  }
}
