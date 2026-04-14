# GCP Cloud Run Deployment

- Region: `europe-west1`
- Runtime baseline: ASP.NET Core `8.0.14`
- Warm-start optimization disabled: `minScale: 0`
- Benchmark paths: `GET /api/startup`, `POST /api/compute/matrix`

Deploy:

```bash
gcloud run services replace deploy/gcp-cloud-run/service.yaml --region europe-west1
```

Record zero-state evidence from Cloud Run instance metrics before every `intent: cold` step when available.
