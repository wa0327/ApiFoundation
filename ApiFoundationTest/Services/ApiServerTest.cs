using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Http;
using System.Web.Http.SelfHost;
using ApiFoundation.Web.Http.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace ApiFoundation.Services
{
    [TestClass]
    public class ApiServerTest
    {
        #region 起動 Web API server

        private static IApiServer apiServer;
        private static HttpSelfHostServer server;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            var config = new HttpSelfHostConfiguration("http://localhost:8591");

            apiServer = new ApiServer(config);
            apiServer.RequestReceived += (sender, e) =>
            {
                if (e.Request.Content != null)
                {
                    var content = e.Request.Content.ReadAsStringAsync().Result;
                    Trace.TraceInformation("Request received: {0}", content);
                }
            };
            apiServer.SendingResponse += (sender, e) =>
            {
                if (e.Response.Content != null)
                {
                    var content = e.Response.Content.ReadAsStringAsync().Result;
                    Trace.TraceInformation("Sending response: {0}", content);
                }
            };

            config.Routes.MapHttpRoute("API Default", "api/{controller}/{action}");
            server = new HttpSelfHostServer(config);
            server.OpenAsync().Wait();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            server.Dispose();
        }

        #endregion 起動 Web API server

        #region 建立 Web API client

        private IApiClient client;

        [TestInitialize]
        public void TestInitialize()
        {
            this.client = new ApiClient
            {
                BaseAddress = new Uri("http://localhost:8591"),
            };

            client.SendingRequest += (sender, e) =>
            {
                if (e.Request.Content != null)
                {
                    var content = e.Request.Content.ReadAsStringAsync().Result;
                    Trace.TraceInformation("Sending request: {0}", content);
                }
            };

            client.ResponseReceived += (sender, e) =>
            {
                if (e.Response.Content != null)
                {
                    var content = e.Response.Content.ReadAsStringAsync().Result;
                    Trace.TraceInformation("Response received: {0}", content);
                }
            };
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.client.Dispose();
        }

        #endregion 建立 Web API client

        [TestMethod]
        public void ApiServerTest_GetTimeStamp_Convention()
        {
            this.client.Get<object, string>(
                "/api/TimeStampService/GetTimeStamp",
                null,
                response =>
                {
                    Assert.IsNotNull(response);
                    Assert.IsInstanceOfType(response, typeof(string));
                }
            );
        }

        [TestMethod]
        public void ApiServerTest_GetTimestamp()
        {
            this.client.Get<object, string>(
                "/api/Timestamp/Get",
                null,
                response =>
                {
                    Assert.IsNotNull(response);
                    Assert.IsInstanceOfType(response, typeof(string));
                }
            );
        }

        [TestMethod]
        public void ApiServerTest_InvalidModel()
        {
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
        public void ApiServerTest_BusinessError()
        {
            EventHandler<ExceptionEventArgs> exceptionHandler = (sender, e) =>
                {
                    e.IsBusinessError = true;
                    e.ErrorCode = "7533967";
                    e.ErrorMessage = "中毒太深";
                };

            apiServer.Exception += exceptionHandler;
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
            finally
            {
                apiServer.Exception -= exceptionHandler;
            }
        }

        [TestMethod]
        public void ApiServerTest_ProgramError()
        {
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
        public void ApiServerTest_NoAction()
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
        public void ApiServerTest_NoController()
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
        public void ApiServerTest_SmokeTest()
        {
            this.client.Post<SmokeTestRequest, SmokeTestResponse>(
                "/api/ApiServerTest/SmokeTest",
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
    }
}