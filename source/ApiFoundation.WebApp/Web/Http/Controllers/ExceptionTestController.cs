using System;
using System.Web.Http;

namespace ApiFoundation.Web.Http.Controllers
{
    public class ExceptionTestController : ApiController
    {
        [HttpGet]
        public void ThrowTestingException()
        {
            throw new ExceptionTestException();
        }

        [HttpGet]
        public void Throw(string typeName)
        {
            var type = Type.GetType(typeName, true);

            throw (Exception)Activator.CreateInstance(type);
        }
    }
}