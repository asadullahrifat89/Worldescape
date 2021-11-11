using MediatR;
using System.Reflection;
using WorldescapeServer.Core;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

#region Add Services

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// add validation and mediator
builder.Services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssembly(typeof(AddUserCommandValidator).GetTypeInfo().Assembly));
builder.Services.AddMediatR(typeof(AddUserCommandValidator).GetTypeInfo().Assembly);

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

#endregion

app.Run();