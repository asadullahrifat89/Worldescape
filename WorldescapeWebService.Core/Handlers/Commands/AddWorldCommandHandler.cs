using LiteDB;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Worldescape.Data;
using Worldescape.Database;

namespace WorldescapeWebService.Core;

public class AddWorldCommandHandler : IRequestHandler<AddWorldCommand, World>
{
    #region Fields

    private readonly ILogger<AddWorldCommandHandler> _logger;
    private readonly AddWorldCommandValidator _validator;
    private readonly ApiTokenHelper _tokenHelper;
    private readonly DatabaseService _databaseService;

    #endregion

    #region Ctor

    public AddWorldCommandHandler(
        ILogger<AddWorldCommandHandler> logger,
        AddWorldCommandValidator validator,
        ApiTokenHelper tokenHelper,
        DatabaseService databaseService)
    {
        _logger = logger;
        _validator = validator;
        _tokenHelper = tokenHelper;
        _databaseService = databaseService;
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

            if (await _databaseService.InsertDocument(world))
            {
                return await _databaseService.FindById<World>(world.Id);
            }
            else
            {
                throw new Exception($"Failed to save world. Name={request.Name}");
            }

            //// Open database (or create if doesn't exist)
            //using (var db = new LiteDatabase(@"Worldescape.db"))
            //{
            //    // Get World collection
            //    var colWorlds = db.GetCollection<World>("Worlds");

            //    // Get the user from the token
            //    var user = _tokenHelper.GetUserFromApiToken(request.Token);

            //    // Create new user instance
            //    var world = new World
            //    {
            //        Name = request.Name,
            //        ImageUrl = request.ImageUrl,
            //        CreatedOn = DateTime.Now,
            //        UpdatedOn = null,
            //        Creator = new Creator() { Id = user.Id, ImageUrl = user.ImageUrl, Name = user.Name }
            //    };

            //    // Insert new user document (Id will be auto-incremented)
            //    BsonValue? id = colWorlds.Insert(world);

            //    if (!id.IsNull)
            //    {
            //        return colWorlds.FindById(id.AsInt32);
            //    }
            //    else
            //    {
            //        return new World();
            //    }
            //}
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new World();
        }
    }

    #endregion
}

