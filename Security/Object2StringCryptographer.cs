using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

namespace CoreFramework.CFSecurity
{
    public class Object2StringCryptographer : Object2StreamCryptographer
    {
        public Object2StringCryptographer(string iv, string key) : base(iv, key)
        {
        }
        public object Decrypt(string enc)
        {
            if (string.IsNullOrEmpty(enc))
            {
                return false;
            }

            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(enc)))
                return Decrypt(ms);
        }

        public string Encrypt(object obj)
        {
            using (MemoryStream output = new MemoryStream())
            {
                Encrypt(obj, output);
                return Convert.ToBase64String(output.ToArray());
            }
        }
    }
}
