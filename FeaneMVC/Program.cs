using FeaneMVC.Clients;
using FeaneMVC.Configuration;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddApplicationDependencies(builder.Configuration);
builder.Services.AddMenuAndReservationClients(builder.Configuration);

var app = builder.Build();

app.UseApplicationPipeline();

app.Run();
