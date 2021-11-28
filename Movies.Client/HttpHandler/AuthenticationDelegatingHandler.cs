using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Movies.Client.HttpHandler
{
    public class AuthenticationDelegatingHandler : DelegatingHandler
    {
        //private readonly IHttpClientFactory _httpClientFactory;
        //private readonly ClientCredentialsTokenRequest _tokenRequest;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthenticationDelegatingHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            #region Sending Token request for each request and add http call header authorization
            //var httpClient = _httpClientFactory.CreateClient("IDPClient");

            //var tokenResponse = await httpClient.RequestClientCredentialsTokenAsync(_tokenRequest);
            //if (tokenResponse.IsError)
            //{
            //    throw new HttpRequestException("Something went wrong while requesting access token");
            //}request.SetBearerToken(tokenResponse.AccessToken);

            #endregion

            #region Get Existing Token and set to request header as Authorization param.
            string accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
            if (!string.IsNullOrEmpty(accessToken))
            {
                request.SetBearerToken(accessToken);
            }

            #endregion

            return await base.SendAsync(request, cancellationToken);
        }


    }
}
