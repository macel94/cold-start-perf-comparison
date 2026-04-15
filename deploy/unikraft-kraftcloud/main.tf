provider "ukc" {}

resource "ukc_instance" "benchmark" {
  name      = var.instance_name
  metro     = var.metro
  image     = var.image
  memory_mb = var.memory_mb
  autostart = true

  service_group = {
    services = [
      {
        port             = 443
        destination_port = 8080
        handlers         = ["http+tls"]
      }
    ]
  }
}
