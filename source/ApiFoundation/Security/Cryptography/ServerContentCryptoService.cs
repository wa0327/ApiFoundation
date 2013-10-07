using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using Newtonsoft.Json.Linq;

namespace ApiFoundation.Security.Cryptography
{
    internal sealed class ServerContentCryptoService : IHttpMessageCryptoService
    {
        private readonly ICryptoService cryptoService;
        private readonly ITimestampProvider timestampProvider;

        internal ServerContentCryptoService(ICryptoService cryptoService, ITimestampProvider timestampProvider)
        {
            if (cryptoService == null)
            {
                throw new ArgumentNullException("cryptoService");
            }

            if (timestampProvider == null)
            {
                throw new ArgumentNullException("timestampProvider");
            }

            this.cryptoService = cryptoService;
            this.timestampProvider = timestampProvider;
        }

        public HttpContent Decrypt(HttpContent cipherContent)
        {
            if (cipherContent == null)
            {
                return cipherContent;
            }

            if (cipherContent.Headers.ContentLength == 0 || cipherContent.ReadAsByteArrayAsync().Result.Length == 0)
            {
                return cipherContent;
            }

            var content = cipherContent.ReadAsAsync<JObject>().Result;

            byte[] plain;
            try
            {
                var timestamp = (string)content["Timestamp"];
                var cipher = Convert.FromBase64String((string)content["CipherText"]);
                var signature = (string)content["Signature"];

                this.timestampProvider.Validate(timestamp);
                this.cryptoService.Decrypt(cipher, timestamp, signature, out plain);
            }
            catch (Exception ex)
            {
                throw new InvalidHttpContentException(ex);
            }

            var originContent = new ByteArrayContent(plain);
            originContent.Headers.ContentType = cipherContent.Headers.ContentType;

            return originContent;
        }

        public HttpContent Encrypt(HttpContent plainContent)
        {
            if (plainContent == null)
            {
                return plainContent;
            }

            var plain = plainContent.ReadAsByteArrayAsync().Result;
            string timestamp;
            DateTime expires;
            this.timestampProvider.GetTimestamp(out timestamp, out expires);
            byte[] cipher;
            string signature;
            this.cryptoService.Encrypt(plain, timestamp, out cipher, out signature);

            var message = new
            {
                Timestamp = timestamp,
                CipherText = Convert.ToBase64String(cipher),
                Signature = signature,
                Expires = expires,
            };

            var encryptedContent = new ObjectContent(message.GetType(), message, new JsonMediaTypeFormatter());
            encryptedContent.Headers.ContentType = plainContent.Headers.ContentType;

            return encryptedContent;
        }
    }
}