using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;

namespace CoreFramework.CFIO
{
    public class DownloadedFile
    {
        public string Filename { get; set; }
        public string ContentType { get; set; }
        public Stream Stream { get; set; }
        public static DownloadedFile Create(string url, bool copyStream = false)
        {
            var df = new DownloadedFile();
            using (WebClient web = new WebClient())
            {
                var stream = web.OpenRead(url);
                try
                {
                    string hcd = web.ResponseHeaders["content-disposition"];
                    df.ContentType = web.ResponseHeaders["content-type"];
                    if (!string.IsNullOrEmpty(hcd))
                    {
                        ContentDisposition cd = new ContentDisposition(hcd);
                        df.Filename = cd.FileName;
                    }

                    if (copyStream)
                    {
                        df.Stream = new MemoryStream();
                        stream.CopyTo(df.Stream);
                    }
                    else
                    {
                        df.Stream = stream;
                    }
                }
                finally
                {
                    if (copyStream)
                    {
                        stream.Dispose();
                    }
                }
            }

            return df;
        }

        
    }
}
