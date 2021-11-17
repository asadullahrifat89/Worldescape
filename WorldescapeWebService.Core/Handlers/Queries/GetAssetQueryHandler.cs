using MediatR;
using Microsoft.Extensions.Logging;

namespace WorldescapeWebService.Core;

public class GetAssetQueryHandler : IRequestHandler<GetAssetQuery, byte[]>
{
    #region Fields

    private readonly ILogger<GetAssetQueryHandler> _logger;
    //private readonly GetFileQueryValidator _validator;

    #endregion

    #region Ctor

    public GetAssetQueryHandler(
        ILogger<GetAssetQueryHandler> logger
        /*GetFileQueryValidator validator*/)
    {
        _logger = logger;
        //_validator = validator;
    }

    #endregion

    #region Methods

    public async Task<byte[]> Handle(
        GetAssetQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Assets", request.FileName);
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

