using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;

namespace SQLConnectionPool
{
    public class SQLConnectionInfo
    {
        public SQLConnectionInfo()
        {

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

        public MySqlConnection conn { get; set; }
    }
}
