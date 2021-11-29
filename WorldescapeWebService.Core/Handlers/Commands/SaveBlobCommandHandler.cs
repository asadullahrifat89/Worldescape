using MediatR;
using Microsoft.Extensions.Logging;
using Worldescape.Common;
using Worldescape.Database;

namespace WorldescapeWebService.Core;

public class SaveBlobCommandHandler : IRequestHandler<SaveBlobCommand, SaveBlobCommandResponse>
{
    #region Fields

    private readonly ILogger<SaveBlobCommandHandler> _logger;
    private readonly SaveBlobCommandValidator _validator;
    private readonly DatabaseService _databaseService;

    #endregion

    #region Ctor

    public SaveBlobCommandHandler(
        ILogger<SaveBlobCommandHandler> logger,
        SaveBlobCommandValidator validator,
        DatabaseService databaseService)
    {
        _logger = logger;
        _validator = validator;
        _databaseService = databaseService;
    }

    #endregion

    #region Methods

    public async Task<SaveBlobCommandResponse> Handle(
        SaveBlobCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            validationResult.EnsureValidResult();

            var blob = new Blob
            {
                Id = request.Id,
                DataUrl = request.DataUrl
            };

            if (await _databaseService.UpsertById(blob, request.Id))
            {
                var result = await _databaseService.FindById<Blob>(request.Id);

                return new SaveBlobCommandResponse() { Id = result.Id, HttpStatusCode = System.Net.HttpStatusCode.OK };
            }
            else
            {
                throw new Exception($"Failed to save Blob. Id={request.Id}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new SaveBlobCommandResponse() { HttpStatusCode = System.Net.HttpStatusCode.InternalServerError, ExternalError = ex.Message };
        }
    }

    #endregion
}

