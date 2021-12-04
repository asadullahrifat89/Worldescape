using System;
using System.Threading.Tasks;
using Worldescape.Common;

namespace Worldescape.Service
{
    public class UserRepository
    {
        readonly HttpServiceHelper _httpServiceHelper;

        public UserRepository(HttpServiceHelper httpServiceHelper)
        {
            _httpServiceHelper = httpServiceHelper;
        }

       /// <summary>
       /// Gets a user information from the provided params.
       /// </summary>
       /// <param name="token"></param>
       /// <param name="email"></param>
       /// <param name="password"></param>
       /// <returns></returns>
        public async Task<RepositoryResponse> GetUser(string token, string email, string password)
        {
            var user = await _httpServiceHelper.SendGetRequest<User>(
                   actionUri: Constants.Action_GetUser,
                   payload: new GetUserQueryRequest() { Token = token, Email = email, Password = password });

            var success = user != null && !user.IsEmpty();

            return RepositoryResponse.BuildResponse(
                   success: success,
                   result: user,
                   error: success ? null : "Failed to login.");
        }
      
        /// <summary>
        /// Adds a user.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="dateofbirth"></param>
        /// <param name="gender"></param>
        /// <param name="firstname"></param>
        /// <param name="lastname"></param>
        /// <returns></returns>
        public async Task<RepositoryResponse> AddUser(
            string email,
            string password,
            DateTime dateofbirth,
            Gender gender,
            string firstname,
            string lastname)
        {
            var command = new AddUserCommandRequest
            {
                Email = email,
                Password = password,
                DateOfBirth = dateofbirth,
                Gender = gender,
                FirstName = firstname,
                LastName = lastname,
                Name = firstname + " " + lastname,
            };

            var response = await _httpServiceHelper.SendPostRequest<ServiceResponse>(
                actionUri: Constants.Action_AddUser,
                payload: command);

            return RepositoryResponse.BuildResponse(
                   success: response.HttpStatusCode == System.Net.HttpStatusCode.OK && response.ExternalError.IsNullOrBlank(),
                   result: System.Net.HttpStatusCode.OK,
                   error: response.ExternalError);
        }

        /// <summary>
        /// Updates a user.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="id"></param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="dateofbirth"></param>
        /// <param name="gender"></param>
        /// <param name="firstname"></param>
        /// <param name="lastname"></param>
        /// <param name="imageUrl"></param>
        /// <returns></returns>
        public async Task<RepositoryResponse> UpdateUser(
            string token,
            int id,
            string email,
            string password,
            DateTime dateofbirth,
            Gender gender,
            string firstname,
            string lastname,
            string imageUrl)
        {
            var command = new UpdateUserCommandRequest
            {
                Token = token,
                Id = id,
                Email = email,
                Password = password,
                DateOfBirth = dateofbirth,
                Gender = gender,
                FirstName = firstname,
                LastName = lastname,
                Name = firstname + " " + lastname,
                ImageUrl = imageUrl,
            };

            var response = await _httpServiceHelper.SendPostRequest<ServiceResponse>(
                actionUri: Constants.Action_UpdateUser,
                payload: command);

            return RepositoryResponse.BuildResponse(
                   success: response.HttpStatusCode == System.Net.HttpStatusCode.OK && response.ExternalError.IsNullOrBlank(),
                   result: System.Net.HttpStatusCode.OK,
                   error: response.ExternalError);
        }
    }
}
