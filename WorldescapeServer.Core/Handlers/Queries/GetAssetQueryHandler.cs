using MediatR;
using Microsoft.Extensions.Logging;
using Worldescape.Database;

namespace WorldescapeServer.Core;

public class GetAssetQueryHandler : IRequestHandler<GetAssetQuery, byte[]>
{
    #region Fields

    private readonly ILogger<GetAssetQueryHandler> _logger;
    private readonly GetAssetQueryValidator _validator;
    private readonly DatabaseService _databaseService;

    #endregion

    #region Ctor

    public GetAssetQueryHandler(
        ILogger<GetAssetQueryHandler> logger,
        GetAssetQueryValidator validator,
        DatabaseService databaseService)
    {
        _logger = logger;
        _validator = validator;
        _databaseService = databaseService;
    }

    #endregion

    #region Methods



    public async Task<byte[]> Handle(
        GetAssetQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            validationResult.EnsureValidResult();

            var path = Path.Combine(Directory.GetCurrentDirectory(), "Assets", request.FileName);
            
            byte[] bytes = new byte[] { };

            if (File.Exists(path))
                bytes = await File.ReadAllBytesAsync(path);
            else
                _logger.LogError($"{request.FileName} not found at {path}.");

            return bytes;
        }
        catch (Exception)
        {
            throw;
        }
    }

    #endregion
}

