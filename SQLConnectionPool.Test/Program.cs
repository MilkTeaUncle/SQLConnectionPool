using System;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace SQLConnectionPool.Test
{
    class Program
    {
        public static SQLConnectionPool pool = SQLConnectionPool.GetInstance();
        static void Main(string[] args)
        {
            SQLConnectionPool.min = 1;
            SQLConnectionPool.max = 2;
            SQLConnectionPool.connectionStr = "server=127.0.0.1;port=3306;user=testdb;password=123;database=testdb;";

            //并发测试
            for (int i = 0; i < 100; i++)
            {
                Task.Run(() =>
                {
                    SQLConnectionInfo info = pool.GetConnection();

                    MySqlCommand mySqlCommand = new MySqlCommand("SELECT * FROM testtbl", info.conn);

                    int re = (int)mySqlCommand.ExecuteScalar();

                    info.isUse = false;

                    Console.WriteLine(re);
                });

                

            }

            //测试五十次，因为间隔为1秒钟 之前多线程创建的多余连接 会被自行回收
            for (int i = 0; i < 50; i++)
            {
                SQLConnectionInfo info = pool.GetConnection();

                MySqlCommand mySqlCommand = new MySqlCommand("SELECT * FROM testtbl", info.conn);

                int re = (int)mySqlCommand.ExecuteScalar();

                info.isUse = false;

                Console.WriteLine(re);
                Thread.Sleep(1000);
            }

            //测试五十次，设置连接池最小值为1 所以当连接池剩一个连接时 即使低于每秒钟平均次也不会被释放
            for (int i = 0; i < 50; i++)
            {
                SQLConnectionInfo info = pool.GetConnection();

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
