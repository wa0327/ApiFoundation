using ApiFoundation.Web.Http.Controllers;

namespace ApiFoundation.Web.Http.Filters
{
    internal sealed class CustomExceptionFilter : ExceptionFilter
    {
        protected override void OnException(ExceptionEventArgs e)
        {
            if (e.Exception is ExceptionTestException)
            {
                e.Handled = true;
                e.ReturnCode = "這是測試回傳的 return code.";
                e.Message = "這是測試回傳的 message.";
            }
            else if (e.Exception is BusinessErrorException)
            {
                e.Handled = true;
                e.ReturnCode = "7533967";
                e.Message = "中毒太深";
            }

            base.OnException(e);
        }
    }
}