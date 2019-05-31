using System;
using System.Collections.Generic;
using System.Text;

namespace Rzdppk.Core.Repositoryes.Sqls.Train
{
    class TrainCommon
    {
        public static string sqlCommon = @"SELECT t.*,
CAST(
   CASE WHEN EXISTS(SELECT * FROM Carriages c WHERE c.TrainId=t.Id) THEN 0
   ELSE 1
   END 
AS BIT) as CanDelete, s.*
FROM Trains as t
LEFT JOIN Stantions as s ON t.StantionId = s.Id
";


        public static string sqlCountCommon = @"SELECT count(*) FROM Trains as t ";
    }
}
