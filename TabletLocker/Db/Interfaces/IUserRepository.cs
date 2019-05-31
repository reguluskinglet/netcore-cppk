using System.Collections.Generic;
using TabletLocker.Model;

namespace TabletLocker.Db
{
    public interface IUserRepository
    {
        User FindAdminByLogin(string login, string password);

        User GetUserById(int id);

        List<User> GetUsersByBarcode(string barcode);
    }
}