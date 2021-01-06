using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using MySql.Data.MySqlClient;

namespace SQLConnectionPool
{
    public class SQLConnectionInfo<T> where T : DbConnection
    {

        public SQLConnectionInfo(Type type,string connectionStr)
        {

            conn = (T)Activator.CreateInstance(type);
            conn.ConnectionString = connectionStr;
        }

        public DateTime time = DateTime.Now;

        public bool isUse { get; set; }


        public int connCount { get; set; }

        public bool isExpired
        {
            get
            {
                if (isUse)
                    return false;

                double minutesCount = (double)connCount / (double)(DateTime.Now - this.time).TotalSeconds;
                return (minutesCount < 1) ? true : false;
            }
            set
            {
                this.isExpired = false;
            }

        }

        public T conn { get; set; }
    }
}
