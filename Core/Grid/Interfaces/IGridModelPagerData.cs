using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Core.Grid
{
    public interface IGridModelPagerData
    {
        Task<LazyPagination> GetPager(IDbConnection connection, string condition, object parameters, int page, int pageSize, bool hasFilter);
    }
}