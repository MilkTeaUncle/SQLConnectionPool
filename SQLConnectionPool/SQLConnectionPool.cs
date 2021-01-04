using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SQLConnectionPool.Test
{
    public class SQLConnectionPool : ISQLConnectionPool
    {
        public static SQLConnectionPool sQLConnectionPool;

        SQLConnectionPool()
        {
            //Task.Run(() =>
            //{
            //    this.ReleaseConnection();
            //});
        }

        public static SQLConnectionPool GetInstance()
        {
            if (sQLConnectionPool == null)
            {
                lock (Monitor)
                {
                    if (sQLConnectionPool == null)
                    {
                        sQLConnectionPool = new SQLConnectionPool();
                    }
                }
            }
            return sQLConnectionPool;
        }


        public static string connectionStr { get; set; }
        public static int min { get; set; } = 5;
        public static int max { get; set; } = 10;

        private static List<SQLConnectionInfo> sQLConnectionInfos = new List<SQLConnectionInfo>();


        private static readonly object Monitor = new object();





        /// <summary>
        /// 从数据库连接池里获取连接 如果获取不到则 进入等待 （如果必要请自行写入超时）
        /// </summary>
        /// <returns></returns>
        public SQLConnectionInfo GetConnection()
        {
            lock (Monitor)
            {
                SQLConnectionInfo connInfo = null;
                bool canConn = false;

                ReleaseConnection();

                if (sQLConnectionInfos.Count <= max)
                {
                    while (!canConn)
                    {
                        connInfo = sQLConnectionInfos.Find(s => !s.isUse);

                        if (connInfo != null)
                        {
                            if (connInfo.conn.TestConnection())
                            {
                                connInfo.isUse = true;
                                connInfo.connCount++;
                                canConn = true;
                            }
                            else
                            {
                                sQLConnectionInfos.Remove(connInfo);
                            }
                        }
                        else if (sQLConnectionInfos.Count < max)
                        {
                            SQLConnectionInfo newSQLConnectionInfo = new SQLConnectionInfo { isUse = false, conn = new MySqlConnection(connectionStr) };
                            newSQLConnectionInfo.conn.Open();
                            if (newSQLConnectionInfo.conn.TestConnection())
                            {
                                newSQLConnectionInfo.isUse = true;
                                newSQLConnectionInfo.connCount++;

                                sQLConnectionInfos.Add(newSQLConnectionInfo);

                                connInfo = newSQLConnectionInfo;
                                canConn = true;
                            }
                        }
                    }



                }




                return connInfo;
            }


        }

        /// <summary>
        /// 释放 每秒平均次数 低于1的连接 如果该连接处于连接状态则不释放
        /// </summary>
        private void ReleaseConnection()
        {

            Console.WriteLine($"--------------------------------目前连接池还剩余：{sQLConnectionInfos.Count}--------------------------------");

            SQLConnectionInfo sQLConnectionInfo = sQLConnectionInfos.Find(s => s.isExpired && sQLConnectionInfos.Count > min && !s.isUse);
            sQLConnectionInfos.Remove(sQLConnectionInfo);
            sQLConnectionInfos.ForEach(s =>
            {
                double time = (double)(DateTime.Now - s.time).TotalSeconds;
                Console.WriteLine($"该连接已被使用次数：{s.connCount}     是否被连接占用：{s.isUse}     " +
                    $"次（{s.connCount}）/ 秒（{time}） = 每秒钟平均次（ {(double)s.connCount / time})     每秒平均次低于 1 该连接会被释放");
            });

        }



    }

    /// <summary>
    /// 扩展方法 用于判断该连接是否可用
    /// </summary>
    public static class SQLConnectionPoolEx
    {
        public static bool TestConnection(this MySqlConnection conn)
        {
            bool result = true;
            var cmd = new MySqlCommand("select 1", conn);
            try
            {
                cmd.ExecuteScalar().ToString();
            }
            catch
            {
                result = false;
            }
            return result;
        }
    }
}
