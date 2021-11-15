using MediatR;
using System.Reflection;
using FluentValidation.AspNetCore;
using WorldescapeWebService.Core;

#region Service Registration

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddCors(o => o.AddPolicy(
                   "CorsPolicy",
                   builder => builder
                      .AllowAnyOrigin() // this is risky
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                   ));

builder.Services.AddSignalR();

// Add validation and mediator
builder.Services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssembly(typeof(AddUserCommandValidator).GetTypeInfo().Assembly));
builder.Services.AddMediatR(typeof(AddUserCommandValidator).GetTypeInfo().Assembly);
builder.Services.AddSingleton<ApiTokenHelper>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
app.UseDeveloperExceptionPage();
app.UseSwagger();
app.UseSwaggerUI();
//}

// Configure the HTTP request pipeline.
app.UseCors("CorsPolicy");
//app.UseHttpsRedirection();
//app.UseHsts();

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

#region SignalRHub

app.MapHub<WorldescapeHub>("/WorldescapeHub");

#endregion

app.Run();