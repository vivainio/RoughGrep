using System;
using System.Collections.Generic;
using System.Linq;

// this is local copy of https://github.com/vivainio/trivialbehinds

namespace TrivialBehind
{
    public static class TrivialBehinds
    {
        static Dictionary<Type, Type> registeredBehinds = new Dictionary<Type, Type>();
        static List<(Type, object)> createdBehinds = new List<(Type, object)>();

        class BDisposer : IDisposable
        {
            private readonly object obj;
            internal BDisposer(object toDispose)
            {
                this.obj = toDispose;
            }
            public void Dispose()
            {
                createdBehinds.RemoveAll(d => d.Item2 == this.obj);
            }
        }
        public static void RegisterBehind<TUi, TBehind>()
        {
            registeredBehinds.Add(typeof(TUi), typeof(TBehind));
        }

        // returns the disposer that removes this from list
        public static IDisposable CreateForUi<TUi>(TUi ui)
        {
            var handler = registeredBehinds[typeof(TUi)];
            var ctor = handler.GetConstructor(new[] { typeof(TUi) });
            var instance = ctor.Invoke(new[] { (object) ui });
            createdBehinds.Add((handler, instance));
            return new BDisposer(instance);
        }
        // should not be called from form side (since it shouldn't know behind types
        public static TBehind[] FindBehinds<TBehind>()
        {
            var needle = typeof(TBehind);
            var res = createdBehinds.Where(e => e.Item1 == needle).Select(e => e.Item2).Cast<TBehind>().ToArray();
            return res;
        }
    }
}
