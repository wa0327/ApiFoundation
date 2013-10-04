using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiFoundation.Security.Cryptography
{
    internal class DefaultKeyProvider : IKeyProvider
    {
        private readonly byte[] key = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f };
        private readonly byte[] iv = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f };

        byte[] IKeyProvider.Key
        {
            get { return key; }
        }

        byte[] IKeyProvider.IV
        {
            get { return iv; }
        }
    }
}