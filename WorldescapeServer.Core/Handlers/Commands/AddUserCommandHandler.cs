//using LiteDB;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System.Text.Json;
using Worldescape.Common;
using Worldescape.Database;

namespace WorldescapeServer.Core;

public class AddUserCommandHandler : IRequestHandler<AddUserCommand, ServiceResponse>
{
    #region Fields

    private readonly ILogger<AddUserCommandHandler> _logger;
    private readonly AddUserCommandValidator _validator;
    private readonly DatabaseService _databaseService;

    #endregion

    #region Ctor

    public AddUserCommandHandler(
        ILogger<AddUserCommandHandler> logger,
        AddUserCommandValidator validator,
        DatabaseService databaseService)
    {
        _logger = logger;
        _validator = validator;
        _databaseService = databaseService;
    }

    #endregion

    #region Methods

    public async Task<ServiceResponse> Handle(
        AddUserCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            validationResult.EnsureValidResult();

            var filter = Builders<User>.Filter.Eq(x => x.Email, request.Email);         

            // Create new user instance
            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Name = request.Name,
                ImageUrl = request.ImageUrl,
                CreatedOn = DateTime.Now,
                UpdatedOn = null,
                Email = request.Email,
                Password = request.Password,
                Phone = request.Phone,
                Gender = request.Gender,
                DateOfBirth = request.DateOfBirth,
            };

            if (await _databaseService.InsertDocument(user))
            {
               var result = await _databaseService.FindOne(filter);

                if (result != null)
                {
                    // Create new api token instance for the saved user
                    var apiToken = new ApiToken()
                    {
                        UserId = result.Id,
                        Token = Guid.NewGuid().ToString()
                    };

                    if (await _databaseService.InsertDocument(apiToken))
                    {
                        _logger.LogInformation($"User: {JsonSerializer.Serialize(user, options: new JsonSerializerOptions() { WriteIndented = true })}");
                    }
                    else
                    {
                        throw new Exception("Api Token for User: " + request.Email + "insert failed.");
                    }
                }
            }
            else
            {
                throw new Exception("User with Email: " + request.Email + "insert failed.");
            }

            return new ServiceResponse() { HttpStatusCode = System.Net.HttpStatusCode.OK };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new ServiceResponse() { HttpStatusCode = System.Net.HttpStatusCode.InternalServerError, ExternalError = ex.Message };
        }
    }

    #endregion
}

