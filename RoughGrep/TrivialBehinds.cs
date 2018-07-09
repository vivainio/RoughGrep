using System;
using System.Collections.Generic;
using System.Linq;

namespace TrivialBehind
{
    struct StoredBehind
    {
        public Type DataType;
        public object BehindInstance;
        public object Creator; // e.g. the form instance that created the behind
    }
    public static class TrivialBehinds
    {
        static Dictionary<Type, Type> registeredBehinds = new Dictionary<Type, Type>();
        static List<StoredBehind> createdBehinds = new List<StoredBehind>();

        class BDisposer : IDisposable
        {
            private readonly object obj;
            internal BDisposer(object toDispose)
            {
                this.obj = toDispose;
            }
            public void Dispose()
            {
                createdBehinds.RemoveAll(d => d.BehindInstance == this.obj);
            }
        }
        // register behind for later creation with CreateBehind
        public static void RegisterBehind<TData, TBehind>()
        {
            registeredBehinds.Add(typeof(TData), typeof(TBehind));
        }

        // creates a behind object for corresponding TData type
        // returns the disposer that removes this from list
        public static IDisposable CreateBehind<TData>(object creator, TData ui)
        {
            Type handler;
            var ok = registeredBehinds.TryGetValue(typeof(TData), out handler);
            if (!ok)
            {
                throw new ArgumentException($"Behind handler for {ui} not found");
            }
            var ctor = handler.GetConstructor(new[] { typeof(TData) });
            var instance = ctor.Invoke(new[] { (object) ui });
            createdBehinds.Add(
                new StoredBehind
                {
                    DataType = handler,
                    BehindInstance = instance,
                    Creator = creator
                });
            return new BDisposer(instance);
        }
        // should not be called from form side (since it shouldn't know behind types)
        public static TBehind[] BehindsByType<TBehind>()
        {
            var needle = typeof(TBehind);
            var res = createdBehinds.Where(e => e.DataType == needle).Select(e => e.BehindInstance).Cast<TBehind>().ToArray();
            return res;
        }

        public static TBehind BehindFor<TBehind>(object creator) where TBehind: class =>
            createdBehinds.First(b => b.Creator == creator) as TBehind;
    }
}
