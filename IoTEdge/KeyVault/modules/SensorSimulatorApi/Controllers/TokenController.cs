using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Mvc;
using util;

namespace SensorSimulatorApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<string> Get(string username, string password)
        {
            string token = "";

            try
            {
                // if (username == "user" && password == "pass")
                // {
                //     var tokenExpiration = TimeSpan.FromMinutes(10);
                //     ClaimsIdentity identity = new ClaimsIdentity("Basic");
                //     identity.AddClaim(new Claim(ClaimTypes.Name, username));
                //     identity.AddClaim(new Claim("role", "user"));

                //     var props = new AuthenticationProperties()
                //     {
                //         IssuedUtc = DateTime.UtcNow,
                //         ExpiresUtc = DateTime.UtcNow.Add(tokenExpiration),
                //     };

                //     var ticket = new AuthenticationTicket(identity, props);
                //     var accessToken = Startup.OAuthBearerOptions.AccessTokenFormat.Protect(ticket);
                // }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return token;
        }
    }
}