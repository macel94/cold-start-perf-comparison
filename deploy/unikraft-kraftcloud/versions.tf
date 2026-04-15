terraform {
  required_version = ">= 1.7.0"

  required_providers {
    ukc = {
      source  = "unikraft-cloud/ukc"
      version = ">= 0.2.0"
    }
  }
}
