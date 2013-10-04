using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http;

namespace ApiFoundation.Services
{
    public sealed class SecuredApiServer : ApiServer, ISecuredApiServer
    {
        public SecuredApiServer(HttpConfiguration configuration)
            : base(configuration)
        {
        }

        public event EventHandler<HttpContentEventArgs> RequestDecrypting;

        public event EventHandler<HttpContentEventArgs> RequestDecrypted;

        public event EventHandler<HttpContentEventArgs> ResponseEncrypting;

        public event EventHandler<HttpContentEventArgs> ResponseEncrypted;
    }
}