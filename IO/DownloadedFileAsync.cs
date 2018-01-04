using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoreFramework
{
    public class DownloadedFileAsync<T>
    {
        public string Filename { get; set; }
        public string ContentType { get; set; }
        public Task<T> Task { get; set; }
        public static DownloadedFileAsync<T> Create(Uri url, Func<DownloadedFileAsync<T>,Stream,T> convertResult, CancellationToken cancellationToken)
        {
            var df = new DownloadedFileAsync<T>();
            using (WebClient web = new WebClient())
            {
                var tcs = new TaskCompletionSource<T>();

                cancellationToken.Register(() => tcs.SetCanceled());

                web.OpenReadCompleted += (sender, args) =>
                {
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
                        tcs.SetException(ex);
                    }

                };

                web.OpenReadAsync(url);

                df.Task = tcs.Task;
            }

            return df;
        }

        public static DownloadedFileAsync<Stream> Create(Uri url, CancellationToken cancellationToken)
        {
            return DownloadedFileAsync<Stream>.Create(url, (df, stream) => stream, cancellationToken);
        }
    }
}
