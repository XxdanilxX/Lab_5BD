using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lb_5BD
{
    internal class DBUtils
    {
        public static MySqlConnection GetDBConnection()
        {
            string host = "localhost";
            int port = 3306;
            string database = "LB2";
            string username = "monty";
            string password = "some_pass";

            return DBMySQLUtils.GetDBConnection(host, port, database, username, password);
        }

    }
}
