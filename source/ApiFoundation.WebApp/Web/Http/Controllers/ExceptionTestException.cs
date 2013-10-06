using System;

namespace ApiFoundation.Web.Http.Controllers
{
    internal sealed class ExceptionTestException : Exception
    {
        public ExceptionTestException()
            : base("此例外僅用於測試當 server 端發生例外時，用於識別為已處理的例外。")
        {
        }
    }
}