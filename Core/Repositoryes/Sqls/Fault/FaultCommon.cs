using System;
using System.Collections.Generic;
using System.Text;

namespace Rzdppk.Core.Repositoryes.Sqls.Fault
{
    class FaultCommon
    {
        public static string sqlCommon = @"SELECT * FROM Faults ";
        public static string sqlCountCommon = @"SELECT count(*) FROM Faults ";
    }
}
