using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Base;

namespace Rzdppk.Model.Auth
{
    public class UserRole : BaseEntity
    {
        public string Name { get; set; }

        public int Permissions { get; set; }
    }
}
