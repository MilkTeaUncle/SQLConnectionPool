using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace SQLConnectionPool
{
    public interface ISQLConnectionPool
    {
        SQLConnectionInfo GetConnection();
    }
}
