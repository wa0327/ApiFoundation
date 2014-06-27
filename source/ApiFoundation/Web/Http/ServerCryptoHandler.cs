using System;
using System.Net.Http;
using System.Threading;
using ApiFoundation.Net.Http;
using ApiFoundation.Security.Cryptography;

namespace ApiFoundation.Web.Http
{
    public class ServerCryptoHandler : MessageProcessingHandler
    {
        private readonly IHttpMessageCryptoService messageCryptoService;

        public ServerCryptoHandler(IHttpMessageCryptoService messageCryptoService)
        {
            if (messageCryptoService == null)
            {
                throw new ArgumentNullException("messageCryptoService");
            }

            this.messageCryptoService = messageCryptoService;
        }

        public ServerCryptoHandler(string secretKeyPassword, string initialVectorPassword, string hashKeyString)
        {
            var symmetricAlgorithm = new AES(secretKeyPassword, initialVectorPassword);
            var hashAlgorithm = new HMACSHA512(hashKeyString);
            var timestampProvider = new DefaultTimestampProvider(TimeSpan.FromMinutes(15)) as ITimestampProvider<string>;

            this.messageCryptoService = new DefaultHttpMessageCryptoService(symmetricAlgorithm, hashAlgorithm, timestampProvider);
        }

        protected override HttpRequestMessage ProcessRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return this.messageCryptoService.Decrypt(request);
        }

        protected override HttpResponseMessage ProcessResponse(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            return this.messageCryptoService.Encrypt(response);
        }
    }
}