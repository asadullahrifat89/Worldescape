﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Worldescape.Common;

namespace Worldescape.Service
{
    public class ApiTokenRepository
    {
        readonly HttpServiceHelper _httpServiceHelper;

        public ApiTokenRepository(HttpServiceHelper httpServiceHelper)
        {
            _httpServiceHelper = httpServiceHelper;
        }

        /// <summary>
        /// Get ApiToken for the provided credentials.
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public async Task<RepositoryResponse> GetApiToken(string email, string password)
        {
            var response = await _httpServiceHelper.SendGetRequest<StringResponse>(
              actionUri: Constants.Action_GetApiToken,
              payload: new GetApiTokenQueryRequest { Email = email, Password = password,  });

            return RepositoryResponse.BuildResponse(
                   success: response.HttpStatusCode == System.Net.HttpStatusCode.OK && response.ExternalError.IsNullOrBlank(),
                   result: response.Response,
                   error: response.ExternalError);
        }
    }
}
