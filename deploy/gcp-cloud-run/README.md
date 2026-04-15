# GCP Cloud Run Deployment

- Region: `europe-west1`
- Runtime baseline: ASP.NET Core `10.0.5`
- Warm-start optimization disabled: Cloud Run minimum instances remain `0`
- Benchmark paths: `GET /api/startup`, `POST /api/compute/matrix`

## Inputs

- Terraform `>= 1.7`
- Google provider credentials (for example `gcloud auth application-default login`)
- A published OCI image for the shared benchmark app

## Configure

Copy `terraform.tfvars.example` to `terraform.tfvars` and set a real project ID and image URI.

## Deploy

```bash
terraform -chdir=deploy/gcp-cloud-run init
terraform -chdir=deploy/gcp-cloud-run plan -out tfplan
terraform -chdir=deploy/gcp-cloud-run apply tfplan
terraform -chdir=deploy/gcp-cloud-run output -raw service_url
```

Record zero-state evidence from Cloud Run revision and instance metrics before every `intent: cold` step when available.
