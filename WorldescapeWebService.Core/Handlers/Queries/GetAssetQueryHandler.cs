using MediatR;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace WorldescapeWebService.Core;

public class GetAssetQueryHandler : IRequestHandler<GetAssetQuery, byte[]>
{
    #region Fields

    private readonly ILogger<GetAssetQueryHandler> _logger;
    private readonly GetAssetQueryValidator _validator;

    #endregion

    #region Ctor

    public GetAssetQueryHandler(
        ILogger<GetAssetQueryHandler> logger,
        GetAssetQueryValidator validator)
    {
        _logger = logger;
        _validator = validator;
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

            var newlocation = location.Replace("WorldescapeWebService\\bin\\Debug\\net6.0\\WorldescapeWebService.Core.dll", "Worldescape.Assets");

            var path = Path.Combine(newlocation, "Assets", request.FileName);

            byte[] bytes = new byte[] { };

            if (File.Exists(path))
                bytes = await System.IO.File.ReadAllBytesAsync(path);

            return bytes;
        }
        catch (Exception)
        {
            throw;
        }
    }

    #endregion
}

