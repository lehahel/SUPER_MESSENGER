using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Dapper;

namespace WindowsFormsApplication1
{
    class DataRepository
    {
        string connectionString = @"Data Source=KOMAR_PC\SQLEXPRESS;Initial Catalog=socklog;Integrated Security=True;";
        public List<User> GetUsers()
        {
            List<User> users = new List<User>();
            try
            {
                using (IDbConnection db = new SqlConnection(connectionString))
                {
                    return db.Query<User>("SELECT top 10 * FROM Users ORDER BY id DESC").ToList();
                }
            }
            catch
            {
                return null;
            }
        }

        public void Create(User user)
        {
            try
            {
                using (IDbConnection db = new SqlConnection(connectionString))
                {
                    var sqlQuery = "INSERT INTO Users (User_ip, Username, Message, Date) VALUES(@User_ip, @Username, @Message, @Date);";
                    db.Query(sqlQuery, user);
                }
            }
            catch
            {
                return;
            }
        }
    }
}
