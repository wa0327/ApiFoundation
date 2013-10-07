using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using Newtonsoft.Json.Linq;

namespace ApiFoundation.Security.Cryptography
{
    internal class DefaultHttpMessageCryptoService : IHttpMessageCryptoService
    {
        private readonly ICryptoService cryptoService;
        private readonly ITimestampProvider timestampProvider;

        private string timestamp;
        private DateTime timestampExpires;

        internal DefaultHttpMessageCryptoService(ICryptoService cryptoService, ITimestampProvider timestampProvider)
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

        public HttpRequestMessage Encrypt(HttpRequestMessage plainRequest)
        {
            if (plainRequest == null)
            {
                throw new ArgumentNullException("plainMessage");
            }

            var plainContent = plainRequest.Content;
            if (plainContent == null)
            {
                return plainRequest;
            }

            var plain = plainContent.ReadAsByteArrayAsync().Result;
            var timestamp = this.GetTimestamp();
            byte[] cipher;
            string signature;
            this.cryptoService.Encrypt(plain, timestamp, out cipher, out signature);

            var encrypted = new
            {
                Timestamp = timestamp,
                CipherText = Convert.ToBase64String(cipher),
                Signature = signature,
            };

            plainRequest.Content = new ObjectContent(encrypted.GetType(), encrypted, new JsonMediaTypeFormatter());
            plainRequest.Content.Headers.ContentType = plainContent.Headers.ContentType; // keep source content type

            return plainRequest;
        }

        public HttpRequestMessage Decrypt(HttpRequestMessage cipherRequest)
        {
            if (cipherRequest == null)
            {
                throw new ArgumentNullException("cipherRequest");
            }

            var cipherContent = cipherRequest.Content;
            if (cipherContent == null)
            {
                return cipherRequest;
            }

            if (cipherContent.Headers.ContentLength == 0 || cipherContent.ReadAsByteArrayAsync().Result.Length == 0)
            {
                return cipherRequest;
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

            cipherRequest.Content = new ByteArrayContent(plain);
            cipherRequest.Content.Headers.ContentType = cipherContent.Headers.ContentType;

            return cipherRequest;
        }

        public HttpResponseMessage Encrypt(HttpResponseMessage plainResponse)
        {
            if (plainResponse == null)
            {
                throw new ArgumentNullException("plainResponse");
            }

            var plainContent = plainResponse.Content;
            if (plainContent == null)
            {
                return plainResponse;
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

            plainResponse.Content = new ObjectContent(message.GetType(), message, new JsonMediaTypeFormatter());
            plainResponse.Content.Headers.ContentType = plainContent.Headers.ContentType;

            return plainResponse;
        }

        public HttpResponseMessage Decrypt(HttpResponseMessage cipherResponse)
        {
            if (cipherResponse == null)
            {
                throw new ArgumentNullException("cipherResponse");
            }

            var cipherContent = cipherResponse.Content;
            if (cipherContent == null)
            {
                return cipherResponse;
            }

            if (cipherContent.Headers.ContentLength == 0 || cipherContent.ReadAsByteArrayAsync().Result.Length == 0)
            {
                return cipherResponse;
            }

            var message = cipherContent.ReadAsAsync<JObject>().Result;

            byte[] plain;
            try
            {
                var timestamp = (string)message["Timestamp"];
                var cipher = Convert.FromBase64String((string)message["CipherText"]);
                var signature = (string)message["Signature"];
                var expires = (DateTime)message["Expires"];

                this.cryptoService.Decrypt(cipher, timestamp, signature, out plain);

                this.timestamp = timestamp;
                this.timestampExpires = expires;
            }
            catch (Exception ex)
            {
                throw new InvalidHttpContentException(ex);
            }

            cipherResponse.Content = new ByteArrayContent(plain);
            cipherResponse.Content.Headers.ContentType = cipherContent.Headers.ContentType;

            return cipherResponse;
        }

        private string GetTimestamp()
        {
            if (this.timestamp == null || DateTime.UtcNow >= this.timestampExpires)
            {
                this.timestampProvider.GetTimestamp(out this.timestamp, out this.timestampExpires);
            }

            return this.timestamp;
        }
    }
}