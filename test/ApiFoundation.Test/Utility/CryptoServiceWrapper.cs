using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ApiFoundation.Security.Cryptography;

namespace ApiFoundation.Utility
{
    internal sealed class CryptoServiceWrapper : IDisposable
    {
        private readonly DefaultCryptoService service;

        internal CryptoServiceWrapper()
        {
            Func<byte[]> secretKeyCreator = () =>
            {
                var password = "123456789012345678901234";

                byte[] salt = new byte[32];
                using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, 32))
                {
                    return deriveBytes.GetBytes(32);
                }
            };

            Func<byte[]> initialVectorCreator = () =>
            {
                var password = "12345678";

                byte[] salt = new byte[16];
                using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, 16))
                {
                    return deriveBytes.GetBytes(16);
                }
            };

            var symmetricAlgorithm = new AesCryptoServiceProvider
            {
                Key = secretKeyCreator(),
                IV = initialVectorCreator(),
            };

            var hashAlgorithm = new HMACSHA512
            {
                Key = Encoding.UTF8.GetBytes("1234567890"),
            };

            this.service = new DefaultCryptoService(symmetricAlgorithm, hashAlgorithm);
        }

        public void Dispose()
        {
            this.service.Dispose();
        }

        public static implicit operator DefaultCryptoService(CryptoServiceWrapper source)
        {
            return source.service;
        }
    }
}