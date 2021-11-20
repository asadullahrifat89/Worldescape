using MediatR;
using System.Reflection;
using FluentValidation.AspNetCore;
using WorldescapeWebService.Core;
using WorldescapeWebService;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Http.Connections;
using Worldescape.Database;

#region Service Registration

var builder = WebApplication.CreateBuilder(args);

// Add cors policy
builder.Services.AddCors(o => o.AddPolicy(
                   "CorsPolicy",
                   builder => builder
                      .AllowAnyOrigin() // this is risky
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                   ));

// Add validation and mediator
builder.Services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssembly(typeof(AddUserCommandValidator).GetTypeInfo().Assembly));
builder.Services.AddMediatR(typeof(AddUserCommandValidator).GetTypeInfo().Assembly);

// Add services to the DI container.
builder.Services.AddSingleton<ApiTokenHelper>();
builder.Services.AddDatabaseService();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});

var app = builder.Build();

#endregion

#region Endpoint Mapping

#region Queries

app.MapGet("/api/Query/GetApiToken", async (string email, string password, IMediator mediator) =>
{
    return await mediator.Send(new GetApiTokenQuery() { Email = email, Password = password });
})
.WithName("GetApiToken");

app.MapGet("/api/Query/GetWorlds", async (string token, int pageIndex, int pageSize, string? searchString, IMediator mediator) =>
{
    return await mediator.Send(new GetWorldsQuery() { Token = token, PageIndex = pageIndex, PageSize = pageSize, SearchString = searchString });
})
.WithName("GetWorlds");

//TODO: incorporate token later
app.MapGet("/api/Query/GetAsset", async (/*string token,*/ string fileName, IMediator mediator) =>
{
    byte[] file = await mediator.Send(new GetAssetQuery() { FileName = fileName });

    string fileN = fileName.Replace('\\','_');

    return Microsoft.AspNetCore.Http.Results.File(file, "text/plain", fileN);
})
.WithName("GetAsset");

#endregion

#region Commands

app.MapPost("/api/Command/AddUser", async (AddUserCommand command, IMediator mediator) =>
{
    return await mediator.Send(command);
})
.WithName("AddUser");

app.MapPost("/api/Command/UpdateUser", async (UpdateUserCommand command, IMediator mediator) =>
{
    return await mediator.Send(command);
})
.WithName("UpdateUser");

app.MapPost("/api/Command/AddWorld", async (AddWorldCommand command, IMediator mediator) =>
{
    return await mediator.Send(command);
})
.WithName("AddWorld");

app.MapPost("/api/Command/UpdateWorld", async (UpdateWorldCommand command, IMediator mediator) =>
{
    return await mediator.Send(command);
})
.WithName("UpdateWorld");

#endregion

#endregion

#region App Run

// Configure the HTTP request pipeline.

//if (app.Environment.IsDevelopment())
//{
app.UseDeveloperExceptionPage();
app.UseSwagger();
app.UseSwaggerUI();
//}

app.UseResponseCompression();
app.UseCors("CorsPolicy");
//app.UseHttpsRedirection();
//app.UseHsts();

app.MapHub<WorldescapeHub>("/worldescapehub", options =>
{
    options.Transports = HttpTransportType.WebSockets;
});
app.Run();

#endregion