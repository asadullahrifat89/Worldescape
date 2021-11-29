using MediatR;
using Microsoft.Extensions.Logging;
using Worldescape.Common;
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

            byte[] bytes = new byte[] { };

            var blob = await _databaseService.FindById<Blob>(request.Id);

            if (blob == null)
                return bytes;

            if (!blob.DataUrl.IsNullOrBlank())
            {
                var base64String = blob.DataUrl;
                base64String = base64String.Substring(base64String.IndexOf(',') + 1);

                bytes = Convert.FromBase64String(base64String);
            }                

            return bytes;
        }
        catch (Exception)
        {
            throw;
        }
    }

    #endregion
}

