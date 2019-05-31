using System;
using Rzdppk.Core.Options;

namespace Rzdppk.Controllers.Base
{
    public class BaseExternalController : BaseController
    {
        protected void CheckApiKey()
        {
            var res = false;
            if (Request.Headers.ContainsKey("ApiKey"))
            {
                var apikey = Request.Headers["ApiKey"];
                if (AppSettings.ApiKey == apikey)
                {
                    res = true;
                }
            }

            if (!res)
            {
                throw new Exception("no permission to access");
            }
        }
    }
}
