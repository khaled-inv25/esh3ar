using Esh3arTech.Abp.Gateway;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseAutofac();

builder.Services.AddControllers();

await builder.AddApplicationAsync<Esh3arTechAbpGatewayModule>();

var app = builder.Build();

await app.InitializeApplicationAsync();
app.MapReverseProxy();
await app.RunAsync();