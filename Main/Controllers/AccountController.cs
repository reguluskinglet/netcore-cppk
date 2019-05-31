using System;
using System.Data.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Rzdppk.Core.Extensions;
using Rzdppk.Core.Options;
using Rzdppk.Core.Services.Interfaces;
using Rzdppk.Core.ViewModels;


namespace Rzdppk.Controllers
{
    /// <summary>
    /// Test Git
    /// </summary>
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger _logger;
        //private readonly IDb _db;
        //private readonly IMemoryCache _memoryCache;


        public AccountController
            (
                IAuthService authService,
                ILogger<AccountController> logger
            //IDb db,
            //IMemoryCache memoryCache
            )
        {
            _authService = authService;
            _logger = logger;

            //_db = db;
            //_memoryCache = memoryCache;
        }






        [HttpPost("/token")]
        public async Task<IActionResult> Token()
        {
            try
            {
                var model = Request.ContentType.Contains("application/json") 
                    ? Request.Body.ToJson<AuthModel>()
                    : new AuthModel(Request.Form["login"], Request.Form["password"]);

                var  user = await _authService.FindByLoginAsync(model.Login, model.Password, true);

                if (user == null)
                    return Unauthorized();

                var identity = _authService.GetIdentity(user);

                var now = DateTime.UtcNow;

                var jwt = new JwtSecurityToken(
                        notBefore: now,
                        claims: identity.Claims,
                        expires: now.Add(TimeSpan.FromDays(Constants.LifTime)),
                        signingCredentials: new SigningCredentials(Constants.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

                var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
                var userInfo = _authService.GetUserInfo(user);
                if (userInfo.Name == null)
                    userInfo.Name = user.Login;


                var response = new
                {
                    access_token = encodedJwt,
                    user_info = userInfo
                };

                return Ok(response);
            }
            catch (Exception)
            {
                return Unauthorized();
            }
        }
    }
}
