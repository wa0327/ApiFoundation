using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ApiFoundation.Security.Cryptography;

namespace ApiFoundation.Net.Http
{
    public class ClientCryptoHandler : MessageProcessingHandler
    {
        private readonly IHttpMessageCryptoService messageCryptoService;

        public ClientCryptoHandler(ICryptoService cryptoService, ITimestampProvider<long> timestampProvider)
        {
            this.messageCryptoService = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider);
        }

        public ClientCryptoHandler(string secretKeyPassword, string initialVectorPassword, string hashKeyString, ITimestampProvider<long> timestampProvider)
        {
            var cryptoService = new DefaultCryptoService(secretKeyPassword, initialVectorPassword, hashKeyString);

            this.messageCryptoService = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider);
        }

        protected override HttpRequestMessage ProcessRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return this.messageCryptoService.Encrypt(request);
        }

        protected override HttpResponseMessage ProcessResponse(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            try
            {
                return this.messageCryptoService.Decrypt(response);
            }
            catch (Exception ex)
            {
                throw new BadMessageException(ex);
            }
        }
    }
}