using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
        public SmokeTestResponse RequestAndResponse(SmokeTestRequest request)
        {
            return new SmokeTestResponse
            {
                BoolValue = request.BoolValue,
                ByteValue = request.ByteValue,
                IntValue = request.IntValue,
                StringValue = request.StringValue,
            };
        }

        [HttpPost]
        public void RequestOnly(SmokeTestRequest request)
        {
            Assert.IsNotNull(request);
            Assert.AreEqual(true, request.BoolValue);
            Assert.AreEqual(byte.MaxValue, request.ByteValue);
            Assert.AreEqual(int.MaxValue, request.IntValue);
            Assert.AreEqual("string123中文abc", request.StringValue);
        }

        [HttpPost]
        public void NullRequest(SmokeTestRequest request)
        {
            Assert.IsNull(request);
        }

        [HttpPost]
        public SmokeTestResponse ResponseOnly()
        {
            return new SmokeTestResponse
            {
                BoolValue = true,
                ByteValue = byte.MaxValue,
                IntValue = int.MaxValue,
                StringValue = "string123中文abc",
            };
        }

        [HttpPost]
        public SmokeTestResponse NullResponse()
        {
            return null;
        }

        [HttpPost]
        public void Action()
        {
        }
    }
}