﻿using MediatR;
using Microsoft.Extensions.Logging;
using Worldescape.Data;
using Worldescape.Database;

namespace WorldescapeWebService.Core;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, ServiceResponse>
{
    #region Fields

    private readonly ILogger<UpdateUserCommandHandler> _logger;
    private readonly UpdateUserCommandValidator _validator;
    private readonly DatabaseService _databaseService;

    #endregion

    #region Ctor

    public UpdateUserCommandHandler(
        ILogger<UpdateUserCommandHandler> logger,
        UpdateUserCommandValidator validator,
        DatabaseService databaseService)
    {
        _logger = logger;
        _validator = validator;
        _databaseService = databaseService;
    }

    #endregion

    #region Methods
    public async Task<ServiceResponse> Handle(
        UpdateUserCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            validationResult.EnsureValidResult();

            var result = await _databaseService.FindById<User>(request.Id);

            if (result == null || result.IsEmpty())
                throw new Exception("User with Id: " + request.Id + "not found.");

            // update user instance
            result.FirstName = request.FirstName;
            result.LastName = request.LastName;
            result.Name = request.Name;
            result.ImageUrl = request.ImageUrl;
            result.UpdatedOn = DateTime.Now;
            result.Email = request.Email;
            result.Password = request.Password;
            result.Phone = request.Phone;
            result.Gender = request.Gender;
            result.DateOfBirth = request.DateOfBirth;

            if (!await _databaseService.ReplaceById(result, request.Id))
                throw new Exception("User with Id: " + request.Id + "Update failed.");

            //// Open database (or create if doesn't exist)
            //using (var db = new LiteDatabase(@"Worldescape.db"))
            //{
            //    // Get Users collection
            //    var colUsers = db.GetCollection<User>("Users");

            //    // Use LINQ to query documents (with no index)
            //    var result = colUsers.FindById(request.Id);

            //    if (result == null || result.IsEmpty())
            //        throw new Exception("User with Id: " + request.Id + "not found.");

            //    // update user instance
            //    result.Name = request.Name;
            //    result.ImageUrl = request.ImageUrl;
            //    result.UpdatedOn = DateTime.Now;
            //    result.Email = request.Email;
            //    result.Password = request.Password;
            //    result.Phone = request.Phone;
            //    result.Gender = request.Gender;
            //    result.DateOfBirth = request.DateOfBirth;

            //    // update user document (Id will be auto-incremented)
            //    colUsers.Update(result);
            //}

            return new ServiceResponse() { HttpStatusCode = System.Net.HttpStatusCode.OK };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new ServiceResponse() { HttpStatusCode = System.Net.HttpStatusCode.InternalServerError, ExternalError = ex.Message };
        }
    }

    #endregion
}

