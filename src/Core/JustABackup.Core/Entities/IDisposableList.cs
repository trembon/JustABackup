using System;
using System.Collections.Generic;
using System.Text;

namespace JustABackup.Core.Entities
{
    public class IDisposableList : List<IDisposable>
    {
        public T CreateAndAdd<T>() where T : IDisposable, new()
        {
            return CreateAndAdd<T>(() => new T());
        }

        public T CreateAndAdd<T>(Func<T> createInstance) where T : IDisposable
        {
            T item = createInstance();
            this.Add(item);
            return item;
        }
    }
}
