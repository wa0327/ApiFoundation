using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace ApiFoundation.Security.Cryptography
{
    public class DefaultEncryptor : IEncryptor
    {
        private readonly byte[] secretKey;
        private readonly byte[] initialVector;
        private readonly byte[] hashKey;

        public DefaultEncryptor(byte[] secretKey, byte[] initialVector, byte[] hashKey)
        {
            if (secretKey == null)
            {
                throw new ArgumentNullException("secretKey");
            }

            if (initialVector == null)
            {
                throw new ArgumentNullException("initialVector");
            }

            if (hashKey == null)
            {
                throw new ArgumentNullException("hashKey");
            }

            this.secretKey = secretKey;
            this.initialVector = initialVector;
            this.hashKey = hashKey;
        }

        void IEncryptor.Encrypt(byte[] plain, string timestamp, out byte[] cipher, out string signature)
        {
            if (plain == null)
            {
                throw new ArgumentNullException("plain");
            }

            if (timestamp == null)
            {
                throw new ArgumentNullException("timestamp");
            }

            cipher = this.Transform(plain, o => o.CreateEncryptor(this.secretKey, this.initialVector));
            signature = this.ComputeSignature(cipher, timestamp);
        }

        void IEncryptor.Decrypt(byte[] cipher, string timestamp, string signature, out byte[] plain)
        {
            if (cipher == null)
            {
                throw new ArgumentNullException("cipher");
            }

            if (timestamp == null)
            {
                throw new ArgumentNullException("timestamp");
            }

            if (signature == null)
            {
                throw new ArgumentNullException("signature");
            }

            var signature2 = this.ComputeSignature(cipher, timestamp);
            if (signature != signature2)
            {
                throw new InvalidSignatureException();
            }

            plain = this.Transform(cipher, o => o.CreateDecryptor(this.secretKey, this.initialVector));
        }

        private byte[] Transform(byte[] input, Func<SymmetricAlgorithm, ICryptoTransform> transformerCreator)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (transformerCreator == null)
            {
                throw new ArgumentNullException("transformerCreator");
            }

            using (var symmetricAlgorithm = new AesCryptoServiceProvider())
            {
                var transformer = transformerCreator(symmetricAlgorithm);
                using (var output = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(output, transformer, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(input, 0, input.Length);
                        cryptoStream.FlushFinalBlock();
                    }

                    return output.ToArray();
                }
            }
        }

        private string ComputeSignature(byte[] cipher, string timestamp)
        {
            var hashBase = Convert.ToBase64String(cipher) + timestamp;
            var hashBaseBytes = Encoding.UTF8.GetBytes(hashBase);

            using (var hashAlgorithm = new HMACSHA512(this.hashKey))
            {
                var hash = hashAlgorithm.ComputeHash(hashBaseBytes);
                hashAlgorithm.Clear();

                return string.Join(string.Empty, hash.Select(o => o.ToString("x2")));
            }
        }
    }
}