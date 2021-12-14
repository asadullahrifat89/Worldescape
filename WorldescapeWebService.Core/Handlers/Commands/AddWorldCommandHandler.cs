using MediatR;
using Microsoft.Extensions.Logging;
using Worldescape.Common;
using Worldescape.Database;

namespace WorldescapeWebService.Core;

public class AddWorldCommandHandler : IRequestHandler<AddWorldCommand, RecordResponse<World>>
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

    public async Task<RecordResponse<World>> Handle(
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
                Creator = new Creator() { Id = user.Id, ImageUrl = user.ImageUrl, Name = user.FirstName },
                PopulationCount = 0,
            };

            if (await _databaseService.InsertDocument(world))
            {
                var result= await _databaseService.FindById<World>(world.Id);
                return new RecordResponse<World>().BuildSuccessResponse(result);
            }
            else
            {
                throw new Exception($"Failed to save world. Name={request.Name}");
            }            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new RecordResponse<World>().BuildErrorResponse(ex.Message);
        }
    }

    #endregion
}

