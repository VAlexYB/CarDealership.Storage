using CarDealership.Storage.Endpoints.Grpc;
using CarDealership.Storage.Services.Abstraction;
using CarDealership.Storage.Services.Implementation;
using CarDealership.Storage.Settings;
using Microsoft.Extensions.Options;
using Minio;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5229, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1;
    });

    options.ListenAnyIP(7292, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });
});

var services = builder.Services;

services.Configure<MinioConnectionOptions>(builder.Configuration.GetSection(nameof(MinioConnectionOptions)));

services.AddSingleton<IMinioClient>(provider =>
{
    var options = provider.GetRequiredService<IOptions<MinioConnectionOptions>>().Value;
    return new MinioClient()
        .WithEndpoint(options.Endpoint)
        .WithCredentials(options.AccessKey, options.SecretKey)
        .Build();
});

services.AddGrpc();
services.AddControllers();

services.AddTransient<IFileStorageService, MinioFileStorageService>();
services.AddTransient<FileStorageRpcService>();


var app = builder.Build();
app.MapGrpcService<FileStorageRpcService>();
app.MapControllers();


app.Run();
