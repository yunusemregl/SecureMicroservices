using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Movies.API.ApiService
{
    public class IdentityApiService : IIdentityApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IdentityApiService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task<Dictionary<string, string>> GetUserInfo()
        {
            var idpClient = _httpClientFactory.CreateClient("IDPClient");

            var metaDataResponse = await idpClient.GetDiscoveryDocumentAsync();
            if (metaDataResponse.IsError)
            {
                throw new HttpRequestException("Something went wrong while requesting the access token");
            }

            var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);


            var userInfoResponse = await idpClient.GetUserInfoAsync(new UserInfoRequest()
            {
                Address = metaDataResponse.UserInfoEndpoint,
                Token = accessToken
            });
            if (userInfoResponse.IsError)
            {
                throw new HttpRequestException("Something went wrong while requesting the access token");
            }

            var userDictionary = new Dictionary<string, string>();
            foreach (var item in userInfoResponse.Claims)
            {
                userDictionary.Add(item.Type, item.Value);
            }

            return new Dictionary<string, string>(userDictionary);
        }
    }
}
