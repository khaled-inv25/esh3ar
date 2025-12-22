using Esh3arTech.Abp.Media;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseAutofac();

builder.Services.AddControllers();

await builder.AddApplicationAsync<Esh3arTechAbpMediaModule>();

var app = builder.Build();

await app.InitializeApplicationAsync();
await app.RunAsync();