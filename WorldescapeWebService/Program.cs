using MediatR;
using System.Reflection;
using FluentValidation.AspNetCore;
using WorldescapeWebService.Core;
using WorldescapeWebService;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Http.Connections;
using Worldescape.Database;
using Worldescape.Data;

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

app.MapGet(Constants.Action_GetApiToken, async (string email, string password, IMediator mediator) =>
{
    return await mediator.Send(new GetApiTokenQuery()
    {
        Email = email,
        Password = password
    });
})
.WithName("GetApiToken");

app.MapGet(Constants.Action_GetUser, async (string token, string email, string password, IMediator mediator) =>
{
    return await mediator.Send(new GetUserQuery()
    {
        Token = token,
        Email = email,
        Password = password
    });
})
.WithName("GetUser");

app.MapGet(Constants.Action_GetWorlds, async (string token, int pageIndex, int pageSize, string? searchString, IMediator mediator) =>
{
    return await mediator.Send(new GetWorldsQuery()
    {
        Token = token,
        PageIndex = pageIndex,
        PageSize = pageSize,
        SearchString = searchString
    });
})
.WithName("GetWorlds");

app.MapGet(Constants.Action_GetAsset, async (string token, string fileName, IMediator mediator) =>
{
    byte[] file = await mediator.Send(new GetAssetQuery()
    {
        Token = token,
        FileName = fileName
    });

    string fileN = fileName.Replace('\\', '_');

    return Microsoft.AspNetCore.Http.Results.File(file, "text/plain", fileN);
})
.WithName("GetAsset");

#endregion

#region Commands

app.MapPost(Constants.Action_AddUser, async (AddUserCommandRequest command, IMediator mediator) =>
{
    return await mediator.Send(new AddUserCommand
    {
        FirstName = command.FirstName,
        LastName = command.LastName,

        Name = command.Name,
        Email = command.Email,
        Password = command.Password,
        Phone = command.Phone,
        DateOfBirth = command.DateOfBirth,
        Gender = command.Gender,
        ImageUrl = command.ImageUrl,
    });
})
.WithName("AddUser");

app.MapPost(Constants.Action_UpdateUser, async (UpdateUserCommandRequest command, IMediator mediator) =>
{
    return await mediator.Send(new UpdateUserCommand
    {
        Token = command.Token,
        Id = command.Id,

        FirstName = command.FirstName,
        LastName = command.LastName,

        Name = command.Name,
        ImageUrl = command.ImageUrl,
        Gender = command.Gender,
        DateOfBirth = command.DateOfBirth,
        Phone = command.Phone,
        Email = command.Email,
        Password = command.Password,
    });
})
.WithName("UpdateUser");

app.MapPost(Constants.Action_AddWorld, async (AddWorldCommandRequest command, IMediator mediator) =>
{
    return await mediator.Send(new AddWorldCommand
    {
        Token = command.Token,
        ImageUrl = command.ImageUrl,
        Name = command.Name,
    });
})
.WithName("AddWorld");

app.MapPost(Constants.Action_UpdateWorld, async (UpdateWorldCommandRequest command, IMediator mediator) =>
{
    return await mediator.Send(new UpdateWorldCommand
    {
        Token = command.Token,
        Id = command.Id,

        ImageUrl = command.ImageUrl,
        Name = command.Name,
    });
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