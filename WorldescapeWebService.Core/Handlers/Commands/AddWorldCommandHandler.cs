using MediatR;
using Microsoft.Extensions.Logging;
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
            var user = await _tokenHelper.GetUserFromApiToken(request.Token);

            // Create new user instance
            var world = new World
            {
                Name = request.Name,
                ImageUrl = request.ImageUrl,
                CreatedOn = DateTime.Now,
                UpdatedOn = null,
                Creator = new Creator() { Id = user.Id, ImageUrl = user.ImageUrl, Name = user.FirstName }
            };

            if (await _databaseService.InsertDocument(world))
            {
                return await _databaseService.FindById<World>(world.Id);
            }
            else
            {
                throw new Exception($"Failed to save world. Name={request.Name}");
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

