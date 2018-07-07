using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoreFramework.CFThreading
{
    public class TasksWaitHandle : WaitHandle
    {
        private ManualResetEvent _wh;
        private int _i = 0;
        public TasksWaitHandle()
        {
            _wh = new ManualResetEvent(false);
            SafeWaitHandle = _wh.SafeWaitHandle;
        }

        public void Add(Task t)
        {
            t.ContinueWith(s =>
            {
                if (Interlocked.Decrement(ref _i) == 0)
                {
                    _wh.Set();
                }
            });

            _wh.Reset();
            Interlocked.Increment(ref _i);
        }
        public bool IsCompleted
        {
            get
            {
                return _i == 0;
            }
        }
        public override void Close()
        {
            _wh.Close();
            base.Close();
        }
    }

    public class TasksWaitHandleAsyncResult<T> : TasksWaitHandle, IAsyncResult
    {
        //public new bool IsCompleted => base.IsCompleted;
        public TasksWaitHandleAsyncResult() { }
        public TasksWaitHandleAsyncResult(T state)
        {
            AsyncState = state;
        }
        public WaitHandle AsyncWaitHandle => this;
        public T AsyncState { get; set; }
        object IAsyncResult.AsyncState { get { return (T)AsyncState; } }

        public bool CompletedSynchronously { get; set; }
    }
}
