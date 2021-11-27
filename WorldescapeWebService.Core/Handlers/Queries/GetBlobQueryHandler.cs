using MediatR;
using Microsoft.Extensions.Logging;
using Worldescape.Database;

namespace WorldescapeWebService.Core;

public class GetBlobQueryHandler : IRequestHandler<GetBlobQuery, byte[]>
{
    #region Fields

    private readonly ILogger<GetBlobQueryHandler> _logger;
    private readonly GetBlobQueryValidator _validator;
    private readonly DatabaseService _databaseService;

    #endregion

    #region Ctor

    public GetBlobQueryHandler(
        ILogger<GetBlobQueryHandler> logger,
        GetBlobQueryValidator validator,
        DatabaseService databaseService)
    {
        _logger = logger;
        _validator = validator;
        _databaseService = databaseService;
    }

    #endregion

    #region Methods

    public async Task<byte[]> Handle(
        GetBlobQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            validationResult.EnsureValidResult();

            //var location = typeof(GetBlobQuery).Assembly.Location;

            //var newlocation = location.Replace("WorldescapeWebService\\bin\\Debug\\net6.0\\WorldescapeWebService.Core.dll", "Worldescape.Assets");

            //var path = Path.Combine(newlocation, "Assets", request.FileName);

            byte[] bytes = new byte[] { };

            //if (File.Exists(path))
            //    bytes = await File.ReadAllBytesAsync(path);

            return bytes;
        }
        catch (Exception)
        {
            throw;
        }
    }

    #endregion
}

