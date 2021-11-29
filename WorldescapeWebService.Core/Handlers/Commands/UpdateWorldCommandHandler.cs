using MediatR;
using Microsoft.Extensions.Logging;
using Worldescape.Common;
using Worldescape.Database;

namespace WorldescapeWebService.Core;

public class UpdateWorldCommandHandler : IRequestHandler<UpdateWorldCommand, World>
{
    #region Fields

    private readonly ILogger<UpdateWorldCommandHandler> _logger;
    private readonly UpdateWorldCommandValidator _validator;
    private readonly DatabaseService _databaseService;

    #endregion

    #region Ctor

    public UpdateWorldCommandHandler(
        ILogger<UpdateWorldCommandHandler> logger,
        UpdateWorldCommandValidator validator,
        DatabaseService databaseService)
    {
        _logger = logger;
        _validator = validator;
        _databaseService = databaseService;
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

            var result = await _databaseService.FindById<World>(request.Id);

            // update world instance
            result.Name = request.Name;
            result.ImageUrl = request.ImageUrl;
            result.UpdatedOn = DateTime.Now;

            if (await _databaseService.ReplaceById(result, result.Id))
                return result;

            else
                throw new Exception("World with Id: " + request.Id + "Update failed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new World();
        }
    }

    #endregion
}

