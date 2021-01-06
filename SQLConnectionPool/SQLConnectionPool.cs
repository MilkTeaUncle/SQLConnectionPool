using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace SQLConnectionPool.Test
{
    public class SQLConnectionPool<T, Y> where T : DbConnection where Y : DbCommand
    {
        public static SQLConnectionPool<T, Y> sQLConnectionPool;
        private Type sqlCoon;
        private Type sqlCmd;

        SQLConnectionPool(Type sqlCoon, Type sqlCmd)
        {
            this.sqlCoon = sqlCoon;
            this.sqlCmd = sqlCmd;
            Task.Run(() =>
            {
                ReleaseConnection();
            });
        }
        public static SQLConnectionPool<T, Y> GetInstance(Type sqlCoon, Type sqlCmd)
        {
            if (sQLConnectionPool == null)
            {
                lock (Monitor)
                {
                    if (sQLConnectionPool == null)
                    {
                        sQLConnectionPool = new SQLConnectionPool<T, Y>(sqlCoon, sqlCmd);
                    }
                }
            }
            return sQLConnectionPool;
        }
        public static string connectionStr { get; set; }
        public static int min { get; set; } = 5;
        public static int max { get; set; } = 10;

        private static MyList<SQLConnectionInfo<T>> SQLConnectionInfos = new MyList<SQLConnectionInfo<T>>();


        private static readonly object Monitor = new object();






        /// <summary>
        /// 从数据库连接池里获取连接 如果获取不到则 进入等待 （如果必要请自行写入超时）
        /// </summary>
        /// <returns></returns>
        public SQLConnectionInfo<T> GetConnection()
        {
            lock (Monitor)
            {
                SQLConnectionInfo<T> connInfo = null;
                bool canConn = false;

                if (SQLConnectionInfos.Count <= max)
                {
                    while (!canConn)
                    {
                        connInfo = SQLConnectionInfos.Find(s => !s.isUse);

                        if (connInfo != null)
                        {
                            if (TestConnection(connInfo.conn))
                            {
                                connInfo.isUse = true;
                                connInfo.connCount++;
                                canConn = true;
                            }
                            else
                            {
                                SQLConnectionInfos.MyRemove(connInfo);
                            }
                        }
                        else if (SQLConnectionInfos.Count < max)
                        {
                            SQLConnectionInfo<T> newSQLConnectionInfo = new SQLConnectionInfo<T>(this.sqlCoon, connectionStr) { isUse = false };
                            newSQLConnectionInfo.conn.Open();
                            if (TestConnection(newSQLConnectionInfo.conn))
                            {
                                newSQLConnectionInfo.isUse = true;
                                newSQLConnectionInfo.connCount++;

                                SQLConnectionInfos.MyAdd(newSQLConnectionInfo);

                                connInfo = newSQLConnectionInfo;
                                canConn = true;
                            }
                        }
                        else
                        {
                            Thread.Sleep(1000);
                        }

                    }



                }




                return connInfo;
            }


        }

        public bool TestConnection(T conn)
        {
            bool result = true;
            try
            {
                Y mySqlCommand = (Y)Activator.CreateInstance(sqlCmd);
                mySqlCommand.Connection = conn;
                mySqlCommand.CommandText = "select 1";
                mySqlCommand.ExecuteScalar().ToString();
            }
            catch
            {
                result = false;
            }
            return result;
        }


        /// <summary>
        /// 释放 每秒平均次数 低于1的连接 如果该连接处于连接状态则不释放
        /// </summary>
        private static void ReleaseConnection()
        {
            while (true)
            {
                Console.WriteLine($"--------------------------------Pool Connection Count：{SQLConnectionInfos.Count}--------------------------------");

                SQLConnectionInfo<T> SQLConnectionInfo = SQLConnectionInfos.Find(s => s.isExpired && SQLConnectionInfos.Count > min && !s.isUse);
                SQLConnectionInfos.MyRemove(SQLConnectionInfo);

                SQLConnectionInfos.ForEach(s =>
                {
                    double time = (double)(DateTime.Now - s.time).TotalSeconds;
                    Console.WriteLine($"Use Count：{s.connCount}     Is Use：{s.isUse}     " +
                        $"（{s.connCount}）Count / （{time}）S  = Count Per Second（ {(double)s.connCount / time})     每秒平均次低于 1 该连接会被释放");
                });
                Thread.Sleep(1000);
            }


        }








    }


}
