using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http;

namespace ApiFoundation.Web.Http.Controllers
{
    public class ApiServerTestController : ApiController
    {
        [HttpPost]
        public void InvalidModelTest(InvalidModelRequest request)
        {
        }

        [HttpPost]
        public void BusinessErrorTest()
        {
            throw new BusinessErrorException();
        }

        [HttpPost]
        public void ProgramErrorTest()
        {
            throw new InvalidOperationException("模擬非商業邏輯錯誤。");
        }

        [HttpPost]
        public SmokeTestResponse SmokeTest(SmokeTestRequest request)
        {
            return new SmokeTestResponse
            {
                BoolValue = request.BoolValue,
                ByteValue = request.ByteValue,
                IntValue = request.IntValue,
                StringValue = request.StringValue,
            };
        }
    }
}