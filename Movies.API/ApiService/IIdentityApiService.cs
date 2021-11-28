using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Movies.API.ApiService
{
    public interface IIdentityApiService
    {
        Task<Dictionary<string,string>> GetUserInfo();
    }
}
