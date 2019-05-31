using System;
using System.Collections.Generic;
using System.Text;

namespace Rzdppk.Core.Repositoryes.Sqls.Parkings
{
    class ParkingCommon
    {
        public static string sqlCommon = @"SELECT p.*, s.*
FROM Parkings AS p
LEFT JOIN Stantions as s ON p.StantionId = s.Id";

        public static string sqlCountCommon = @"SELECT count(*) FROM Parkings as p";

        public static string SqlQueryPagingEndT = @"ORDER by p.Name OFFSET @skip ROWS FETCH NEXT @limit ROWS ONLY";
    }
}
