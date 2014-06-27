using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using ApiFoundation.Security.Cryptography;
using Newtonsoft.Json.Linq;

namespace ApiFoundation.Net.Http
{
    /// <summary>
    /// 預設的 HTTP message 加解密服務實作。
    /// </summary>
    public class DefaultHttpMessageCryptoService : IHttpMessageCryptoService
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

        private readonly ISymmetricAlgorithm symmetricAlgorithm;
        private readonly IHashAlgorithm hashAlgorithm;
        private readonly ITimestampProvider<string> timestampProvider;

        [ThreadStatic]
        private string lastTimestamp;

        public DefaultHttpMessageCryptoService(ISymmetricAlgorithm symmetricAlgorithm, IHashAlgorithm hashAlgorithm, ITimestampProvider<string> timestampProvider)
        {
            if (symmetricAlgorithm == null)
            {
                throw new ArgumentNullException("symmetricAlgorithm");
            }

            if (hashAlgorithm == null)
            {
                throw new ArgumentNullException("hashAlgorithm");
            }

            if (timestampProvider == null)
            {
                throw new ArgumentNullException("timestampProvider");
            }

            this.symmetricAlgorithm = symmetricAlgorithm;
            this.hashAlgorithm = hashAlgorithm;
            this.timestampProvider = timestampProvider;
        }

        public void Dispose()
        {
            this.symmetricAlgorithm.Dispose();
            this.hashAlgorithm.Dispose();
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
            this.Encrypt(plain, timestamp, out cipher, out signature);

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

                string timestamp;
                byte[] cipher;
                string signature;
                this.ParseCipherModel(cipherModel, out timestamp, out cipher, out signature);
                this.timestampProvider.Validate(timestamp);
                this.Decrypt(cipher, timestamp, signature, out plain);

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
            this.Encrypt(plain, timestamp, out cipher, out signature);

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

                string timestamp;
                byte[] cipher;
                string signature;
                this.ParseCipherModel(cipherModel, out timestamp, out cipher, out signature);
                this.Decrypt(cipher, timestamp, signature, out plain);

                if (timestamp != this.lastTimestamp)
                {
                    throw new InvalidTimestampException(timestamp);
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

        private JObject CreateCipherModel(string timestamp, byte[] cipher, string signature)
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

        private void ParseCipherModel(JObject model, out string timestamp, out byte[] cipher, out string signature)
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
                timestamp = timestampToken.Value<string>();
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

        private void Encrypt(byte[] plain, string timestamp, out byte[] cipher, out string signature)
        {
            if (plain == null)
            {
                throw new ArgumentNullException("plain");
            }

            if (timestamp == null)
            {
                throw new ArgumentNullException("timestamp");
            }

            cipher = this.symmetricAlgorithm.Encrypt(plain);
            signature = this.ComputeSignature(cipher, timestamp);
        }

        private void Decrypt(byte[] cipher, string timestamp, string signature, out byte[] plain)
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
                throw new InvalidSignatureException(signature);
            }

            plain = this.symmetricAlgorithm.Decrypt(cipher);
        }

        private string ComputeSignature(byte[] cipher, string timestamp)
        {
            if (cipher == null)
            {
                throw new ArgumentNullException("cipher");
            }

            if (timestamp == null)
            {
                throw new ArgumentNullException("timestamp");
            }

            var hashBase = Convert.ToBase64String(cipher) + timestamp;
            var hashBaseBytes = Encoding.UTF8.GetBytes(hashBase);
            var hash = this.hashAlgorithm.ComputeHash(hashBaseBytes);

            return string.Join(string.Empty, hash.Select(o => o.ToString("x2")));
        }
    }
}