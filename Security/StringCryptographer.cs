using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CoreFramework.Security
{
    public class StringCryptographer : StreamCryptographer
    {
        public StringCryptographer(string iv, string key) : base(iv, key)
        {
        }
        public bool Decrypt(string enc, Func<StreamReader, bool> readData)
        {
            if (string.IsNullOrEmpty(enc))
            {
                return false;
            }

            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(enc)))
                return Decrypt(ms, readData);
        }

        public string Encrypt(Action<StreamWriter> writeData)
        {
            using (MemoryStream output = new MemoryStream())
            {
                Encrypt(writeData, output);
                return Convert.ToBase64String(output.ToArray());
            }
        }
    }
}
