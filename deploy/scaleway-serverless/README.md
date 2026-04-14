# Scaleway Serverless Containers Deployment

- Region: `fr-par`
- Runtime baseline: ASP.NET Core `8.0.14`
- Warm-start optimization disabled: keep-warm settings must remain disabled
- Benchmark paths: `GET /api/startup`, `POST /api/compute/matrix`

Deploy:

```bash
scw container namespace create name=benchmark region=fr-par
scw container container create name=benchmark-app namespace-id=<namespace-id> min-scale=0 max-scale=1 registry-image=rg.fr-par.scw.cloud/example/cold-start-perf-comparison:latest port=8080 cpu-limit=1000m memory-limit=1024Mi
```

Record container-instance evidence before each cold probe when the platform exposes it.
