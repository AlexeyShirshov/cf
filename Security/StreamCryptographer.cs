using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CoreFramework.Security
{
    public class StreamCryptographer
    {

        public StreamCryptographer(string iv, string key)
        {
            IV = iv;
            Key = key;
        }
        public string IV { get; }
        public string Key { get; }

        public bool Decrypt(Stream input, Func<StreamReader, bool> readData)
        {
            ICryptoTransform decryptor = null;
            using (AesManaged algo = new AesManaged())
            {
                SetKeys(algo);
                decryptor = algo.CreateDecryptor();
            }

            try
            {
                using (CryptoStream cryptostreamDecr = new CryptoStream(input, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader sr = new StreamReader(cryptostreamDecr, Encoding.UTF8))
                    {
                        return readData(sr);
                    }
                }
            }
            finally
            {
                if (decryptor != null)
                {
                    decryptor.Dispose();
                }
            }
        }

        private void SetKeys(SymmetricAlgorithm algo)
        {
            using (Rfc2898DeriveBytes magicBytes = new Rfc2898DeriveBytes(IV, new byte[] { 35, 50, 164, 225, 168, 8, 201, 153 }))
            {
                algo.IV = magicBytes.GetBytes(algo.BlockSize / 8);
            }

            using (Rfc2898DeriveBytes magicBytes = new Rfc2898DeriveBytes(Key, new byte[] { 35, 50, 164, 225, 168, 8, 201, 153 }))
            {
                algo.Key = magicBytes.GetBytes(algo.KeySize / 8);
            }
        }

        public void Encrypt(Action<StreamWriter> writeData, Stream output)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (StreamWriter sw = new StreamWriter(ms, Encoding.UTF8))
                {
                    writeData(sw);
                    sw.Flush();

                    ICryptoTransform encryptor = null;
                    using (AesManaged algo = new AesManaged())
                    {
                        SetKeys(algo);
                        encryptor = algo.CreateEncryptor();
                    }

                    try
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(output, encryptor, CryptoStreamMode.Write))
                        {
                            ms.Seek(0, SeekOrigin.Begin);
                            ms.CopyTo(cryptoStream);
                            if (!cryptoStream.HasFlushedFinalBlock)
                            {
                                cryptoStream.FlushFinalBlock();
                            }
                        }
                    }
                    finally
                    {
                        if (encryptor != null)
                        {
                            encryptor.Dispose();
                        }
                    }
                }
            }
        }
    }
}
