using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Rzdppk.Core.Options
{
    public class Constants
    {
        public const string Issuer = "RzdAuthServer";
        private const string Key = "5427b144-892c-4ebc-9c95-8f2494fe494a";   // ключ для шифрации

        public const int LifTime = 365; //Время жизни токена

        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Key));
        }
    }
}
