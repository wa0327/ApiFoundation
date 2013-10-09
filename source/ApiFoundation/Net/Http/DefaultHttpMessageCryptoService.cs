using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using ApiFoundation.Security.Cryptography;
using Newtonsoft.Json.Linq;

namespace ApiFoundation.Net.Http
{
    internal class DefaultHttpMessageCryptoService : IHttpMessageCryptoService
    {
        private static readonly MediaTypeFormatter DefaultFormatter;
        private static readonly HttpContent EmptyContentAlternatives;
        private static readonly byte[] EmptyContentAlternativesByteArray;

        static DefaultHttpMessageCryptoService()
        {
            DefaultHttpMessageCryptoService.DefaultFormatter = new JsonMediaTypeFormatter();
            DefaultHttpMessageCryptoService.EmptyContentAlternatives = new ObjectContent<string>("-=-!1健a2康b3平c4安d5快e6樂f7幸g8福h9!-=-", DefaultHttpMessageCryptoService.DefaultFormatter);
            DefaultHttpMessageCryptoService.EmptyContentAlternativesByteArray = DefaultHttpMessageCryptoService.EmptyContentAlternatives.ReadAsByteArrayAsync().Result;
        }

        private static bool IsEmptyContent(byte[] plain)
        {
            if (plain == null)
            {
                throw new ArgumentNullException("plain");
            }

            if (plain.Length != DefaultHttpMessageCryptoService.EmptyContentAlternativesByteArray.Length)
            {
                return false;
            }

            for (int i = 0; i < plain.Length; i++)
            {
                if (plain[i] != DefaultHttpMessageCryptoService.EmptyContentAlternativesByteArray[i])
                {
                    return false;
                }
            }

            return true;
        }

        private readonly ICryptoService cryptoService;
        private readonly ITimestampProvider<long> timestampProvider;

        [ThreadStatic]
        private long lastTimestamp;

        internal DefaultHttpMessageCryptoService(ICryptoService cryptoService, ITimestampProvider<long> timestampProvider)
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

        public void Dispose()
        {
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
                Trace.TraceWarning("Content of request message is NULL, replace to ALTERNATIVES!");
                plainContent = DefaultHttpMessageCryptoService.EmptyContentAlternatives;
            }

            var plain = plainContent.ReadAsByteArrayAsync().Result;
            var timestamp = this.timestampProvider.GetTimestamp(); // acquire a timestamp for each request.

            byte[] cipher;
            string signature;
            this.cryptoService.Encrypt(plain, timestamp.ToString(), out cipher, out signature);

            var message = new
            {
                Timestamp = timestamp,
                CipherText = Convert.ToBase64String(cipher),
                Signature = signature,
            };

            var encryptedContent = new ObjectContent(message.GetType(), message, DefaultHttpMessageCryptoService.DefaultFormatter);
            Trace.TraceInformation("Encrypted request content: {0}", encryptedContent.ReadAsStringAsync().Result);

            plainRequest.Content = encryptedContent;
            plainRequest.Content.Headers.ContentType = plainContent.Headers.ContentType; // keep source content type

            this.lastTimestamp = timestamp;

            return plainRequest;
        }

        public HttpRequestMessage Decrypt(HttpRequestMessage cipherRequest)
        {
            if (cipherRequest == null)
            {
                throw new ArgumentNullException("cipherRequest");
            }

            var cipherContent = cipherRequest.Content;
            byte[] plain;
            try
            {
                // !!! NO empty encrypted message !!!
                var message = cipherContent.ReadAsAsync<JObject>().Result;
                var timestamp = (long)message["Timestamp"];
                var cipher = Convert.FromBase64String((string)message["CipherText"]);
                var signature = (string)message["Signature"];

                this.timestampProvider.Validate(timestamp);
                this.cryptoService.Decrypt(cipher, timestamp.ToString(), signature, out plain);

                this.lastTimestamp = timestamp;
            }
            catch (Exception ex)
            {
                throw new InvalidHttpContentException(ex);
            }

            if (DefaultHttpMessageCryptoService.IsEmptyContent(plain))
            {
                Trace.TraceWarning("Content of request message is ALTERNATIVES, replace to NULL!");
                cipherRequest.Content = null;
            }
            else
            {
                var decryptedContent = new ByteArrayContent(plain);
                Trace.TraceInformation("Encrypted request content: {0}", decryptedContent.ReadAsStringAsync().Result);

                cipherRequest.Content = decryptedContent;
                cipherRequest.Content.Headers.ContentType = cipherContent.Headers.ContentType;
            }

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
                Trace.TraceWarning("Content of response message is NULL, replace to ALTERNATIVES!");
                plainContent = DefaultHttpMessageCryptoService.EmptyContentAlternatives;
                plainResponse.StatusCode = HttpStatusCode.OK;
            }

            var plain = plainContent.ReadAsByteArrayAsync().Result;
            var timestamp = this.lastTimestamp;
            byte[] cipher;
            string signature;
            this.cryptoService.Encrypt(plain, timestamp.ToString(), out cipher, out signature);

            var message = new
            {
                Timestamp = timestamp,
                CipherText = Convert.ToBase64String(cipher),
                Signature = signature,
            };

            var encryptedContent = new ObjectContent(message.GetType(), message, DefaultHttpMessageCryptoService.DefaultFormatter);
            Trace.TraceInformation("Encrypted response content: {0}", encryptedContent.ReadAsStringAsync().Result);

            plainResponse.Content = encryptedContent;
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
            byte[] plain;
            try
            {
                // !!! NO empty encrypted message !!!
                var message = cipherContent.ReadAsAsync<JObject>().Result;
                var timestamp = (long)message["Timestamp"];
                var cipher = Convert.FromBase64String((string)message["CipherText"]);
                var signature = (string)message["Signature"];

                this.cryptoService.Decrypt(cipher, timestamp.ToString(), signature, out plain);

                if (timestamp != this.lastTimestamp)
                {
                    throw new InvalidTimestampException();
                }

                this.lastTimestamp = timestamp;
            }
            catch (Exception ex)
            {
                throw new InvalidHttpContentException(ex);
            }

            if (DefaultHttpMessageCryptoService.IsEmptyContent(plain))
            {
                Trace.TraceWarning("Content of response message is ALTERNATIVES, replace to NULL!");
                cipherResponse.Content = null;
                cipherResponse.StatusCode = HttpStatusCode.NoContent;
            }
            else
            {
                var decryptedContent = new ByteArrayContent(plain);
                Trace.TraceInformation("Encrypted response content: {0}", decryptedContent.ReadAsStringAsync().Result);

                cipherResponse.Content = decryptedContent;
                cipherResponse.Content.Headers.ContentType = cipherContent.Headers.ContentType;
            }

            return cipherResponse;
        }
    }
}