# Unikraft/KraftCloud Deployment

- Metro: `fra`
- Runtime baseline: ASP.NET Core `10.0.5` / .NET SDK `10.0.201`
- Warm-start optimization disabled: stateful scale-to-zero snapshots must remain disabled for baseline parity
- Benchmark paths: `GET /api/startup`, `POST /api/compute/matrix`

Build and package the shared benchmark app by following the official `.NET 10` HTTP server guide for Unikraft/KraftCloud:

- https://unikraft.com/docs/guides/httpserver-dotnet10.0

Deploy with the official `.NET 10` guide command shape:

```bash
unikraft run --metro=fra -p 443:8080/tls+http -m 512M <your-benchmark-image>
```

Keep the app bound to port `8080`, leave stateful snapshotting disabled for baseline parity, and record KraftCloud standby/instance-state evidence before each cold probe when the platform exposes it. KraftCloud documents scale-to-zero support and enables it by default.
