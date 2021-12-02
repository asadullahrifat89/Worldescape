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

#region Queries

app.MapGet(pattern: Constants.Action_GetApiToken, handler: async (string email, string password, IMediator mediator) =>
{
    return await mediator.Send(new GetApiTokenQuery()
    {
        Email = email,
        Password = password
    });

}).WithName(Constants.GetActionName(Constants.Action_GetApiToken));

app.MapGet(pattern: Constants.Action_GetUser, handler: async (string token, string email, string password, IMediator mediator) =>
{
    return await mediator.Send(new GetUserQuery()
    {
        Token = token,
        Email = email,
        Password = password
    });

}).WithName(Constants.GetActionName(Constants.Action_GetUser));

app.MapGet(pattern: Constants.Action_GetWorldsCount, handler: async (string token, string searchString, int creatorId, IMediator mediator) =>
{
    return await mediator.Send(new GetWorldsCountQuery()
    {
        Token = token,
        SearchString = searchString,
        CreatorId = creatorId
    });

}).WithName(Constants.GetActionName(Constants.Action_GetWorldsCount));

app.MapGet(pattern: Constants.Action_GetWorlds, handler: async (string token, int pageIndex, int pageSize, string searchString, int creatorId, IMediator mediator) =>
{
    return await mediator.Send(new GetWorldsQuery()
    {
        Token = token,
        PageIndex = pageIndex,
        PageSize = pageSize,
        SearchString = searchString,
        CreatorId = creatorId
    });

}).WithName(Constants.GetActionName(Constants.Action_GetWorlds));

app.MapGet(pattern: Constants.Action_GetConstructsCount, handler: async (string token, int worldId, IMediator mediator) =>
{
    return await mediator.Send(new GetConstructsCountQuery()
    {
        Token = token,
        WorldId = worldId
    });

}).WithName(Constants.GetActionName(Constants.Action_GetConstructsCount));

app.MapGet(pattern: Constants.Action_GetConstructs, handler: async (string token, int pageIndex, int pageSize, int worldId, IMediator mediator) =>
{
    return await mediator.Send(new GetConstructsQuery()
    {
        Token = token,
        PageIndex = pageIndex,
        PageSize = pageSize,
        WorldId = worldId
    });

}).WithName(Constants.GetActionName(Constants.Action_GetConstructs));

app.MapGet(pattern: Constants.Action_GetAvatarsCount, handler: async (string token, int worldId, IMediator mediator) =>
{
    return await mediator.Send(new GetAvatarsCountQuery()
    {
        Token = token,
        WorldId = worldId
    });

}).WithName(Constants.GetActionName(Constants.Action_GetAvatarsCount));

app.MapGet(pattern: Constants.Action_GetAvatars, handler: async (string token, int pageIndex, int pageSize, int worldId, IMediator mediator) =>
{
    return await mediator.Send(new GetAvatarsQuery()
    {
        Token = token,
        PageIndex = pageIndex,
        PageSize = pageSize,
        WorldId = worldId
    });

}).WithName(Constants.GetActionName(Constants.Action_GetAvatars));

app.MapGet(pattern: Constants.Action_GetAsset, handler: async (string token, string fileName, IMediator mediator, ICacheService cacheService) =>
{
    string key = token + fileName;

    byte[] file = new byte[] { };

    if (cacheService.IsSet(key))
    {
        file = cacheService.Get<byte[]>(key);
    }
    else
    {
        file = await mediator.Send(new GetAssetQuery()
        {
            Token = token,
            FileName = fileName
        });

        cacheService.Set(key, file);
    }        

    string fileN = fileName.Replace('\\', '_');

    var resultFile = Microsoft.AspNetCore.Http.Results.File(file, "text/plain", fileN);

    return resultFile;

}).WithName(Constants.GetActionName(Constants.Action_GetAsset));

app.MapGet(pattern: Constants.Action_GetBlob, handler: async (string token, int id, IMediator mediator, ICacheService cacheService) =>
{
    string key = token + id;

    byte[] file = new byte[] { };

    if (cacheService.IsSet(key))
    {
        file = cacheService.Get<byte[]>(key);
    }
    else
    {
        file = await mediator.Send(new GetBlobQuery()
        {
            Token = token,
            Id = id
        });

        cacheService.Set(key, file);
    }   

    string fileN = id.ToString();

    var resultFile = Microsoft.AspNetCore.Http.Results.File(file, "text/plain", fileN);

    return resultFile;

}).WithName(Constants.GetActionName(Constants.Action_GetBlob));

#endregion

#region Commands

app.MapPost(pattern: Constants.Action_AddUser, handler: async (AddUserCommandRequest command, IMediator mediator) =>
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

}).WithName(Constants.GetActionName(Constants.Action_AddUser));

app.MapPost(pattern: Constants.Action_UpdateUser, handler: async (UpdateUserCommandRequest command, IMediator mediator) =>
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

}).WithName(Constants.GetActionName(Constants.Action_UpdateUser));

app.MapPost(pattern: Constants.Action_AddWorld, handler: async (AddWorldCommandRequest command, IMediator mediator) =>
{
    return await mediator.Send(new AddWorldCommand
    {
        Token = command.Token,
        ImageUrl = command.ImageUrl,
        Name = command.Name,
    });

}).WithName(Constants.GetActionName(Constants.Action_AddWorld));

app.MapPost(pattern: Constants.Action_UpdateWorld, handler: async (UpdateWorldCommandRequest command, IMediator mediator) =>
{
    return await mediator.Send(new UpdateWorldCommand
    {
        Token = command.Token,
        Id = command.Id,

        ImageUrl = command.ImageUrl,
        Name = command.Name,
    });

}).WithName(Constants.GetActionName(Constants.Action_UpdateWorld));

app.MapPost(pattern: Constants.Action_SaveBlob, handler: async (SaveBlobCommandRequest command, IMediator mediator) =>
{
    return await mediator.Send(new SaveBlobCommand
    {
        Token = command.Token,
        Id = command.Id,
        DataUrl = command.DataUrl,
    });

}).WithName(Constants.GetActionName(Constants.Action_SaveBlob));

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