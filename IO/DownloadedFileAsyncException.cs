using System;
using System.Runtime.Serialization;

namespace CoreFramework.CFIO
{
    [Serializable]
    internal class DownloadedFileAsyncException : Exception
    {
        public DownloadedFileAsyncException()
        {
        }

        public DownloadedFileAsyncException(string message) : base(message)
        {
        }

        public DownloadedFileAsyncException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DownloadedFileAsyncException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}