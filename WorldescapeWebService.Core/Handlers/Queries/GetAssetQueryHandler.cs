using MediatR;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Worldescape.Assets;
using Worldescape.Database;

namespace WorldescapeWebService.Core;

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

            var location = typeof(GetAssetQuery).Assembly.Location;

            string environment = null;
#if DEBUG
            environment = "Debug";
#else
            environment = "Release";
#endif
            var newlocation = location.Replace($"WorldescapeWebService\\bin\\{environment}\\net6.0\\WorldescapeWebService.Core.dll", "Worldescape.Assets");

            var path = Path.Combine(newlocation, "Assets", request.FileName);

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

