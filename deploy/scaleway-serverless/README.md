# Scaleway Serverless Containers Deployment

- Region: `fr-par`
- Runtime baseline: ASP.NET Core `10.0.5`
- Warm-start optimization disabled: keep-warm settings remain disabled via `min_scale = 0`
- Benchmark paths: `GET /api/startup`, `POST /api/compute/matrix`

## Inputs

- Terraform `>= 1.7`
- Scaleway credentials (`SCW_ACCESS_KEY`, `SCW_SECRET_KEY`, `SCW_DEFAULT_PROJECT_ID` or explicit `project_id`)
- A published OCI image for the shared benchmark app

## Configure

Copy `terraform.tfvars.example` to `terraform.tfvars` and set a real project ID and image URI.

## Deploy

```bash
terraform -chdir=deploy/scaleway-serverless init
terraform -chdir=deploy/scaleway-serverless plan -out tfplan
terraform -chdir=deploy/scaleway-serverless apply tfplan
terraform -chdir=deploy/scaleway-serverless output -raw service_url
```

Record container-instance evidence before each cold probe when the platform exposes it.
