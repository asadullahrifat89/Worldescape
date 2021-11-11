using MediatR;
using System.Reflection;
using WorldescapeWebService.Core;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

#region Add Services

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

// add validation and mediator
builder.Services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssembly(typeof(AddUserCommandValidator).GetTypeInfo().Assembly));
builder.Services.AddMediatR(typeof(AddUserCommandValidator).GetTypeInfo().Assembly);
builder.Services.AddSingleton<ApiTokenHelper>();

#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

#region Endpoint Mapping

app.MapGet("/api/GetApiToken", async (string email, string password, IMediator mediator) =>
{
    return await mediator.Send(new GetApiTokenQuery() { Email = email, Password = password });
})
.WithName("GetApiToken");

app.MapPost("/api/AddUser", async (AddUserCommand command, IMediator mediator) =>
{
    return await mediator.Send(command);
})
.WithName("AddUser");

app.MapPost("/api/UpdateUser", async (UpdateUserCommand command, IMediator mediator) =>
{
    return await mediator.Send(command);
})
.WithName("UpdateUser");

app.MapPost("/api/AddWorld", async (AddWorldCommand command, IMediator mediator) =>
{
    return await mediator.Send(command);
})
.WithName("AddWorld");

app.MapPost("/api/UpdateWorld", async (UpdateWorldCommand command, IMediator mediator) =>
{
    return await mediator.Send(command);
})
.WithName("UpdateWorld");

app.MapGet("/api/GetWorlds", async (string token, int pageIndex, int pageSize, string? searchString, IMediator mediator) =>
{
    return await mediator.Send(new GetWorldsQuery() { Token = token, PageIndex = pageIndex, PageSize = pageSize, SearchString = searchString });
})
.WithName("GetWorlds");

#endregion

app.MapHub<WorldescapeHubService>("/WorldescapeHub");

app.Run();