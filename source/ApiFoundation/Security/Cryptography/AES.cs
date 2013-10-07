using System;
using System.IO;
using System.Security.Cryptography;

namespace ApiFoundation.Security.Cryptography
{
    public sealed class AES : ISymmetricAlgorithm
    {
        private readonly SymmetricAlgorithm symmetricAlgorithm;

        /// <summary>
        /// Initializes a new instance of the <see cref="AES"/> class.
        /// 直接給予對稱金鑰及初始向量。
        /// </summary>
        /// <param name="secretKey">The secret key.</param>
        /// <param name="initialVector">The initial vector.</param>
        /// <exception cref="System.ArgumentNullException">secretKey
        /// or
        /// initialVector</exception>
        public AES(byte[] secretKey, byte[] initialVector)
        {
            if (secretKey == null)
            {
                throw new ArgumentNullException("secretKey");
            }

            if (initialVector == null)
            {
                throw new ArgumentNullException("initialVector");
            }

            this.symmetricAlgorithm = new AesCryptoServiceProvider
            {
                Key = secretKey,
                IV = initialVector,
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AES"/> class.
        /// 給予對稱金鑰密碼及初始向量密碼。
        /// </summary>
        /// <param name="secretKeyPassword">The secret key password.</param>
        /// <param name="initialVectorPassword">The initial vector password.</param>
        /// <exception cref="System.ArgumentNullException">
        /// secretKeyPassword
        /// or
        /// initialVectorPassword
        /// </exception>
        public AES(string secretKeyPassword, string initialVectorPassword)
        {
            if (secretKeyPassword == null)
            {
                throw new ArgumentNullException("secretKeyPassword");
            }

            if (initialVectorPassword == null)
            {
                throw new ArgumentNullException("initialVectorPassword");
            }

            var secretKey = this.DeriveBytes(secretKeyPassword, 32);
            var initialVector = this.DeriveBytes(initialVectorPassword, 16);

            this.symmetricAlgorithm = new AesCryptoServiceProvider
            {
                Key = secretKey,
                IV = initialVector,
            };
        }

        public byte[] Encrypt(byte[] plain)
        {
            using (var encryptor = this.symmetricAlgorithm.CreateEncryptor())
            {
                return this.Transform(plain, encryptor);
            }
        }

        public byte[] Decrypt(byte[] cipher)
        {
            using (var decryptor = this.symmetricAlgorithm.CreateDecryptor())
            {
                return this.Transform(cipher, decryptor);
            }
        }

        public void Dispose()
        {
            this.symmetricAlgorithm.Dispose();
        }

        private byte[] DeriveBytes(string password, int length)
        {
            byte[] salt = new byte[length];

            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, length))
            {
                return deriveBytes.GetBytes(length);
            }
        }

        private byte[] Transform(byte[] input, ICryptoTransform transform)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (transform == null)
            {
                throw new ArgumentNullException("transform");
            }

            using (var buffer = new MemoryStream())
            using (var cryptoStream = new CryptoStream(buffer, transform, CryptoStreamMode.Write))
            {
                cryptoStream.Write(input, 0, input.Length);
                cryptoStream.FlushFinalBlock();

                return buffer.ToArray();
            }
        }
    }
}