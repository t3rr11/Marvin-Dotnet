using Marvin.DbAccess.EntityFramework;
using TestServer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMarvinDbContext();
builder.Services.AddHostedService<TestingHostedService>();

var app = builder.Build();

app.Run();