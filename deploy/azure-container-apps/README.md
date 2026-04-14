# Azure Container Apps Deployment

- Region: `westeurope`
- Runtime baseline: ASP.NET Core `10.0.5`
- Warm-start optimization disabled: `minReplicas: 0`
- Benchmark paths: `GET /api/startup`, `POST /api/compute/matrix`

Deploy:

```bash
az containerapp create --resource-group rg-benchmark --yaml deploy/azure-container-apps/containerapp.yaml
```

Capture replica scale evidence from Azure Container Apps before each cold probe when platform metrics are available.
