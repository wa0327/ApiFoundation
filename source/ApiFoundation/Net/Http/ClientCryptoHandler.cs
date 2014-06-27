using System;
using System.Net.Http;
using System.Threading;

namespace ApiFoundation.Net.Http
{
    public class ClientCryptoHandler : MessageProcessingHandler
    {
        private readonly IHttpMessageCryptoService messageCryptoService;

        public ClientCryptoHandler(IHttpMessageCryptoService messageCryptoService)
        {
            if (messageCryptoService == null)
            {
                throw new ArgumentNullException("messageCryptoService");
            }

            this.messageCryptoService = messageCryptoService;
        }

        protected override HttpRequestMessage ProcessRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return this.messageCryptoService.Encrypt(request);
        }

        protected override HttpResponseMessage ProcessResponse(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            return this.messageCryptoService.Decrypt(response);
        }
    }
}