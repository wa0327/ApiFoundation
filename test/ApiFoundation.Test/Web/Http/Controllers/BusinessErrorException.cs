using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiFoundation.Web.Http.Controllers
{
    internal class BusinessErrorException : Exception
    {
        internal BusinessErrorException()
            : base("模擬商業邏輯錯誤")
        {
        }
    }
}