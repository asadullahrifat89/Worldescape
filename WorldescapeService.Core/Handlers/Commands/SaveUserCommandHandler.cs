using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldescapeService.Core;

public class SaveUserCommandHandler : IRequestHandler<SaveUserCommand, ServiceResponse>
{
    #region Fields

    private readonly ILogger<SaveUserCommandHandler> _logger;
    private readonly SaveUserCommandValidator _validator;

    #endregion

    public SaveUserCommandHandler(
        ILogger<SaveUserCommandHandler> logger,
        SaveUserCommandValidator validator)
    {
        _logger = logger;
        _validator = validator;
    }

    public async Task<ServiceResponse> Handle(SaveUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            validationResult.EnsureValidResult();

            return new ServiceResponse() { HttpStatusCode = System.Net.HttpStatusCode.OK };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new ServiceResponse() { HttpStatusCode = System.Net.HttpStatusCode.InternalServerError, ExternalError = ex.Message };
        }
    }
}

