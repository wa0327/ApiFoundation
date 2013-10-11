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
                Trace.TraceWarning("Content of request is NULL, replace to ALTERNATIVES!");
                plainContent = DefaultHttpMessageCryptoService.EmptyContentAlternatives;
            }

            var plain = plainContent.ReadAsByteArrayAsync().Result;
            var timestamp = this.timestampProvider.GetTimestamp(); // acquire a timestamp for each request.

            byte[] cipher;
            string signature;
            this.cryptoService.Encrypt(plain, timestamp.ToString(), out cipher, out signature);

            var cipherModel = this.CreateCipherModel(timestamp, cipher, signature);
            var cipherContent = new ObjectContent(cipherModel.GetType(), cipherModel, DefaultHttpMessageCryptoService.DefaultFormatter);
            Trace.TraceInformation("Encrypted request content: {0}", cipherContent.ReadAsStringAsync().Result);

            plainRequest.Content = cipherContent;
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
                if (cipherContent == null)
                {
                    throw new HttpContentNullException();
                }

                var cipherModel = cipherContent.ReadAsAsync<JObject>().Result;

                long timestamp;
                byte[] cipher;
                string signature;
                this.ParseCipherModel(cipherModel, out timestamp, out cipher, out signature);
                this.timestampProvider.Validate(timestamp);
                this.cryptoService.Decrypt(cipher, timestamp.ToString(), signature, out plain);

                this.lastTimestamp = timestamp; // keep timestamp for decrypting response.
            }
            catch (Exception ex)
            {
                throw new InvalidHttpMessageException(ex);
            }

            if (DefaultHttpMessageCryptoService.IsEmptyContent(plain))
            {
                Trace.TraceWarning("Content of request is ALTERNATIVES, replace to NULL!");
                cipherRequest.Content = null;
            }
            else
            {
                var decryptedContent = new ByteArrayContent(plain);
                Trace.TraceInformation("Decrypted request content: {0}", decryptedContent.ReadAsStringAsync().Result);

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
                Trace.TraceWarning("Content of response is NULL, replace to ALTERNATIVES!");
                plainContent = DefaultHttpMessageCryptoService.EmptyContentAlternatives;
                plainResponse.StatusCode = HttpStatusCode.OK;
            }

            var plain = plainContent.ReadAsByteArrayAsync().Result;
            var timestamp = this.lastTimestamp;
            byte[] cipher;
            string signature;
            this.cryptoService.Encrypt(plain, timestamp.ToString(), out cipher, out signature);

            var cipherModel = this.CreateCipherModel(timestamp, cipher, signature);
            var cipherContent = new ObjectContent(cipherModel.GetType(), cipherModel, DefaultHttpMessageCryptoService.DefaultFormatter);
            Trace.TraceInformation("Encrypted response content: {0}", cipherContent.ReadAsStringAsync().Result);

            plainResponse.Content = cipherContent;
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
                if (cipherContent == null)
                {
                    throw new HttpContentNullException();
                }

                var cipherModel = cipherContent.ReadAsAsync<JObject>().Result;

                long timestamp;
                byte[] cipher;
                string signature;
                this.ParseCipherModel(cipherModel, out timestamp, out cipher, out signature);
                this.cryptoService.Decrypt(cipher, timestamp.ToString(), signature, out plain);

                if (timestamp != this.lastTimestamp)
                {
                    throw new InvalidTimestampException(timestamp.ToString());
                }
            }
            catch (Exception ex)
            {
                throw new InvalidHttpMessageException(ex);
            }

            if (DefaultHttpMessageCryptoService.IsEmptyContent(plain))
            {
                Trace.TraceWarning("Content of response is ALTERNATIVES, replace to NULL!");
                cipherResponse.Content = null;
                cipherResponse.StatusCode = HttpStatusCode.NoContent;
            }
            else
            {
                var decryptedContent = new ByteArrayContent(plain);
                Trace.TraceInformation("Decrypted response content: {0}", decryptedContent.ReadAsStringAsync().Result);

                cipherResponse.Content = decryptedContent;
                cipherResponse.Content.Headers.ContentType = cipherContent.Headers.ContentType;
            }

            return cipherResponse;
        }

        private JObject CreateCipherModel(long timestamp, byte[] cipher, string signature)
        {
            if (cipher == null)
            {
                throw new ArgumentNullException("cipher");
            }

            if (signature == null)
            {
                throw new ArgumentNullException("signature");
            }

            var model = new JObject();
            model["Timestamp"] = timestamp;
            model["CipherText"] = Convert.ToBase64String(cipher);
            model["Signature"] = signature;

            return model;
        }

        private void ParseCipherModel(JObject model, out long timestamp, out byte[] cipher, out string signature)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            JValue timestampToken = model["Timestamp"] as JValue;
            if (timestampToken == null || timestampToken.Value == null)
            {
                throw new TimestampNullException();
            }

            var cipherTextToken = model["CipherText"] as JValue;
            if (cipherTextToken == null || cipherTextToken.Value == null)
            {
                throw new CipherTextNullException();
            }

            var signatureToken = model["Signature"] as JValue;
            if (signatureToken == null || signatureToken.Value == null)
            {
                throw new SignatureNullException();
            }

            try
            {
                timestamp = timestampToken.Value<long>();
            }
            catch (Exception ex)
            {
                throw new InvalidTimestampException(timestampToken.Value<string>(), ex);
            }

            var cipherText = cipherTextToken.Value<string>();
            if (cipherText.Length == 0)
            {
                throw new InvalidCipherTextException(cipherText);
            }

            try
            {
                cipher = Convert.FromBase64String(cipherText);
            }
            catch (Exception ex)
            {
                throw new InvalidCipherTextException(cipherText, ex);
            }

            signature = signatureToken.Value<string>();
            if (signature.Length != 128)
            {
                throw new InvalidSignatureException(signature);
            }
        }
    }
}