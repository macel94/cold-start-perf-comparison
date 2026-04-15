using BenchmarkApp;

var builder = WebApplication.CreateBuilder(args);
BenchmarkApplication.ConfigureServices(builder.Services);

var app = builder.Build();
BenchmarkApplication.MapEndpoints(app);
app.Run();
