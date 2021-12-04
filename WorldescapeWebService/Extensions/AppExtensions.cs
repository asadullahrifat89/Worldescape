using MediatR;
using System.Reflection;
using FluentValidation.AspNetCore;
using WorldescapeWebService.Core;
using WorldescapeWebService;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Http.Connections;
using Worldescape.Database;
using Worldescape.Common;

namespace WorldescapeWebService
{
    public static class AppExtensions
    {
        public static WebApplication MapEndpoints(this WebApplication app)
        {
            #region Queries

            #region User

            app.MapGet(pattern: Constants.Action_GetApiToken, handler: async (string email, string password, IMediator mediator) =>
               {
                   return await mediator.Send(new GetApiTokenQuery()
                   {
                       Email = email,
                       Password = password
                   });

               }).WithName(Constants.GetActionName(Constants.Action_GetApiToken));

            app.MapGet(pattern: Constants.Action_GetUser, handler: async (string token, string email, string password, IMediator mediator) =>
            {
                return await mediator.Send(new GetUserQuery()
                {
                    Token = token,
                    Email = email,
                    Password = password
                });

            }).WithName(Constants.GetActionName(Constants.Action_GetUser));

            #endregion

            #region World

            app.MapGet(pattern: Constants.Action_GetWorldsCount, handler: async (string token, string searchString, int creatorId, IMediator mediator) =>
              {
                  return await mediator.Send(new GetWorldsCountQuery()
                  {
                      Token = token,
                      SearchString = searchString,
                      CreatorId = creatorId
                  });

              }).WithName(Constants.GetActionName(Constants.Action_GetWorldsCount));

            app.MapGet(pattern: Constants.Action_GetWorlds, handler: async (string token, int pageIndex, int pageSize, string searchString, int creatorId, IMediator mediator) =>
            {
                return await mediator.Send(new GetWorldsQuery()
                {
                    Token = token,
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    SearchString = searchString,
                    CreatorId = creatorId
                });

            }).WithName(Constants.GetActionName(Constants.Action_GetWorlds));

            #endregion

            #region Construct

            app.MapGet(pattern: Constants.Action_GetConstructsCount, handler: async (string token, int worldId, IMediator mediator) =>
              {
                  return await mediator.Send(new GetConstructsCountQuery()
                  {
                      Token = token,
                      WorldId = worldId
                  });

              }).WithName(Constants.GetActionName(Constants.Action_GetConstructsCount));

            app.MapGet(pattern: Constants.Action_GetConstructs, handler: async (string token, int pageIndex, int pageSize, int worldId, IMediator mediator) =>
            {
                return await mediator.Send(new GetConstructsQuery()
                {
                    Token = token,
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    WorldId = worldId
                });

            }).WithName(Constants.GetActionName(Constants.Action_GetConstructs));

            #endregion

            #region Avatar

            app.MapGet(pattern: Constants.Action_GetAvatarsCount, handler: async (string token, int worldId, IMediator mediator) =>
               {
                   return await mediator.Send(new GetAvatarsCountQuery()
                   {
                       Token = token,
                       WorldId = worldId
                   });

               }).WithName(Constants.GetActionName(Constants.Action_GetAvatarsCount));

            app.MapGet(pattern: Constants.Action_GetAvatars, handler: async (string token, int pageIndex, int pageSize, int worldId, IMediator mediator) =>
            {
                return await mediator.Send(new GetAvatarsQuery()
                {
                    Token = token,
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    WorldId = worldId
                });

            }).WithName(Constants.GetActionName(Constants.Action_GetAvatars));

            #endregion

            #region Blob

            app.MapGet(pattern: Constants.Action_GetAsset, handler: async (string token, string fileName, IMediator mediator, ICacheService cacheService) =>
               {
                   string key = fileName;

                   byte[] file = new byte[] { };

                   if (cacheService.IsSet(key))
                   {
                       file = cacheService.Get<byte[]>(key);
                   }
                   else
                   {
                       file = await mediator.Send(new GetAssetQuery()
                       {
                           Token = token,
                           FileName = fileName
                       });

                       cacheService.Set(key, file);
                   }

                   string fileN = fileName.Replace('\\', '_');

                   var resultFile = Microsoft.AspNetCore.Http.Results.File(fileContents: file, contentType: "image/*", fileDownloadName: fileN);

                   return resultFile;

               }).WithName(Constants.GetActionName(Constants.Action_GetAsset));

            app.MapGet(pattern: Constants.Action_GetBlob, handler: async (string token, int id, IMediator mediator, ICacheService cacheService) =>
            {
                string key = token + id;

                byte[] file = new byte[] { };

                if (cacheService.IsSet(key))
                {
                    file = cacheService.Get<byte[]>(key);
                }
                else
                {
                    file = await mediator.Send(new GetBlobQuery()
                    {
                        Token = token,
                        Id = id
                    });

                    cacheService.Set(key, file);
                }

                string fileN = id.ToString();

                var resultFile = Microsoft.AspNetCore.Http.Results.File(fileContents: file, contentType: "text/plain", fileDownloadName: fileN);

                return resultFile;

            }).WithName(Constants.GetActionName(Constants.Action_GetBlob));

            #endregion

            #endregion

            #region Commands

            #region User

            app.MapPost(pattern: Constants.Action_AddUser, handler: async (AddUserCommandRequest command, IMediator mediator) =>
              {
                  return await mediator.Send(new AddUserCommand
                  {
                      FirstName = command.FirstName,
                      LastName = command.LastName,

                      Name = command.Name,
                      Email = command.Email,
                      Password = command.Password,
                      Phone = command.Phone,
                      DateOfBirth = command.DateOfBirth,
                      Gender = command.Gender,
                      ImageUrl = command.ImageUrl,
                  });

              }).WithName(Constants.GetActionName(Constants.Action_AddUser));

            app.MapPost(pattern: Constants.Action_UpdateUser, handler: async (UpdateUserCommandRequest command, IMediator mediator) =>
            {
                return await mediator.Send(new UpdateUserCommand
                {
                    Token = command.Token,
                    Id = command.Id,

                    FirstName = command.FirstName,
                    LastName = command.LastName,

                    Name = command.Name,
                    ImageUrl = command.ImageUrl,
                    Gender = command.Gender,
                    DateOfBirth = command.DateOfBirth,
                    Phone = command.Phone,
                    Email = command.Email,
                    Password = command.Password,
                });

            }).WithName(Constants.GetActionName(Constants.Action_UpdateUser));

            #endregion

            #region World

            app.MapPost(pattern: Constants.Action_AddWorld, handler: async (AddWorldCommandRequest command, IMediator mediator) =>
               {
                   return await mediator.Send(new AddWorldCommand
                   {
                       Token = command.Token,
                       ImageUrl = command.ImageUrl,
                       Name = command.Name,
                   });

               }).WithName(Constants.GetActionName(Constants.Action_AddWorld));

            app.MapPost(pattern: Constants.Action_UpdateWorld, handler: async (UpdateWorldCommandRequest command, IMediator mediator) =>
            {
                return await mediator.Send(new UpdateWorldCommand
                {
                    Token = command.Token,
                    Id = command.Id,

                    ImageUrl = command.ImageUrl,
                    Name = command.Name,
                });

            }).WithName(Constants.GetActionName(Constants.Action_UpdateWorld));

            #endregion

            #region Blob

            app.MapPost(pattern: Constants.Action_SaveBlob, handler: async (SaveBlobCommandRequest command, IMediator mediator) =>
               {
                   return await mediator.Send(new SaveBlobCommand
                   {
                       Token = command.Token,
                       Id = command.Id,
                       DataUrl = command.DataUrl,
                   });

               }).WithName(Constants.GetActionName(Constants.Action_SaveBlob)); 

            #endregion

            #endregion

            return app;
        }
    }
}
