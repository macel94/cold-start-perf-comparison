# Unikraft/KraftCloud Deployment

- Metro: `fra`
- Runtime baseline: ASP.NET Core `10.0.5` / .NET SDK `10.0.201`
- Warm-start optimization disabled: stateful scale-to-zero snapshots must remain disabled for baseline parity
- Benchmark paths: `GET /api/startup`, `POST /api/compute/matrix`

## Inputs

- Terraform `>= 1.7`
- `UKC_TOKEN` or equivalent KraftCloud credentials
- A published KraftCloud image built from the official `.NET 10` HTTP server workflow

Build the benchmark image by following the official `.NET 10` guide before applying Terraform:

- https://unikraft.com/docs/guides/httpserver-dotnet10.0

## Configure

Copy `terraform.tfvars.example` to `terraform.tfvars` and set the published image reference.

## Deploy

```bash
terraform -chdir=deploy/unikraft-kraftcloud init
terraform -chdir=deploy/unikraft-kraftcloud plan -out tfplan
terraform -chdir=deploy/unikraft-kraftcloud apply tfplan
terraform -chdir=deploy/unikraft-kraftcloud output -raw service_url
```

The Terraform stack preserves the documented `fra` metro, `512 MB` memory target, and a `443 -> 8080` HTTPS listener equivalent to the `unikraft run --metro=fra -p 443:8080/tls+http -m 512M ...` workflow.

Record KraftCloud standby and instance-state evidence before each cold probe when the platform exposes it. KraftCloud documents scale-to-zero support and enables it by default.
