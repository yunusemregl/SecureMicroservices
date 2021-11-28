using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Movies.Client.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Movies.Client.ApiServices
{
    public class MovieApiService : IMovieApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MovieApiService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task<UserInfoViewModel> GetUserInfo()
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

            return new UserInfoViewModel(userDictionary);

        }

        public async Task<Movie> CreateMovie(Movie movie)
        {
            var httpClient = _httpClientFactory.CreateClient("MovieAPIClient");

            JsonContent content = JsonContent.Create(movie);

            var request = new HttpRequestMessage(HttpMethod.Post, $"/movies");
            request.Content = content;

            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            if (response.StatusCode != System.Net.HttpStatusCode.Created)
            {
                throw new HttpRequestException("Something went wrong.");
            }


            var respContent = await response.Content.ReadAsStringAsync();

            var newMovie = JsonConvert.DeserializeObject<Movie>(respContent);

            return newMovie;
        }

        public async Task DeleteMovie(int id)
        {
            var httpClient = _httpClientFactory.CreateClient("MovieAPIClient");

            var response = await httpClient.DeleteAsync($"/movies/{id}").ConfigureAwait(false);

            if (response.StatusCode != System.Net.HttpStatusCode.NoContent)
            {
                throw new HttpRequestException("Something went wrong.");
            }
        }

        public async Task<Movie> GetMovie(string id)
        {
            var httpClient = _httpClientFactory.CreateClient("MovieAPIClient");

            var request = new HttpRequestMessage(HttpMethod.Get, $"/movies/{id}");

            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var movie = JsonConvert.DeserializeObject<Movie>(content);

            return movie;
        }

        public async Task<IEnumerable<Movie>> GetMovies()
        {

            var httpClient = _httpClientFactory.CreateClient("MovieAPIClient");

            var request = new HttpRequestMessage(HttpMethod.Get, "/movies");

            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            // 3- Deserialize Object to MovieList
            var movieList = JsonConvert.DeserializeObject<List<Movie>>(content);

            return movieList;
            #region old

            // 1- Get Token from Identity Server, of course we should provide IS configuration like 
            // 2- Send request to Protected API
            // 3- Deserialize Object to MovieList



            //#region Get Token from the Identity Server
            //// 1. "retrieve" our api credentials. This must be registered on Identity Server!
            //var apiClientCredentials = new ClientCredentialsTokenRequest()
            //{
            //    Address = "https://localhost:5005/connect/token",

            //    ClientId = "movieClient",
            //    ClientSecret = "secret",

            //    // this is the scope our Protected API requires.
            //    Scope = "movieAPI"
            //};

            //// Creates a new HttpClient to talk our IdentityServer (localhost:5005)
            //var client = new HttpClient();

            //// just check if we can reach the Discovery Document. not 100% needed but..
            //var disco = await client.GetDiscoveryDocumentAsync("https://localhost:5005");
            //if (disco.IsError)
            //{
            //    return null;// throw 500 error
            //}


            //// 2.Authenticated and get an access token from Identity Server
            //var tokenResponse = await client.RequestClientCredentialsTokenAsync(apiClientCredentials);
            //if (tokenResponse.IsError)
            //{
            //    return null;// throw 500 error
            //}
            //#endregion


            //// 2- Send request to Protected API

            //// Another HTtpClidnt for taking with our Protected API
            //var apiClient = new HttpClient();

            //// Set he access_token in the request Authorization: Bearer <token>
            //apiClient.SetBearerToken(tokenResponse.AccessToken);

            //// Send a request to our Protected API
            //var response = await apiClient.GetAsync("https://localhost:5001/movies");
            //response.EnsureSuccessStatusCode();

            //var content = await response.Content.ReadAsStringAsync();

            //// 3- Deserialize Object to MovieList
            //var movieList = JsonConvert.DeserializeObject<List<Movie>>(content);

            //return movieList; 
            #endregion
        }


        public async Task<Movie> UpdateMovie(Movie movie)
        {
            var httpClient = _httpClientFactory.CreateClient("MovieAPIClient");

            JsonContent content = JsonContent.Create(movie);
            var request = new HttpRequestMessage(HttpMethod.Post, $"/movies/{movie.Id}");
            request.Content = content;

            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            if (response.StatusCode != System.Net.HttpStatusCode.NoContent)
            {
                throw new HttpRequestException("Something went wrong.");
            }

            return await this.GetMovie(movie.Id.ToString());

        }
    }
}
