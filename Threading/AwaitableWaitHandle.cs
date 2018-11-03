using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoreFramework.CFThreading
{
    public static class AwaitableWaitHandleExtensions
    {
        public static Task ToTask(this WaitHandle handle)
        {
            if (handle == null) throw new ArgumentNullException("handle");

            var tcs = new TaskCompletionSource<object>();
            RegisteredWaitHandle shared = null;
            RegisteredWaitHandle produced = ThreadPool.RegisterWaitForSingleObject(
                handle,
                (state, timedOut) =>
                {
                    tcs.SetResult(null);

                    while (true)
                    {
                        RegisteredWaitHandle consumed = Interlocked.CompareExchange(ref shared, null, null);
                        if (consumed != null)
                        {
                            consumed.Unregister(null);
                            break;
                        }
                    }
                },
                state: null,
                millisecondsTimeOutInterval: Timeout.Infinite,
                executeOnlyOnce: true);

            // Publish the RegisteredWaitHandle so that the callback can see it.
            Interlocked.CompareExchange(ref shared, produced, null);

            return tcs.Task;
        }
    }   
}
