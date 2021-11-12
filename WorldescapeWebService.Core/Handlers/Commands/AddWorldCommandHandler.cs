using LiteDB;
using MediatR;
using Microsoft.Extensions.Logging;
using Worldescape.App.Core;

namespace WorldescapeWebService.Core;

public class AddWorldCommandHandler : IRequestHandler<AddWorldCommand, World>
{
    #region Fields

    private readonly ILogger<AddWorldCommandHandler> _logger;
    private readonly AddWorldCommandValidator _validator;
    private readonly ApiTokenHelper _tokenHelper;

    #endregion

    #region Ctor

    public AddWorldCommandHandler(
        ILogger<AddWorldCommandHandler> logger,
        AddWorldCommandValidator validator,
        ApiTokenHelper tokenHelper)
    {
        _logger = logger;
        _validator = validator;
        _tokenHelper = tokenHelper;
    }

    #endregion

    #region Methods

    public async Task<World> Handle(
        AddWorldCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            validationResult.EnsureValidResult();

            // Open database (or create if doesn't exist)
            using (var db = new LiteDatabase(@"Worldescape.db"))
            {
                // Get World collection
                var colWorlds = db.GetCollection<World>("Worlds");

                // Get the user from the token
                var user = _tokenHelper.GetUserFromApiToken(request.Token);

                // Create new user instance
                var world = new World
                {
                    Name = request.Name,
                    ImageUrl = request.ImageUrl,
                    CreatedOn = DateTime.Now,
                    UpdatedOn = null,
                    Creator = new Creator() { Id = user.Id, ImageUrl = user.ImageUrl, Name = user.Name }
                };

                // Insert new user document (Id will be auto-incremented)
                BsonValue? id = colWorlds.Insert(world);

                if (!id.IsNull)
                {
                    return colWorlds.FindById(id.AsInt32);
                }
                else
                {
                    return new World();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new World();
        }
    }

    #endregion
}

