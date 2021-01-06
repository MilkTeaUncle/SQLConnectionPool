using System;
using System.Collections.Generic;
using System.Text;

namespace SQLConnectionPool
{
    public class MyList<T> : List<T>
    {
        private static readonly object Monitor = new object();
        public bool MyRemove(T item)
        {
            lock (Monitor)
            {
                return base.Remove(item);
            }
        }

        public void MyAdd(T item)
        {
            lock (Monitor)
            {
                base.Add(item);
            }
        }

    }
}
