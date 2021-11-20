using LiteDB;
using MediatR;
using Microsoft.Extensions.Logging;
using Worldescape.Data;

namespace WorldescapeWebService.Core;

public class UpdateWorldCommandHandler : IRequestHandler<UpdateWorldCommand, World>
{
    #region Fields

    private readonly ILogger<UpdateWorldCommandHandler> _logger;
    private readonly UpdateWorldCommandValidator _validator;

    #endregion

    #region Ctor

    public UpdateWorldCommandHandler(
        ILogger<UpdateWorldCommandHandler> logger,
        UpdateWorldCommandValidator validator)
    {
        _logger = logger;
        _validator = validator;
    }

    #endregion

    #region Methods
    public async Task<World> Handle(
        UpdateWorldCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            validationResult.EnsureValidResult();

            // Open database (or create if doesn't exist)
            using (var db = new LiteDatabase(@"Worldescape.db"))
            {
                // Get Worlds collection
                var colWorlds = db.GetCollection<World>("Worlds");

                // Use LINQ to query documents (with no index)
                var result = colWorlds.FindById(request.Id);

                if (result == null || result.IsEmpty())
                    throw new Exception("World with Id: " + request.Id + "not found.");

                // update user instance
                result.Name = request.Name;
                result.ImageUrl = request.ImageUrl;
                result.UpdatedOn = DateTime.Now;

                // update user document (Id will be auto-incremented)
                colWorlds.Update(result);

                return result;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return null;
        }
    }

    #endregion
}

