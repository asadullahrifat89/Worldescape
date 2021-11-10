using LiteDB;
using MediatR;
using Microsoft.Extensions.Logging;
using Worldescape.Core;

namespace WorldescapeService.Core;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, ServiceResponse>
{
    #region Fields

    private readonly ILogger<UpdateUserCommandHandler> _logger;
    private readonly UpdateUserCommandValidator _validator;

    #endregion

    #region Ctor

    public UpdateUserCommandHandler(
        ILogger<UpdateUserCommandHandler> logger,
        UpdateUserCommandValidator validator)
    {
        _logger = logger;
        _validator = validator;
    }

    #endregion

    #region Methods
    public async Task<ServiceResponse> Handle(
        UpdateUserCommand request,
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

                // Use LINQ to query documents (with no index)
                var result = col.FindOne(x => x.Id == request.Id);

                if (result == null)
                    throw new Exception("User with Id: " + request.Id + "not found");

                // update user instance
                result.Name = request.Name;
                result.ImageUrl = request.ImageUrl;
                result.UpdatedOn = DateTime.Now;
                result.Email = request.Email;
                result.Password = request.Password;
                result.Phone = request.Phone;

                // update user document (Id will be auto-incremented)
                col.Update(result);
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

