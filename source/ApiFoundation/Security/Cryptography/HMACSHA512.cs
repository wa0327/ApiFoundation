using System;
using System.Security.Cryptography;
using System.Text;

namespace ApiFoundation.Security.Cryptography
{
    public sealed class HMACSHA512 : IHashAlgorithm
    {
        private readonly HashAlgorithm hashAlgorithm;

        /// <summary>
        /// Initializes a new instance of the <see cref="HMACSHA512"/> class.
        /// 直接給予金鑰。
        /// </summary>
        /// <param name="key">The key.</param>
        /// <exception cref="System.ArgumentNullException">key</exception>
        public HMACSHA512(byte[] key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            this.hashAlgorithm = new System.Security.Cryptography.HMACSHA512
            {
                Key = key,
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HMACSHA512"/> class.
        /// 給予金鑰字串。
        /// </summary>
        /// <param name="keyString">The key string.</param>
        public HMACSHA512(string keyString)
        {
            if (keyString == null)
            {
                throw new ArgumentNullException("keyString");
            }

            this.hashAlgorithm = new System.Security.Cryptography.HMACSHA512
            {
                Key = Encoding.UTF8.GetBytes(keyString),
            };
        }

        public byte[] ComputeHash(byte[] source)
        {
            return this.hashAlgorithm.ComputeHash(source);
        }

        public void Dispose()
        {
            this.hashAlgorithm.Dispose();
        }
    }
}