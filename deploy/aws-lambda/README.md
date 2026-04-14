# AWS Lambda Deployment

- Region: `eu-west-1`
- Runtime baseline: ASP.NET Core `8.0.14`
- Warm-start optimization disabled: provisioned concurrency must remain unset
- Benchmark paths: `GET /api/startup`, `POST /api/compute/matrix`

Publish the Lambda shim:

```bash
dotnet publish src/BenchmarkApp.AwsLambdaHost/BenchmarkApp.AwsLambdaHost.csproj -c Release
sam deploy --template-file deploy/aws-lambda/template.yaml --guided
```

AWS Lambda does not expose deterministic public scale-to-zero confirmation in v1. The runner records a parity exception for every cold step instead of failing the run.
