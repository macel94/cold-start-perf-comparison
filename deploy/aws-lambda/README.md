# AWS Lambda Deployment

- Region: `eu-west-1`
- Runtime baseline: ASP.NET Core `10.0.5`
- Warm-start optimization disabled: provisioned concurrency must remain unset
- Benchmark paths: `GET /api/startup`, `POST /api/compute/matrix`

## Inputs

- Terraform `>= 1.7`
- AWS credentials for `eu-west-1`
- A published Lambda shim build from `src/BenchmarkApp.AwsLambdaHost`

## Build the Lambda package input

```bash
dotnet publish src/BenchmarkApp.AwsLambdaHost/BenchmarkApp.AwsLambdaHost.csproj -c Release
```

Terraform zips the publish directory automatically before creating the Lambda function and HTTP API.

## Configure

Copy `terraform.tfvars.example` to `terraform.tfvars` if you need to override the default publish directory or function name.

## Deploy

```bash
terraform -chdir=deploy/aws-lambda init
terraform -chdir=deploy/aws-lambda plan -out tfplan
terraform -chdir=deploy/aws-lambda apply tfplan
terraform -chdir=deploy/aws-lambda output -raw api_base_url
```

AWS Lambda does not expose deterministic public scale-to-zero confirmation in v1. The runner records a parity exception for every cold step instead of failing the run.
