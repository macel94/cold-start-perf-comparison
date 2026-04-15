using Amazon.Lambda.AspNetCoreServer;
using BenchmarkApp;

namespace BenchmarkApp.AwsLambdaHost;

public sealed class LambdaEntryPoint : APIGatewayHttpApiV2ProxyFunction
{
    protected override void Init(IWebHostBuilder builder)
    {
        builder.ConfigureServices(BenchmarkApplication.ConfigureServices);
        builder.Configure(app =>
        {
            app.UseRouting();
            app.UseEndpoints(BenchmarkApplication.MapEndpoints);
        });
    }
}
