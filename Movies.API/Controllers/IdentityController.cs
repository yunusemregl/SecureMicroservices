using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using IdentityModel.Client;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Movies.API.ApiService;

namespace Movies.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class IdentityController : ControllerBase
    {
        private readonly IIdentityApiService _identityApiService;

        public IdentityController(IIdentityApiService identityApiService)
        {
            _identityApiService = identityApiService ?? throw new ArgumentNullException(nameof(identityApiService));
        }

        [HttpGet]
        public IActionResult Get()
        {
            return new JsonResult(from c in User.Claims select new { c.Type, c.Value });
        }


        public async Task<Dictionary<string, string>> GetUserInfo()
        {
            return await _identityApiService.GetUserInfo();

        }
    }
}
