using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiFoundation.Entities
{
    internal sealed class InvocationNotAcceptable
    {
        public string ReturnCode { get; set; }

        public string ErrorMessage { get; set; }
    }
}