using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using NLog;
using Rzdppk.Core.Helpers;
using TabletLocker.Model;

namespace TabletLocker.Db
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public UserRepository()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["LocalDbConnString"].ConnectionString;
        }

        public User FindAdminByLogin(string login, string password)
        {
            var user = GetUserByLogin(login);

            if (user == null || !user.IsAdmin || !CryptoHelper.VerifyHashedPassword(user.PasswordHash, password))
                return null;

            return user;
        }

        public User GetUserByLogin(string login)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                _logger.Debug($"GetUserByLogin SQL: {SqlFindByLogin}, Login={login}");
                return db.QuerySingleOrDefault<User>(SqlFindByLogin , new {Login = login});
            }
        }

        public User GetUserById(int id)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                _logger.Debug($"GetUserById SQL: {SqlFindById}, Id={id}");
                return db.QuerySingleOrDefault<User>(SqlFindById, new { Id = id });
            }
        }

        public List<User> GetUsersByBarcode(string barcode)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                _logger.Debug($"GetUserByBarcode SQL: {SqlFindByBarcode}, Barcode={barcode}");
                return db.Query<User>(SqlFindByBarcode, new { Barcode = barcode }).ToList();
            }
        }

        public static string GetShortFio(string fio)
        {
            var ret = "?";
            if (fio != null)
            {
                var tokens = fio.Split(' ');
                if (tokens.Length > 0)
                {
                    ret = tokens[0]; //фамилия
                    if (tokens.Length > 1) //инициалы
                    {
                        ret += $" {tokens[1].Substring(0, 1).ToUpper()}.";
                        if (tokens.Length > 2)
                        {
                            ret += $"{tokens[2].Substring(0, 1).ToUpper()}.";
                        }
                    }
                }
            }

            return ret;
        }

        private const string SqlFindByLogin = "SELECT * FROM [Users] WHERE [IsBlocked]=0 AND login=@Login";
        private const string SqlFindById = "SELECT * FROM [Users] WHERE [Id]=@Id";
        private const string SqlFindByBarcode = "SELECT * FROM [Users] WHERE [IsBlocked]=0 AND [PersonNumber]=@Barcode AND IsAdmin=0";
    }
}
