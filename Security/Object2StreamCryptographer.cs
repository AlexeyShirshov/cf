using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

namespace CoreFramework.Security
{
    public class Object2StreamCryptographer : StreamCryptographer
    {
        public Object2StreamCryptographer(string iv, string key) : base(iv, key)
        {
        }
        public object Decrypt(Stream input)
        {
            object obj = null;
            Decrypt(input, (sr)=>
            {
                try
                {
                    new BinaryFormatter().Deserialize(sr.BaseStream);
                    return true;
                }
                catch
                {
                    return false;
                }
            });

            return obj;
        }

        public void Encrypt(object obj, Stream output)
        {
            Encrypt((sw)=>new BinaryFormatter().Serialize(sw.BaseStream, obj), output);
        }
    }
}
