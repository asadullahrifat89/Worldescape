using MediatR;
using System.Reflection;
using WorldescapeService.Core;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// add validation and mediator
builder.Services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssembly(typeof(AddUserCommandValidator).GetTypeInfo().Assembly));
builder.Services.AddMediatR(typeof(AddUserCommandValidator).GetTypeInfo().Assembly);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/api/GetAccessToken", async (string email, string password, IMediator mediator) =>
{
    return await mediator.Send(new GetAccessTokenQuery() { Email = email, Password = password });
})
.WithName("GetAccessToken");

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

app.Run();

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}