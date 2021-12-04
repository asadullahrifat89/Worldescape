using MediatR;
using System.Reflection;
using FluentValidation.AspNetCore;
using WorldescapeWebService.Core;
using WorldescapeWebService;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Http.Connections;
using Worldescape.Database;
using Worldescape.Common;

#region Service Registration

var builder = WebApplication.CreateBuilder(args);

// Add cors policy
builder.Services.AddCors(o => o.AddPolicy("CorsPolicy", builder => builder
.AllowAnyOrigin() // TODO: CorsPolicy: AllowAnyOrigin() -> this is risky, should fix this after real time host
.AllowAnyMethod()
.AllowAnyHeader()));

// Add response caching and compression
builder.Services.AddResponseCaching();
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
            new[] { "application/octet-stream" });
});

// Add validation and mediator
builder.Services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssembly(typeof(AddUserCommandValidator).GetTypeInfo().Assembly));
builder.Services.AddMediatR(typeof(AddUserCommandValidator).GetTypeInfo().Assembly);

// Add services to the DI container.
builder.Services.AddSingleton<ApiTokenHelper>();
builder.Services.AddDatabaseService();
builder.Services.AddSingleton<ICacheService, CacheService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add signalR
builder.Services.AddSignalR();

var app = builder.Build();

#endregion

#region Endpoint Mapping

app.MapEndpoints();

#endregion

#region App Run

// Configure the HTTP request pipeline.

//if (app.Environment.IsDevelopment())
//{
app.UseDeveloperExceptionPage();
app.UseSwagger();
app.UseSwaggerUI();
//}

app.UseCors("CorsPolicy");
app.UseResponseCaching();
app.UseResponseCompression();
//app.UseHttpsRedirection();
//app.UseHsts();

app.MapHub<SignalRHub>("/worldescapehub", options =>
{
    options.Transports = HttpTransportType.WebSockets;
});
app.Run();

#endregion