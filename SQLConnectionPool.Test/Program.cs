using System;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace SQLConnectionPool.Test
{
    class Program
    {
        public static SQLConnectionPool<MySqlConnection, MySqlCommand>
            pool = SQLConnectionPool<MySqlConnection, MySqlCommand>.GetInstance(typeof(MySqlConnection), typeof(MySqlCommand));
        static void Main(string[] args)
        {
            SQLConnectionPool<MySqlConnection, MySqlCommand>.min = 1;
            SQLConnectionPool<MySqlConnection, MySqlCommand>.max = 2;
            SQLConnectionPool<MySqlConnection, MySqlCommand>
                .connectionStr = "server=118.126.108.181;port=3306;user=testdb;password=KN5tHtAjAzwYZZ6r;database=testdb;";

            //并发测试
            for (int i = 0; i < 100; i++)
            {
                {
                    SQLConnectionInfo<MySqlConnection> info = pool.GetConnection();

                    MySqlCommand mySqlCommand = new MySqlCommand("SELECT * FROM testtbl", info.conn);

                    int re = (int)mySqlCommand.ExecuteScalar();

                    info.isUse = false;

                    Console.WriteLine(re);
                }

                Task.Run(() =>
                {
                    SQLConnectionInfo<MySqlConnection> info = pool.GetConnection();

                    MySqlCommand mySqlCommand = new MySqlCommand("SELECT * FROM testtbl", info.conn);

                    int re = (int)mySqlCommand.ExecuteScalar();

                    info.isUse = false;

                    Console.WriteLine(re);
                });



            }

            //测试五十次，因为间隔为1秒钟 之前多线程创建的多余连接 会被自行回收
            for (int i = 0; i < 50; i++)
            {
                SQLConnectionInfo<MySqlConnection> info = pool.GetConnection();

                MySqlCommand mySqlCommand = new MySqlCommand("SELECT * FROM testtbl", info.conn);

                int re = (int)mySqlCommand.ExecuteScalar();

                info.isUse = false;

                Console.WriteLine(re);
                Thread.Sleep(1000);
            }

            //测试五十次，设置连接池最小值为1 所以当连接池剩一个连接时 即使低于每秒钟平均次也不会被释放
            for (int i = 0; i < 50; i++)
            {
                SQLConnectionInfo<MySqlConnection> info = pool.GetConnection();

                MySqlCommand mySqlCommand = new MySqlCommand("SELECT * FROM testtbl", info.conn);

                int re = (int)mySqlCommand.ExecuteScalar();

                info.isUse = false;

                Console.WriteLine(re);
                Thread.Sleep(5000);
            }
            Console.ReadKey();
        }
    }
}
