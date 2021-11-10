using LiteDB;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Worldescape.Core;

namespace WorldescapeService.Core;

public class AddUserCommandHandler : IRequestHandler<AddUserCommand, ServiceResponse>
{
    #region Fields

    private readonly ILogger<AddUserCommandHandler> _logger;
    private readonly AddUserCommandValidator _validator;

    #endregion

    #region Ctor

    public AddUserCommandHandler(
        ILogger<AddUserCommandHandler> logger,
        AddUserCommandValidator validator)
    {
        _logger = logger;
        _validator = validator;
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

            // Open database (or create if doesn't exist)
            using (var db = new LiteDatabase(@"WorldescapeServiceData.db"))
            {
                // Get Users collection
                var col = db.GetCollection<User>("Users");

                // Create new user instance
                var user = new User
                {
                    Name = request.Name,
                    ImageUrl = request.ImageUrl,
                    CreatedOn = DateTime.Now,
                    UpdatedOn = null,
                    Email = request.Email,
                    Pasword = request.Password,
                    Phone = request.Phone,
                };

                // Insert new user document (Id will be auto-incremented)
                col.Insert(user);
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

