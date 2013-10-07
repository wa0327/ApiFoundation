using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Http;
using System.Web.Http.SelfHost;
using ApiFoundation.Utility;
using ApiFoundation.Web.Http.Controllers;
using ApiFoundation.Web.Http.Filters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace ApiFoundation.Services
{
    [TestClass]
    public class ApiServerTest
    {
        private ApiServerWrapper server;
        private ApiClient client;

        [TestInitialize]
        public void TestInitialize()
        {
            this.server = new ApiServerWrapper();
            this.client = new ApiClientWrapper();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.client.Dispose();
            this.server.Dispose();
        }

        [TestMethod]
        public void ApiServer_RequestAndResponse()
        {
            this.client.Post<SmokeTestRequest, SmokeTestResponse>(
                "/api/ApiServerTest/RequestAndResponse",
                () =>
                {
                    return new SmokeTestRequest
                    {
                        BoolValue = true,
                        ByteValue = byte.MaxValue,
                        IntValue = int.MaxValue,
                        StringValue = "string123中文abc",
                    };
                },
                response =>
                {
                    Assert.IsNotNull(response);
                    Assert.IsInstanceOfType(response, typeof(SmokeTestResponse));
                    Assert.AreEqual(true, response.BoolValue);
                    Assert.AreEqual(byte.MaxValue, response.ByteValue);
                    Assert.AreEqual(int.MaxValue, response.IntValue);
                    Assert.AreEqual("string123中文abc", response.StringValue);
                });
        }

        [TestMethod]
        public void ApiServer_RequestOnly()
        {
            this.client.Post<SmokeTestRequest, object>(
                "/api/ApiServerTest/RequestOnly",
                () =>
                {
                    return new SmokeTestRequest
                    {
                        BoolValue = true,
                        ByteValue = byte.MaxValue,
                        IntValue = int.MaxValue,
                        StringValue = "string123中文abc",
                    };
                },
                null);
        }

        [TestMethod]
        public void ApiServer_NullRequest()
        {
            this.client.Post<SmokeTestRequest, object>(
                "/api/ApiServerTest/NullRequest",
                () => null,
                null);
        }

        [TestMethod]
        public void ApiServer_ResponseOnly()
        {
            this.client.Post<object, SmokeTestResponse>(
                "/api/ApiServerTest/ResponseOnly",
                null,
                response =>
                {
                    Assert.IsNotNull(response);
                    Assert.IsInstanceOfType(response, typeof(SmokeTestResponse));
                    Assert.AreEqual(true, response.BoolValue);
                    Assert.AreEqual(byte.MaxValue, response.ByteValue);
                    Assert.AreEqual(int.MaxValue, response.IntValue);
                    Assert.AreEqual("string123中文abc", response.StringValue);
                });
        }

        [TestMethod]
        public void ApiServer_NullResponse()
        {
            this.client.Post<object, SmokeTestResponse>(
                "/api/ApiServerTest/NullResponse",
                null,
                response => Assert.IsNull(response));
        }

        [TestMethod]
        public void ApiServer_ActionOnly()
        {
            this.client.Post<object, object>("/api/ApiServerTest/ActionOnly", null, null);
        }
    }
}