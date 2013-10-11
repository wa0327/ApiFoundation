﻿using System;
using System.Net.Http;
using System.Threading;
using ApiFoundation.Net.Http;
using ApiFoundation.Security.Cryptography;

namespace ApiFoundation.Web.Http
{
    public class ServerCryptoHandler : MessageProcessingHandler
    {
        private readonly IHttpMessageCryptoService messageCryptoService;

        public ServerCryptoHandler(ICryptoService cryptoService, ITimestampProvider<long> timestampProvider)
        {
            if (cryptoService == null)
            {
                throw new ArgumentNullException("cryptoService");
            }

            if (timestampProvider == null)
            {
                throw new ArgumentNullException("timestampProvider");
            }

            this.messageCryptoService = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider);
        }

        public ServerCryptoHandler(string secretKeyPassword, string initialVectorPassword, string hashKeyString, ITimestampProvider<long> timestampProvider)
        {
            if (timestampProvider == null)
            {
                throw new ArgumentNullException("timestampProvider");
            }

            var cryptoService = new DefaultCryptoService(secretKeyPassword, initialVectorPassword, hashKeyString);

            this.messageCryptoService = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider);
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