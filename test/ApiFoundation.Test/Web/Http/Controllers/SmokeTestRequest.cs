using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiFoundation.Web.Http.Controllers
{
    public sealed class SmokeTestRequest
    {
        public bool BoolValue { get; set; }

        public byte ByteValue { get; set; }

        public int IntValue { get; set; }

        public string StringValue { get; set; }
    }
}