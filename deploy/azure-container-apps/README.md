# Azure Container Apps Deployment

- Region: `westeurope`
- Runtime baseline: ASP.NET Core `10.0.5`
- Warm-start optimization disabled: `minReplicas = 0`
- Benchmark paths: `GET /api/startup`, `POST /api/compute/matrix`

## Inputs

- Terraform `>= 1.7`
- Azure credentials (`az login` or equivalent service principal auth)
- A published OCI image for the shared benchmark app

## Configure

Copy `terraform.tfvars.example` to `terraform.tfvars` and set a reachable image URI.

## Deploy

```bash
terraform -chdir=deploy/azure-container-apps init
terraform -chdir=deploy/azure-container-apps plan -out tfplan
terraform -chdir=deploy/azure-container-apps apply tfplan
terraform -chdir=deploy/azure-container-apps output -raw service_url
```

Capture replica scale evidence from Azure Container Apps before each cold probe when platform metrics are available.
