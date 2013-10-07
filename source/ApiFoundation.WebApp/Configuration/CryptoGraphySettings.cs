using System;
using System.Collections.Specialized;

namespace ApiFoundation.Configuration
{
    internal class CryptoGraphySettings
    {
        private readonly NameValueCollection source;
        private readonly string version;

        internal CryptoGraphySettings(NameValueCollection source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            this.version = source["CurrentVersion"];
            this.source = source;
        }

        internal string SecretKey
        {
            get { return this.source["AESKey_" + version]; }
        }

        internal string InitialVector
        {
            get { return this.source["AESIV_" + version]; }
        }

        internal string HashKey
        {
            get { return this.source["HashKey_" + version]; }
        }
    }
}