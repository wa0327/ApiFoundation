using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace ApiFoundation.Security.Cryptography
{
    public class FakeHttpContentEncryptor : IMessageEncryptor
    {
        HttpContent IMessageEncryptor.Encrypt(HttpContent originContent)
        {
            if (originContent == null)
            {
                throw new ArgumentNullException("originContent");
            }

            var originStream = originContent.ReadAsStreamAsync().Result;

            if (originStream.Length == 0)
            {
                return originContent;
            }

            byte[] originBytes = new byte[originStream.Length];
            originStream.Read(originBytes, 0, originBytes.Length);
            var encodedString = Convert.ToBase64String(originBytes);

            var encodedContent = new StringContent(encodedString);
            encodedContent.Headers.ContentType = originContent.Headers.ContentType;

            return encodedContent;
        }

        HttpContent IMessageEncryptor.Decrypt(HttpContent encryptedContent)
        {
            if (encryptedContent == null)
            {
                throw new ArgumentNullException("encryptedContent");
            }

            var encodedString = encryptedContent.ReadAsStringAsync().Result;

            if (string.IsNullOrEmpty(encodedString))
            {
                return encryptedContent;
            }

            var originBytes = Convert.FromBase64String(encodedString);
            var originStream = new MemoryStream(originBytes);

            var originContent = new StreamContent(originStream);
            originContent.Headers.ContentType = encryptedContent.Headers.ContentType;

            return originContent;
        }

        void IDisposable.Dispose()
        {
        }
    }
}