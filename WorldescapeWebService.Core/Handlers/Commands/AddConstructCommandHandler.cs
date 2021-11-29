using MediatR;
using Microsoft.Extensions.Logging;
using Worldescape.Common;
using Worldescape.Database;

namespace WorldescapeWebService.Core;

public class AddConstructCommandHandler : IRequestHandler<AddConstructCommand, Construct>
{
    #region Fields

    private readonly ILogger<AddConstructCommandHandler> _logger;
    private readonly AddConstructCommandValidator _validator;
    private readonly DatabaseService _databaseService;

    #endregion

    #region Ctor

    public AddConstructCommandHandler(
        ILogger<AddConstructCommandHandler> logger,
        AddConstructCommandValidator validator,
        DatabaseService databaseService)
    {
        _logger = logger;
        _validator = validator;
        _databaseService = databaseService;
    }

    #endregion

    #region Methods

    public async Task<Construct> Handle(
        AddConstructCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            validationResult.EnsureValidResult();

            if (await _databaseService.UpsertById(request.Construct, request.Construct.Id))
            {
                return await _databaseService.FindById<Construct>(request.Construct.Id);
            }
            else
            {
                throw new Exception($"Failed to save Construct. Name={request.Construct.Name}");
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

