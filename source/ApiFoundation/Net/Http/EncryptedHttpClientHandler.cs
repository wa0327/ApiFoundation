using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ApiFoundation.Security.Cryptography;
using ApiFoundation.Services;

namespace ApiFoundation.Net.Http
{
    public class EncryptedHttpClientHandler : DelegatingHandler
    {
        private readonly IHttpMessageCryptoService messageCryptoService;

        public EncryptedHttpClientHandler(ICryptoService cryptoService, ITimestampProvider timestampProvider)
        {
            var baseAddress = new Uri("http://apifoundation.self.monday:9999");
            var timestampUri = new Uri(baseAddress, TimestampServiceHandler.GetUri);

            this.messageCryptoService = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider);
        }

        public EncryptedHttpClientHandler(string secretKeyPassword, string initialVectorPassword, string hashKeyString, ITimestampProvider timestampProvider)
        {
            var baseAddress = new Uri("http://apifoundation.self.monday:9999");
            var timestampUri = new Uri(baseAddress, TimestampServiceHandler.GetUri);
            var cryptoService = new DefaultCryptoService(secretKeyPassword, initialVectorPassword, hashKeyString);

            this.messageCryptoService = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request = this.Encrypt(request);

            return base.SendAsync(request, cancellationToken).ContinueWith(t => this.Decrypt(t.Result));
        }

        private HttpRequestMessage Encrypt(HttpRequestMessage request)
        {
            var e = new HttpRequestEventArgs(request);

            return this.messageCryptoService.Encrypt(e.RequestMessage);
        }

        private HttpResponseMessage Decrypt(HttpResponseMessage response)
        {
            var e = new HttpResponseEventArgs(response);

            try
            {
                return this.messageCryptoService.Decrypt(e.ResponseMessage);
            }
            catch (Exception ex)
            {
                throw new BadMessageException(ex);
            }
        }
    }
}