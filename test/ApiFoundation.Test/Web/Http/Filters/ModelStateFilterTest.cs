using System.Net.Http;
using ApiFoundation.Net.Http;
using ApiFoundation.Web.Http.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiFoundation.Web.Http.Filters
{
    [TestClass]
    public class ModelStateFilterTest
    {
        [TestMethod]
        public void ModelStateFilterTest_InvalidModel()
        {
            using (var server = new HttpServerWrapper())
            using (HttpClient client = new HttpClientWrapper())
            {
                server.Configuration.Filters.Add(new ModelStateFilter());

                try
                {
                    client.PostJson<InvalidModelRequest, object>(
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
                catch (InvalidModelException ex)
                {
                    Assert.IsNotNull(ex.ModelState);
                    Assert.AreEqual("The UserId field is required.", ex.ModelState["request.UserId"][0]);
                    Assert.AreEqual("The BackyardId field is required.", ex.ModelState["request.BackyardId"][0]);
                    Assert.AreEqual("The CatSubIds field is required.", ex.ModelState["request.CatSubIds"][0]);
                }
            }
        }
    }
}