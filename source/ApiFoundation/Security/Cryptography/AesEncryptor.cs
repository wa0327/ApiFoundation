using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http.Tracing;

namespace ApiFoundation.Security.Cryptography
{
    public class AesEncryptor : IEncryptor
    {
        private readonly IKeyProvider keyProvider;
        private readonly SymmetricAlgorithm algorithm;

        public AesEncryptor(IKeyProvider keyProvider)
        {
            this.keyProvider = keyProvider;
            this.algorithm = new AesManaged();
        }

        HttpContent IEncryptor.Encrypt(HttpContent originContent)
        {
            if (originContent == null)
            {
                throw new ArgumentNullException("originContent");
            }

            var originData = originContent.ReadAsStreamAsync().Result;

            if (originData.Length == 0)
            {
                return originContent;
            }

            this.algorithm.Key = this.keyProvider.Key;
            this.algorithm.IV = this.keyProvider.IV;

            using (var encryptor = algorithm.CreateEncryptor())
            {
                using (var encryptedData = new MemoryStream())
                {
                    var cryptoStream = new CryptoStream(encryptedData, encryptor, CryptoStreamMode.Write);
                    originData.CopyTo(cryptoStream);
                    cryptoStream.FlushFinalBlock();
                    encryptedData.Position = 0;

                    var encodedString = Convert.ToBase64String(encryptedData.ToArray());
                    var encryptedContent = new StringContent(encodedString);
                    encryptedContent.Headers.ContentType = originContent.Headers.ContentType;

                    return encryptedContent;
                }
            }
        }

        HttpContent IEncryptor.Decrypt(HttpContent encryptedContent)
        {
            if (encryptedContent == null)
            {
                throw new ArgumentNullException("encryptedContent");
            }

            var encodedString = encryptedContent.ReadAsStringAsync().Result;

            using (var encryptedData = new MemoryStream(Convert.FromBase64String(encodedString)))
            {
                if (encryptedData.Length == 0)
                {
                    return encryptedContent;
                }

                this.algorithm.Key = this.keyProvider.Key;
                this.algorithm.IV = this.keyProvider.IV;

                using (var decryptor = algorithm.CreateDecryptor())
                {
                    var cryptoStream = new CryptoStream(encryptedData, decryptor, CryptoStreamMode.Read);
                    var originData = new MemoryStream((int)encryptedData.Length);
                    cryptoStream.CopyTo(originData);
                    originData.Flush();
                    originData.Position = 0;

                    var originContent = new StreamContent(originData);
                    originContent.Headers.ContentType = encryptedContent.Headers.ContentType;

                    return originContent;
                }
            }
        }

        void IDisposable.Dispose()
        {
            algorithm.Dispose();
        }
    }
}