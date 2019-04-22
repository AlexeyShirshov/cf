using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreFramework
{
    public class AutoCleanupT<T> : IDisposable
    {
        private bool disposed;
        private readonly Action<T> executeOnDisposeC;
        private T _ctx;

        public AutoCleanupT(Func<T> executeOnConstruct,
            Action<T> executeOnDispose)
        {            
            if (null != executeOnConstruct)
            {
                _ctx = executeOnConstruct();
            }
            this.executeOnDisposeC = executeOnDispose;
        }
        #region IDisposable Members
        /// <summary>
        /// Disposes the <see cref="AutoCleanup"/> object,
        /// executing the delegate function provided in the
        /// constructor.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        //
        // Internal implementation of the Dispose() method.
        // See the MSDN documentation on the IDisposable
        // interface for a detailed explanation of this
        // pattern.
        //
        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                //
                // When disposing is true, release all
                // managed resources.
                //
                if (disposing)
                {
                    if (null != this.executeOnDisposeC)
                    {
                        this.executeOnDisposeC(_ctx);
                    }
                }
                disposed = true;
            }
        }
        #endregion
    }

}
