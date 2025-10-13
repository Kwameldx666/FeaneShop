using FeaneMVC.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddGatewayDependencies(builder.Configuration);

var app = builder.Build();

app.UseApplicationPipeline();

app.Run();
