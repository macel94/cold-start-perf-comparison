provider "ukc" {
  metro = var.metro
}

resource "ukc_instance" "benchmark" {
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
