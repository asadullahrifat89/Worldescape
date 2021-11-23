using MediatR;
using Microsoft.Extensions.Logging;
using Worldescape.Data;
using Worldescape.Database;

namespace WorldescapeWebService.Core;

public class UpdateConstructCommandHandler : IRequestHandler<UpdateConstructCommand, Construct>
{
    #region Fields

    private readonly ILogger<UpdateConstructCommandHandler> _logger;
    private readonly UpdateConstructCommandValidator _validator;
    private readonly DatabaseService _databaseService;

    #endregion

    #region Ctor

    public UpdateConstructCommandHandler(
        ILogger<UpdateConstructCommandHandler> logger,
        UpdateConstructCommandValidator validator,
        DatabaseService databaseService)
    {
        _logger = logger;
        _validator = validator;
        _databaseService = databaseService;
    }

    #endregion

    #region Methods

    public async Task<Construct> Handle(
        UpdateConstructCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            validationResult.EnsureValidResult();

            if (await _databaseService.ReplaceById(request.Construct, request.Construct.Id))
            {
                return await _databaseService.FindById<Construct>(request.Construct.Id);
            }
            else
            {
                throw new Exception($"Failed to Update Construct. Name={request.Construct.Name}");
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

