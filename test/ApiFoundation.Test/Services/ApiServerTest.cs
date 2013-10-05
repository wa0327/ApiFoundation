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
        public void ApiServer_InvalidModel()
        {
            this.server.Configuration.Filters.Add(new ModelStateFilter());

            try
            {
                this.client.Post<InvalidModelRequest, object>(
                    "/api/ApiServerTest/InvalidModelTest",
                    () =>
                    {
                        return new InvalidModelRequest
                        {
                            UserId = null,
                            BackyardId = null,
                            CatSubIds = null,
                        };
                    },
                    null
                );

                Assert.Fail("Did not throw expected exception BadInvocationException.");
            }
            catch (BadInvocationException ex)
            {
                Assert.IsNotNull(ex.ModelState);
                Assert.AreEqual("The UserId field is required.", ex.ModelState["request.UserId"][0]);
                Assert.AreEqual("The BackyardId field is required.", ex.ModelState["request.BackyardId"][0]);
                Assert.AreEqual("The CatSubIds field is required.", ex.ModelState["request.CatSubIds"][0]);
            }
        }

        [TestMethod]
        public void ApiServer_BusinessError_ExceptionFilter()
        {
            var exceptionFilter = new ExceptionFilter();
            exceptionFilter.Exception += (sender, e) =>
            {
                e.IsBusinessError = true;
                e.ErrorCode = "7533967";
                e.ErrorMessage = "中毒太深";
            };
            this.server.Configuration.Filters.Add(exceptionFilter);

            try
            {
                this.client.Post<object, object>(
                    "/api/ApiServerTest/BusinessErrorTest",
                    null,
                    null
                );

                Assert.Fail("Did not throw expected exception InvocationNotAcceptableException.");
            }
            catch (InvocationNotAcceptableException ex)
            {
                Assert.AreEqual("中毒太深", ex.Message);
                Assert.AreEqual("7533967", ex.ErrorCode);
            }
        }

        [TestMethod]
        public void ApiServer_ProgramError()
        {
            this.server.Configuration.Filters.Add(new ExceptionFilter());

            try
            {
                this.client.Post<object, object>(
                    "/api/ApiServerTest/ProgramErrorTest",
                    null,
                    null
                );

                Assert.Fail("Did not throw expected exception HttpServiceException.");
            }
            catch (HttpServiceException ex)
            {
                Assert.AreEqual("模擬非商業邏輯錯誤。", ex.Message);
                Assert.AreEqual(typeof(InvalidOperationException).FullName, ex.ErrorType);
            }
        }

        [TestMethod]
        public void ApiServer_NoAction()
        {
            try
            {
                this.client.Post<object, object>(
                    "/api/ApiServerTest/NoAction",
                    null,
                    null);

                Assert.Fail("Did not throw expected exception HttpServiceException.");
            }
            catch (HttpServiceException ex)
            {
                Assert.IsTrue(ex.Message.StartsWith("No HTTP resource was found"));
                Assert.IsTrue(ex.MessageDetail.StartsWith("No action was found"));
            }
        }

        [TestMethod]
        public void ApiServer_NoController()
        {
            try
            {
                this.client.Post<object, object>(
                    "/api/NoController/NoAction",
                    null,
                    null);

                Assert.Fail("Did not throw expected exception HttpServiceException.");
            }
            catch (HttpServiceException ex)
            {
                Assert.IsTrue(ex.Message.StartsWith("No HTTP resource was found"));
                Assert.IsTrue(ex.MessageDetail.StartsWith("No type was found"));
            }
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
        public void ApiServer_Action()
        {
            this.client.Post<object, object>("/api/ApiServerTest/Action", null, null);
        }
    }
}