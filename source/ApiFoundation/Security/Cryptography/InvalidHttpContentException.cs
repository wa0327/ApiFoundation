using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiFoundation.Security.Cryptography
{
    public sealed class InvalidHttpContentException : Exception
    {
        public InvalidHttpContentException(Exception innerException)
            : base("Invalid HTTP content.", innerException)
        {
        }
    }
}