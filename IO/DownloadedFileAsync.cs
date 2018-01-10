using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoreFramework.CFIO
{
    public class DownloadedFileAsync<T>
    {
        public string Filename { get; set; }
        public string ContentType { get; set; }
        public Task<T> Task { get; set; }
        public static DownloadedFileAsync<T> Create(string url, Func<DownloadedFileAsync<T>, Stream, T> convertResult, CancellationToken cancellationToken)
        {
            return Create(new Uri(url), convertResult, cancellationToken);
        }
        public static DownloadedFileAsync<T> Create(Uri uri, Func<DownloadedFileAsync<T>,Stream,T> convertResult, CancellationToken cancellationToken)
        {
            var df = new DownloadedFileAsync<T>();
            using (WebClient web = new WebClient())
            {
                var tcs = new TaskCompletionSource<T>();

                cancellationToken.Register(() => tcs.SetCanceled());

                web.OpenReadCompleted += (sender, args) =>
                {
                    if (args.Error != null)
                    {
                        tcs.SetException(new DownloadedFileAsyncException("Error downloading content from \"{0}\"".Format2(uri), args.Error));
                        return;
                    }

                    if (args.Cancelled)
                    {
                        tcs.SetCanceled();
                        return;
                    }

                    try
                    {
                        string hcd = web.ResponseHeaders["content-disposition"];
                        df.ContentType = web.ResponseHeaders["content-type"];
                        if (!string.IsNullOrEmpty(hcd))
                        {
                            ContentDisposition cd = new ContentDisposition(hcd);
                            df.Filename = cd.FileName;
                        }

                        tcs.SetResult(convertResult(df, args.Result));
                    }
                    catch (Exception ex)
                    {
                        tcs.SetException(new DownloadedFileAsyncException("Error downloading content from \"{0}\"".Format2(uri), ex));
                    }

                };

                web.OpenReadAsync(uri);

                df.Task = tcs.Task;
            }

            return df;
        }
        public static DownloadedFileAsync<Stream> Create(string url, CancellationToken cancellationToken)
        {
            return Create(new Uri(url), cancellationToken);
        }
        public static Task<DownloadedFileAsync<Stream>> CreateAsync(string url, CancellationToken cancellationToken)
        {
            var df = Create(new Uri(url), cancellationToken);
            return df.Task.ContinueWith(_ => df);
        }
        public static Task<DownloadedFileAsync<Stream>> CreateAsync(Uri uri, CancellationToken cancellationToken)
        {
            var df = Create(uri, cancellationToken);
            return df.Task.ContinueWith(_ => df);
        }
        public static DownloadedFileAsync<Stream> Create(Uri uri, CancellationToken cancellationToken)
        {
            return DownloadedFileAsync<Stream>.Create(uri, (df, stream) => stream, cancellationToken);
        }
    }
}
