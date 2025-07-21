using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public static class CacheFactory
    {
        public static Func<Task<T?>> Create<T>(Func<Task<T?>> loader)
        {
            return loader;
        }

        public static Func<Task<T?>> Create<T, TKey>(TKey id, Func<TKey, Task<T?>> loader)
        {
            return () => loader(id);
        }
    }
}
