using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Base;

namespace Rzdppk.Model.Auth
{
    public class Permission : BaseEntity
    {
        public string Controller { get; set; }
        public string Action { get; set; }
        public int PermissionBits { get; set; }
    }
}
