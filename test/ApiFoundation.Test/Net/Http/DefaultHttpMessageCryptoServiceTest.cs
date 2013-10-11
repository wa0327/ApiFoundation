using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using ApiFoundation.Security.Cryptography;
using ApiFoundation.Web.Http.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Rhino.Mocks;

namespace ApiFoundation.Net.Http
{
    [TestClass]
    public class DefaultHttpMessageCryptoServiceTest
    {
        #region Normal tests

        #region Null plain content tests

        [TestMethod]
        public void DefaultHttpMessageCryptoServiceTest_EncryptRequest_NullPlainContent()
        {
            var input = new HttpRequestMessage
            {
                Content = null
            };

            var timestampProvider = MockRepository.GenerateStub<ITimestampProvider<long>>();
            timestampProvider
                .Stub(o => o.GetTimestamp())
                .Return(12345);

            using (var cryptoService = new DefaultCryptoService("secretKeyPassword", "initialVectorPassword", "hashKeyString"))
            using (var target = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider))
            {
                var actual = target.Encrypt(input);
                Assert.IsNotNull(actual);
                Assert.IsNotNull(actual.Content);

                var data = actual.Content.ReadAsAsync<JObject>().Result;
                Assert.IsNotNull(data);
                Assert.AreEqual(12345, data["Timestamp"]);
                Assert.AreEqual("0wz/LjHKyXy3zsWb2ny1IhVZ6Q3wmTb2i9G1tYig+RWthxb0lkFkadT/vnZur1t8NeSvGP3eVmS7Z/hm7Fi5WA==", data["CipherText"]);
                Assert.AreEqual("4033e2085d273ff20441cfc181805de8f5b6f0d4e24ef3eaae595cfaf4973c8353e72d9fdc9865239708683fde7fe70bc3fcef18eea8735e4058660a816181ff", data["Signature"]);
            }
        }

        [TestMethod]
        public void DefaultHttpMessageCryptoServiceTest_DecryptRequest_NullPlainContent()
        {
            var cipherContent = new
            {
                Timestamp = 12345,
                CipherText = "0wz/LjHKyXy3zsWb2ny1IhVZ6Q3wmTb2i9G1tYig+RWthxb0lkFkadT/vnZur1t8NeSvGP3eVmS7Z/hm7Fi5WA==",
                Signature = "4033e2085d273ff20441cfc181805de8f5b6f0d4e24ef3eaae595cfaf4973c8353e72d9fdc9865239708683fde7fe70bc3fcef18eea8735e4058660a816181ff"
            };

            var input = new HttpRequestMessage
            {
                Content = new ObjectContent(cipherContent.GetType(), cipherContent, new JsonMediaTypeFormatter())
            };

            var timestampProvider = MockRepository.GenerateStub<ITimestampProvider<long>>();
            timestampProvider
                .Stub(o => o.Validate(Arg<long>.Is.Equal(12345)));

            using (var cryptoService = new DefaultCryptoService("secretKeyPassword", "initialVectorPassword", "hashKeyString"))
            using (var target = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider))
            {
                var actual = target.Decrypt(input);
                Assert.IsNotNull(actual);
                Assert.IsNull(actual.Content);
            }
        }

        [TestMethod]
        public void DefaultHttpMessageCryptoServiceTest_EncryptResponse_NullPlainContent()
        {
            var input = new HttpResponseMessage
            {
                Content = null
            };

            var timestampProvider = MockRepository.GenerateStub<ITimestampProvider<long>>();
            using (var cryptoService = new DefaultCryptoService("secretKeyPassword", "initialVectorPassword", "hashKeyString"))
            using (var target = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider))
            {
                var po = new PrivateObject(target);
                po.SetField("lastTimestamp", 12345);

                var actual = target.Encrypt(input);
                Assert.IsNotNull(actual);
                Assert.IsNotNull(actual.Content);

                var data = actual.Content.ReadAsAsync<JObject>().Result;
                Assert.IsNotNull(data);
                Assert.AreEqual(12345, data["Timestamp"]);
                Assert.AreEqual("0wz/LjHKyXy3zsWb2ny1IhVZ6Q3wmTb2i9G1tYig+RWthxb0lkFkadT/vnZur1t8NeSvGP3eVmS7Z/hm7Fi5WA==", data["CipherText"]);
                Assert.AreEqual("4033e2085d273ff20441cfc181805de8f5b6f0d4e24ef3eaae595cfaf4973c8353e72d9fdc9865239708683fde7fe70bc3fcef18eea8735e4058660a816181ff", data["Signature"]);
            }
        }

        [TestMethod]
        public void DefaultHttpMessageCryptoServiceTest_DecryptResponse_NullPlainContent()
        {
            var cipherContent = new
            {
                Timestamp = 12345,
                CipherText = "0wz/LjHKyXy3zsWb2ny1IhVZ6Q3wmTb2i9G1tYig+RWthxb0lkFkadT/vnZur1t8NeSvGP3eVmS7Z/hm7Fi5WA==",
                Signature = "4033e2085d273ff20441cfc181805de8f5b6f0d4e24ef3eaae595cfaf4973c8353e72d9fdc9865239708683fde7fe70bc3fcef18eea8735e4058660a816181ff"
            };

            var input = new HttpResponseMessage
            {
                Content = new ObjectContent(cipherContent.GetType(), cipherContent, new JsonMediaTypeFormatter())
            };

            var timestampProvider = MockRepository.GenerateStub<ITimestampProvider<long>>();
            using (var cryptoService = new DefaultCryptoService("secretKeyPassword", "initialVectorPassword", "hashKeyString"))
            using (var target = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider))
            {
                var po = new PrivateObject(target);
                po.SetField("lastTimestamp", 12345);

                var actual = target.Decrypt(input);
                Assert.IsNotNull(actual);
                Assert.AreEqual(HttpStatusCode.NoContent, actual.StatusCode);
                Assert.IsNull(actual.Content);
            }
        }

        #endregion Null plain content tests

        #region Has content tests

        [TestMethod]
        public void DefaultHttpMessageCryptoServiceTest_EncryptRequest_HasContent()
        {
            var plainContent = new
            {
                ThisIsString = "abcABC中文",
                ThisIsInteger = int.MaxValue,
                ThisIsBoolean = true
            };

            var input = new HttpRequestMessage
            {
                Content = new ObjectContent(plainContent.GetType(), plainContent, new JsonMediaTypeFormatter())
            };

            var timestampProvider = MockRepository.GenerateStub<ITimestampProvider<long>>();
            timestampProvider
                .Stub(o => o.GetTimestamp())
                .Return(12345);

            using (var cryptoService = new DefaultCryptoService("secretKeyPassword", "initialVectorPassword", "hashKeyString"))
            using (var target = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider))
            {
                var actual = target.Encrypt(input);
                Assert.IsNotNull(actual);
                Assert.IsNotNull(actual.Content);

                var cipherContent = actual.Content.ReadAsAsync<JObject>().Result;
                Assert.IsNotNull(cipherContent);
                Assert.AreEqual(12345, cipherContent["Timestamp"]);
                Assert.AreEqual("zcbpurnokr/F/dzilj9hqFhMbtNR9jN9plz0TkNeFAPj38ryATycD38BW98AaHUJgHtPiUSzdO8shhkEuidjq/2NJ/n4WlJKK4lKO/kx5J8=", cipherContent["CipherText"]);
                Assert.AreEqual("c31553a7eaf726c50084791498a736305924db05f68640cfb5fc93c3e6482ca3c520d275f0118527fa1bace612738e67e035d7aeb91442ffac7c07c1c2cceaa3", cipherContent["Signature"]);
            }
        }

        [TestMethod]
        public void DefaultHttpMessageCryptoServiceTest_DecryptRequest_HasContent()
        {
            var cipherContent = new
            {
                Timestamp = 12345,
                CipherText = "zcbpurnokr/F/dzilj9hqFhMbtNR9jN9plz0TkNeFAPj38ryATycD38BW98AaHUJgHtPiUSzdO8shhkEuidjq/2NJ/n4WlJKK4lKO/kx5J8=",
                Signature = "c31553a7eaf726c50084791498a736305924db05f68640cfb5fc93c3e6482ca3c520d275f0118527fa1bace612738e67e035d7aeb91442ffac7c07c1c2cceaa3"
            };

            var input = new HttpRequestMessage
            {
                Content = new ObjectContent(cipherContent.GetType(), cipherContent, new JsonMediaTypeFormatter())
            };

            var timestampProvider = MockRepository.GenerateStub<ITimestampProvider<long>>();
            timestampProvider
                .Stub(o => o.Validate(Arg<long>.Is.Equal(12345)));

            using (var cryptoService = new DefaultCryptoService("secretKeyPassword", "initialVectorPassword", "hashKeyString"))
            using (var target = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider))
            {
                var actual = target.Decrypt(input);
                Assert.IsNotNull(actual);
                Assert.IsNotNull(actual.Content);

                var plainContent = actual.Content.ReadAsAsync<JObject>().Result;
                Assert.IsNotNull(plainContent);
                Assert.AreEqual("abcABC中文", plainContent["ThisIsString"]);
                Assert.AreEqual(int.MaxValue, plainContent["ThisIsInteger"]);
                Assert.AreEqual(true, plainContent["ThisIsBoolean"]);
            }
        }

        [TestMethod]
        public void DefaultHttpMessageCryptoServiceTest_EncryptResponse_HasContent()
        {
            var plainContent = new
            {
                ThisIsString = "abcABC中文",
                ThisIsInteger = int.MaxValue,
                ThisIsBoolean = true
            };

            var input = new HttpResponseMessage
            {
                Content = new ObjectContent(plainContent.GetType(), plainContent, new JsonMediaTypeFormatter())
            };

            var timestampProvider = MockRepository.GenerateStub<ITimestampProvider<long>>();
            using (var cryptoService = new DefaultCryptoService("secretKeyPassword", "initialVectorPassword", "hashKeyString"))
            using (var target = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider))
            {
                var po = new PrivateObject(target);
                po.SetField("lastTimestamp", 12345);

                var actual = target.Encrypt(input);
                Assert.IsNotNull(actual);
                Assert.IsNotNull(actual.Content);

                var data = actual.Content.ReadAsAsync<JObject>().Result;
                Assert.IsNotNull(data);
                Assert.AreEqual(12345, data["Timestamp"]);
                Assert.AreEqual("zcbpurnokr/F/dzilj9hqFhMbtNR9jN9plz0TkNeFAPj38ryATycD38BW98AaHUJgHtPiUSzdO8shhkEuidjq/2NJ/n4WlJKK4lKO/kx5J8=", data["CipherText"]);
                Assert.AreEqual("c31553a7eaf726c50084791498a736305924db05f68640cfb5fc93c3e6482ca3c520d275f0118527fa1bace612738e67e035d7aeb91442ffac7c07c1c2cceaa3", data["Signature"]);
            }
        }

        [TestMethod]
        public void DefaultHttpMessageCryptoServiceTest_DecryptResponse_HasContent()
        {
            var cipherContent = new
            {
                Timestamp = 12345,
                CipherText = "zcbpurnokr/F/dzilj9hqFhMbtNR9jN9plz0TkNeFAPj38ryATycD38BW98AaHUJgHtPiUSzdO8shhkEuidjq/2NJ/n4WlJKK4lKO/kx5J8=",
                Signature = "c31553a7eaf726c50084791498a736305924db05f68640cfb5fc93c3e6482ca3c520d275f0118527fa1bace612738e67e035d7aeb91442ffac7c07c1c2cceaa3"
            };

            var input = new HttpResponseMessage
            {
                Content = new ObjectContent(cipherContent.GetType(), cipherContent, new JsonMediaTypeFormatter())
            };

            var timestampProvider = MockRepository.GenerateStub<ITimestampProvider<long>>();
            using (var cryptoService = new DefaultCryptoService("secretKeyPassword", "initialVectorPassword", "hashKeyString"))
            using (var target = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider))
            {
                var po = new PrivateObject(target);
                po.SetField("lastTimestamp", 12345);

                var actual = target.Decrypt(input);
                Assert.IsNotNull(actual);
                Assert.AreEqual(HttpStatusCode.OK, actual.StatusCode);
                Assert.IsNotNull(actual.Content);

                var plainContent = actual.Content.ReadAsAsync<JObject>().Result;
                Assert.IsNotNull(plainContent);
                Assert.AreEqual("abcABC中文", plainContent["ThisIsString"]);
                Assert.AreEqual(int.MaxValue, plainContent["ThisIsInteger"]);
                Assert.AreEqual(true, plainContent["ThisIsBoolean"]);
            }
        }

        #endregion Has content tests

        #endregion Normal tests

        #region Expected error tests

        [TestMethod]
        public void DefaultHttpMessageCryptoServiceTest_DecryptRequest_NullCipherContent()
        {
            var input = new HttpRequestMessage
            {
                Content = null
            };

            var timestampProvider = MockRepository.GenerateStub<ITimestampProvider<long>>();
            using (var cryptoService = new DefaultCryptoService("secretKeyPassword", "initialVectorPassword", "hashKeyString"))
            using (var target = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider))
            {
                try
                {
                    target.Decrypt(input);
                    Assert.Fail("Did not throw expected exception InvalidHttpMessageException.");
                }
                catch (InvalidHttpMessageException ex)
                {
                    Assert.IsNotNull(ex.InnerException);
                    Assert.IsInstanceOfType(ex.InnerException, typeof(HttpContentNullException));
                }
            }
        }

        [TestMethod]
        public void DefaultHttpMessageCryptoServiceTest_DecryptRequest_WithoutTimestamp()
        {
            var cipherContent = new
            {
                CipherText = "zcbpurnokr/F/dzilj9hqFhMbtNR9jN9plz0TkNeFAPj38ryATycD38BW98AaHUJgHtPiUSzdO8shhkEuidjq/2NJ/n4WlJKK4lKO/kx5J8=",
                Signature = "c31553a7eaf726c50084791498a736305924db05f68640cfb5fc93c3e6482ca3c520d275f0118527fa1bace612738e67e035d7aeb91442ffac7c07c1c2cceaa3"
            };

            var input = new HttpRequestMessage
            {
                Content = new ObjectContent(cipherContent.GetType(), cipherContent, new JsonMediaTypeFormatter())
            };

            var timestampProvider = MockRepository.GenerateStub<ITimestampProvider<long>>();
            using (var cryptoService = new DefaultCryptoService("secretKeyPassword", "initialVectorPassword", "hashKeyString"))
            using (var target = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider))
            {
                try
                {
                    target.Decrypt(input);
                    Assert.Fail("Did not throw expected exception InvalidHttpMessageException.");
                }
                catch (InvalidHttpMessageException ex)
                {
                    Assert.IsNotNull(ex.InnerException);
                    Assert.IsInstanceOfType(ex.InnerException, typeof(TimestampNullException));
                }
            }
        }

        [TestMethod]
        public void DefaultHttpMessageCryptoServiceTest_DecryptRequest_NullTimestamp()
        {
            var cipherContent = new
            {
                Timestamp = (object)null,
                CipherText = "zcbpurnokr/F/dzilj9hqFhMbtNR9jN9plz0TkNeFAPj38ryATycD38BW98AaHUJgHtPiUSzdO8shhkEuidjq/2NJ/n4WlJKK4lKO/kx5J8=",
                Signature = "c31553a7eaf726c50084791498a736305924db05f68640cfb5fc93c3e6482ca3c520d275f0118527fa1bace612738e67e035d7aeb91442ffac7c07c1c2cceaa3"
            };

            var input = new HttpRequestMessage
            {
                Content = new ObjectContent(cipherContent.GetType(), cipherContent, new JsonMediaTypeFormatter())
            };

            var timestampProvider = MockRepository.GenerateStub<ITimestampProvider<long>>();
            using (var cryptoService = new DefaultCryptoService("secretKeyPassword", "initialVectorPassword", "hashKeyString"))
            using (var target = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider))
            {
                try
                {
                    target.Decrypt(input);
                    Assert.Fail("Did not throw expected exception InvalidHttpMessageException.");
                }
                catch (InvalidHttpMessageException ex)
                {
                    Assert.IsNotNull(ex.InnerException);
                    Assert.IsInstanceOfType(ex.InnerException, typeof(TimestampNullException));
                }
            }
        }

        [TestMethod]
        public void DefaultHttpMessageCryptoServiceTest_DecryptRequest_EmptyTimestamp()
        {
            var cipherContent = new
            {
                Timestamp = string.Empty,
                CipherText = "zcbpurnokr/F/dzilj9hqFhMbtNR9jN9plz0TkNeFAPj38ryATycD38BW98AaHUJgHtPiUSzdO8shhkEuidjq/2NJ/n4WlJKK4lKO/kx5J8=",
                Signature = "c31553a7eaf726c50084791498a736305924db05f68640cfb5fc93c3e6482ca3c520d275f0118527fa1bace612738e67e035d7aeb91442ffac7c07c1c2cceaa3"
            };

            var input = new HttpRequestMessage
            {
                Content = new ObjectContent(cipherContent.GetType(), cipherContent, new JsonMediaTypeFormatter())
            };

            var timestampProvider = MockRepository.GenerateStub<ITimestampProvider<long>>();
            using (var cryptoService = new DefaultCryptoService("secretKeyPassword", "initialVectorPassword", "hashKeyString"))
            using (var target = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider))
            {
                try
                {
                    target.Decrypt(input);
                    Assert.Fail("Did not throw expected exception InvalidHttpMessageException.");
                }
                catch (InvalidHttpMessageException ex)
                {
                    Assert.IsNotNull(ex.InnerException);
                    Assert.IsInstanceOfType(ex.InnerException, typeof(InvalidTimestampException));

                    var actualException = (InvalidTimestampException)ex.InnerException;
                    Assert.AreEqual(string.Empty, actualException.Timestamp);
                }
            }
        }

        [TestMethod]
        public void DefaultHttpMessageCryptoServiceTest_DecryptRequest_InvalidTimestamp()
        {
            var cipherContent = new
            {
                Timestamp = "Timestamp should be numeric.",
                CipherText = "zcbpurnokr/F/dzilj9hqFhMbtNR9jN9plz0TkNeFAPj38ryATycD38BW98AaHUJgHtPiUSzdO8shhkEuidjq/2NJ/n4WlJKK4lKO/kx5J8=",
                Signature = "c31553a7eaf726c50084791498a736305924db05f68640cfb5fc93c3e6482ca3c520d275f0118527fa1bace612738e67e035d7aeb91442ffac7c07c1c2cceaa3"
            };

            var input = new HttpRequestMessage
            {
                Content = new ObjectContent(cipherContent.GetType(), cipherContent, new JsonMediaTypeFormatter())
            };

            var timestampProvider = MockRepository.GenerateStub<ITimestampProvider<long>>();
            using (var cryptoService = new DefaultCryptoService("secretKeyPassword", "initialVectorPassword", "hashKeyString"))
            using (var target = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider))
            {
                try
                {
                    target.Decrypt(input);
                    Assert.Fail("Did not throw expected exception InvalidHttpMessageException.");
                }
                catch (InvalidHttpMessageException ex)
                {
                    Assert.IsNotNull(ex.InnerException);
                    Assert.IsInstanceOfType(ex.InnerException, typeof(InvalidTimestampException));

                    var actualException = (InvalidTimestampException)ex.InnerException;
                    Assert.AreEqual("Timestamp should be numeric.", actualException.Timestamp);
                    Assert.IsInstanceOfType(actualException.InnerException, typeof(FormatException));
                }
            }
        }

        [TestMethod]
        public void DefaultHttpMessageCryptoServiceTest_DecryptRequest_WithoutCipherText()
        {
            var cipherContent = new
            {
                Timestamp = 12345,
                Signature = "c31553a7eaf726c50084791498a736305924db05f68640cfb5fc93c3e6482ca3c520d275f0118527fa1bace612738e67e035d7aeb91442ffac7c07c1c2cceaa3"
            };

            var input = new HttpRequestMessage
            {
                Content = new ObjectContent(cipherContent.GetType(), cipherContent, new JsonMediaTypeFormatter())
            };

            var timestampProvider = MockRepository.GenerateStub<ITimestampProvider<long>>();
            using (var cryptoService = new DefaultCryptoService("secretKeyPassword", "initialVectorPassword", "hashKeyString"))
            using (var target = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider))
            {
                try
                {
                    target.Decrypt(input);
                    Assert.Fail("Did not throw expected exception InvalidHttpMessageException.");
                }
                catch (InvalidHttpMessageException ex)
                {
                    Assert.IsNotNull(ex.InnerException);
                    Assert.IsInstanceOfType(ex.InnerException, typeof(CipherTextNullException));
                }
            }
        }

        [TestMethod]
        public void DefaultHttpMessageCryptoServiceTest_DecryptRequest_NullCipherText()
        {
            var cipherContent = new
            {
                Timestamp = 12345,
                CipherText = (object)null,
                Signature = "c31553a7eaf726c50084791498a736305924db05f68640cfb5fc93c3e6482ca3c520d275f0118527fa1bace612738e67e035d7aeb91442ffac7c07c1c2cceaa3"
            };

            var input = new HttpRequestMessage
            {
                Content = new ObjectContent(cipherContent.GetType(), cipherContent, new JsonMediaTypeFormatter())
            };

            var timestampProvider = MockRepository.GenerateStub<ITimestampProvider<long>>();
            using (var cryptoService = new DefaultCryptoService("secretKeyPassword", "initialVectorPassword", "hashKeyString"))
            using (var target = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider))
            {
                try
                {
                    target.Decrypt(input);
                    Assert.Fail("Did not throw expected exception InvalidHttpMessageException.");
                }
                catch (InvalidHttpMessageException ex)
                {
                    Assert.IsNotNull(ex.InnerException);
                    Assert.IsInstanceOfType(ex.InnerException, typeof(CipherTextNullException));
                }
            }
        }

        [TestMethod]
        public void DefaultHttpMessageCryptoServiceTest_DecryptRequest_EmptyCipherText()
        {
            var cipherContent = new
            {
                Timestamp = 12345,
                CipherText = string.Empty,
                Signature = "c31553a7eaf726c50084791498a736305924db05f68640cfb5fc93c3e6482ca3c520d275f0118527fa1bace612738e67e035d7aeb91442ffac7c07c1c2cceaa3"
            };

            var input = new HttpRequestMessage
            {
                Content = new ObjectContent(cipherContent.GetType(), cipherContent, new JsonMediaTypeFormatter())
            };

            var timestampProvider = MockRepository.GenerateStub<ITimestampProvider<long>>();
            using (var cryptoService = new DefaultCryptoService("secretKeyPassword", "initialVectorPassword", "hashKeyString"))
            using (var target = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider))
            {
                try
                {
                    target.Decrypt(input);
                    Assert.Fail("Did not throw expected exception InvalidHttpMessageException.");
                }
                catch (InvalidHttpMessageException ex)
                {
                    Assert.IsNotNull(ex.InnerException);
                    Assert.IsInstanceOfType(ex.InnerException, typeof(InvalidCipherTextException));

                    var actualException = (InvalidCipherTextException)ex.InnerException;
                    Assert.AreEqual(string.Empty, actualException.CipherText);
                }
            }
        }

        [TestMethod]
        public void DefaultHttpMessageCryptoServiceTest_DecryptRequest_InvalidCipherText()
        {
            var cipherContent = new
            {
                Timestamp = 12345,
                CipherText = "This is not a legal BASE64 string.",
                Signature = "c31553a7eaf726c50084791498a736305924db05f68640cfb5fc93c3e6482ca3c520d275f0118527fa1bace612738e67e035d7aeb91442ffac7c07c1c2cceaa3"
            };

            var input = new HttpRequestMessage
            {
                Content = new ObjectContent(cipherContent.GetType(), cipherContent, new JsonMediaTypeFormatter())
            };

            var timestampProvider = MockRepository.GenerateStub<ITimestampProvider<long>>();
            using (var cryptoService = new DefaultCryptoService("secretKeyPassword", "initialVectorPassword", "hashKeyString"))
            using (var target = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider))
            {
                try
                {
                    target.Decrypt(input);
                    Assert.Fail("Did not throw expected exception InvalidHttpMessageException.");
                }
                catch (InvalidHttpMessageException ex)
                {
                    Assert.IsNotNull(ex.InnerException);
                    Assert.IsInstanceOfType(ex.InnerException, typeof(InvalidCipherTextException));

                    var actualException = (InvalidCipherTextException)ex.InnerException;
                    Assert.AreEqual("This is not a legal BASE64 string.", actualException.CipherText);
                    Assert.IsInstanceOfType(actualException.InnerException, typeof(FormatException));
                }
            }
        }

        [TestMethod]
        public void DefaultHttpMessageCryptoServiceTest_DecryptRequest_WithoutSignature()
        {
            var cipherContent = new
            {
                Timestamp = 12345,
                CipherText = "zcbpurnokr/F/dzilj9hqFhMbtNR9jN9plz0TkNeFAPj38ryATycD38BW98AaHUJgHtPiUSzdO8shhkEuidjq/2NJ/n4WlJKK4lKO/kx5J8=",
            };

            var input = new HttpRequestMessage
            {
                Content = new ObjectContent(cipherContent.GetType(), cipherContent, new JsonMediaTypeFormatter())
            };

            var timestampProvider = MockRepository.GenerateStub<ITimestampProvider<long>>();
            using (var cryptoService = new DefaultCryptoService("secretKeyPassword", "initialVectorPassword", "hashKeyString"))
            using (var target = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider))
            {
                try
                {
                    target.Decrypt(input);
                    Assert.Fail("Did not throw expected exception InvalidHttpMessageException.");
                }
                catch (InvalidHttpMessageException ex)
                {
                    Assert.IsNotNull(ex.InnerException);
                    Assert.IsInstanceOfType(ex.InnerException, typeof(SignatureNullException));
                }
            }
        }

        [TestMethod]
        public void DefaultHttpMessageCryptoServiceTest_DecryptRequest_NullSignature()
        {
            var cipherContent = new
            {
                Timestamp = 12345,
                CipherText = "zcbpurnokr/F/dzilj9hqFhMbtNR9jN9plz0TkNeFAPj38ryATycD38BW98AaHUJgHtPiUSzdO8shhkEuidjq/2NJ/n4WlJKK4lKO/kx5J8=",
                Signature = (object)null
            };

            var input = new HttpRequestMessage
            {
                Content = new ObjectContent(cipherContent.GetType(), cipherContent, new JsonMediaTypeFormatter())
            };

            var timestampProvider = MockRepository.GenerateStub<ITimestampProvider<long>>();
            using (var cryptoService = new DefaultCryptoService("secretKeyPassword", "initialVectorPassword", "hashKeyString"))
            using (var target = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider))
            {
                try
                {
                    target.Decrypt(input);
                    Assert.Fail("Did not throw expected exception InvalidHttpMessageException.");
                }
                catch (InvalidHttpMessageException ex)
                {
                    Assert.IsNotNull(ex.InnerException);
                    Assert.IsInstanceOfType(ex.InnerException, typeof(SignatureNullException));
                }
            }
        }

        [TestMethod]
        public void DefaultHttpMessageCryptoServiceTest_DecryptRequest_EmptySignature()
        {
            var cipherContent = new
            {
                Timestamp = 12345,
                CipherText = "zcbpurnokr/F/dzilj9hqFhMbtNR9jN9plz0TkNeFAPj38ryATycD38BW98AaHUJgHtPiUSzdO8shhkEuidjq/2NJ/n4WlJKK4lKO/kx5J8=",
                Signature = string.Empty
            };

            var input = new HttpRequestMessage
            {
                Content = new ObjectContent(cipherContent.GetType(), cipherContent, new JsonMediaTypeFormatter())
            };

            var timestampProvider = MockRepository.GenerateStub<ITimestampProvider<long>>();
            using (var cryptoService = new DefaultCryptoService("secretKeyPassword", "initialVectorPassword", "hashKeyString"))
            using (var target = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider))
            {
                try
                {
                    target.Decrypt(input);
                    Assert.Fail("Did not throw expected exception InvalidHttpMessageException.");
                }
                catch (InvalidHttpMessageException ex)
                {
                    Assert.IsNotNull(ex.InnerException);
                    Assert.IsInstanceOfType(ex.InnerException, typeof(InvalidSignatureException));

                    var actualException = (InvalidSignatureException)ex.InnerException;
                    Assert.AreEqual(string.Empty, actualException.Signature);
                }
            }
        }

        [TestMethod]
        public void DefaultHttpMessageCryptoServiceTest_DecryptRequest_InvalidSignature()
        {
            var cipherContent = new
            {
                Timestamp = 12345,
                CipherText = "zcbpurnokr/F/dzilj9hqFhMbtNR9jN9plz0TkNeFAPj38ryATycD38BW98AaHUJgHtPiUSzdO8shhkEuidjq/2NJ/n4WlJKK4lKO/kx5J8=",
                Signature = "The length of signature should be 128."
            };

            var input = new HttpRequestMessage
            {
                Content = new ObjectContent(cipherContent.GetType(), cipherContent, new JsonMediaTypeFormatter())
            };

            var timestampProvider = MockRepository.GenerateStub<ITimestampProvider<long>>();
            using (var cryptoService = new DefaultCryptoService("secretKeyPassword", "initialVectorPassword", "hashKeyString"))
            using (var target = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider))
            {
                try
                {
                    target.Decrypt(input);
                    Assert.Fail("Did not throw expected exception InvalidHttpMessageException.");
                }
                catch (InvalidHttpMessageException ex)
                {
                    Assert.IsNotNull(ex.InnerException);
                    Assert.IsInstanceOfType(ex.InnerException, typeof(InvalidSignatureException));

                    var actualException = (InvalidSignatureException)ex.InnerException;
                    Assert.AreEqual("The length of signature should be 128.", actualException.Signature);
                }
            }
        }

        [TestMethod]
        public void DefaultHttpMessageCryptoServiceTest_DecryptResponse_NullCipherContent()
        {
            var input = new HttpResponseMessage
            {
                Content = null
            };

            var timestampProvider = MockRepository.GenerateStub<ITimestampProvider<long>>();
            using (var cryptoService = new DefaultCryptoService("secretKeyPassword", "initialVectorPassword", "hashKeyString"))
            using (var target = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider))
            {
                try
                {
                    target.Decrypt(input);
                    Assert.Fail("Did not throw expected exception InvalidHttpMessageException.");
                }
                catch (InvalidHttpMessageException ex)
                {
                    Assert.IsNotNull(ex.InnerException);
                    Assert.IsInstanceOfType(ex.InnerException, typeof(HttpContentNullException));
                }
            }
        }

        [TestMethod]
        public void DefaultHttpMessageCryptoServiceTest_DecryptResponse_WithoutTimestamp()
        {
            var cipherContent = new
            {
                CipherText = "zcbpurnokr/F/dzilj9hqFhMbtNR9jN9plz0TkNeFAPj38ryATycD38BW98AaHUJgHtPiUSzdO8shhkEuidjq/2NJ/n4WlJKK4lKO/kx5J8=",
                Signature = "c31553a7eaf726c50084791498a736305924db05f68640cfb5fc93c3e6482ca3c520d275f0118527fa1bace612738e67e035d7aeb91442ffac7c07c1c2cceaa3"
            };

            var input = new HttpResponseMessage
            {
                Content = new ObjectContent(cipherContent.GetType(), cipherContent, new JsonMediaTypeFormatter())
            };

            var timestampProvider = MockRepository.GenerateStub<ITimestampProvider<long>>();
            using (var cryptoService = new DefaultCryptoService("secretKeyPassword", "initialVectorPassword", "hashKeyString"))
            using (var target = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider))
            {
                try
                {
                    target.Decrypt(input);
                    Assert.Fail("Did not throw expected exception InvalidHttpMessageException.");
                }
                catch (InvalidHttpMessageException ex)
                {
                    Assert.IsNotNull(ex.InnerException);
                    Assert.IsInstanceOfType(ex.InnerException, typeof(TimestampNullException));
                }
            }
        }

        [TestMethod]
        public void DefaultHttpMessageCryptoServiceTest_DecryptResponse_NullTimestamp()
        {
            var cipherContent = new
            {
                Timestamp = (object)null,
                CipherText = "zcbpurnokr/F/dzilj9hqFhMbtNR9jN9plz0TkNeFAPj38ryATycD38BW98AaHUJgHtPiUSzdO8shhkEuidjq/2NJ/n4WlJKK4lKO/kx5J8=",
                Signature = "c31553a7eaf726c50084791498a736305924db05f68640cfb5fc93c3e6482ca3c520d275f0118527fa1bace612738e67e035d7aeb91442ffac7c07c1c2cceaa3"
            };

            var input = new HttpResponseMessage
            {
                Content = new ObjectContent(cipherContent.GetType(), cipherContent, new JsonMediaTypeFormatter())
            };

            var timestampProvider = MockRepository.GenerateStub<ITimestampProvider<long>>();
            using (var cryptoService = new DefaultCryptoService("secretKeyPassword", "initialVectorPassword", "hashKeyString"))
            using (var target = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider))
            {
                try
                {
                    target.Decrypt(input);
                    Assert.Fail("Did not throw expected exception InvalidHttpMessageException.");
                }
                catch (InvalidHttpMessageException ex)
                {
                    Assert.IsNotNull(ex.InnerException);
                    Assert.IsInstanceOfType(ex.InnerException, typeof(TimestampNullException));
                }
            }
        }

        [TestMethod]
        public void DefaultHttpMessageCryptoServiceTest_DecryptResponse_EmptyTimestamp()
        {
            var cipherContent = new
            {
                Timestamp = string.Empty,
                CipherText = "zcbpurnokr/F/dzilj9hqFhMbtNR9jN9plz0TkNeFAPj38ryATycD38BW98AaHUJgHtPiUSzdO8shhkEuidjq/2NJ/n4WlJKK4lKO/kx5J8=",
                Signature = "c31553a7eaf726c50084791498a736305924db05f68640cfb5fc93c3e6482ca3c520d275f0118527fa1bace612738e67e035d7aeb91442ffac7c07c1c2cceaa3"
            };

            var input = new HttpResponseMessage
            {
                Content = new ObjectContent(cipherContent.GetType(), cipherContent, new JsonMediaTypeFormatter())
            };

            var timestampProvider = MockRepository.GenerateStub<ITimestampProvider<long>>();
            using (var cryptoService = new DefaultCryptoService("secretKeyPassword", "initialVectorPassword", "hashKeyString"))
            using (var target = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider))
            {
                try
                {
                    target.Decrypt(input);
                    Assert.Fail("Did not throw expected exception InvalidHttpMessageException.");
                }
                catch (InvalidHttpMessageException ex)
                {
                    Assert.IsNotNull(ex.InnerException);
                    Assert.IsInstanceOfType(ex.InnerException, typeof(InvalidTimestampException));

                    var actualException = (InvalidTimestampException)ex.InnerException;
                    Assert.AreEqual(string.Empty, actualException.Timestamp);
                }
            }
        }

        [TestMethod]
        public void DefaultHttpMessageCryptoServiceTest_DecryptResponse_InvalidTimestamp()
        {
            var cipherContent = new
            {
                Timestamp = "Timestamp should be numeric.",
                CipherText = "zcbpurnokr/F/dzilj9hqFhMbtNR9jN9plz0TkNeFAPj38ryATycD38BW98AaHUJgHtPiUSzdO8shhkEuidjq/2NJ/n4WlJKK4lKO/kx5J8=",
                Signature = "c31553a7eaf726c50084791498a736305924db05f68640cfb5fc93c3e6482ca3c520d275f0118527fa1bace612738e67e035d7aeb91442ffac7c07c1c2cceaa3"
            };

            var input = new HttpResponseMessage
            {
                Content = new ObjectContent(cipherContent.GetType(), cipherContent, new JsonMediaTypeFormatter())
            };

            var timestampProvider = MockRepository.GenerateStub<ITimestampProvider<long>>();
            using (var cryptoService = new DefaultCryptoService("secretKeyPassword", "initialVectorPassword", "hashKeyString"))
            using (var target = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider))
            {
                try
                {
                    target.Decrypt(input);
                    Assert.Fail("Did not throw expected exception InvalidHttpMessageException.");
                }
                catch (InvalidHttpMessageException ex)
                {
                    Assert.IsNotNull(ex.InnerException);
                    Assert.IsInstanceOfType(ex.InnerException, typeof(InvalidTimestampException));

                    var actualException = (InvalidTimestampException)ex.InnerException;
                    Assert.AreEqual("Timestamp should be numeric.", actualException.Timestamp);
                    Assert.IsInstanceOfType(actualException.InnerException, typeof(FormatException));
                }
            }
        }

        [TestMethod]
        public void DefaultHttpMessageCryptoServiceTest_DecryptResponse_WithoutCipherText()
        {
            var cipherContent = new
            {
                Timestamp = 12345,
                Signature = "c31553a7eaf726c50084791498a736305924db05f68640cfb5fc93c3e6482ca3c520d275f0118527fa1bace612738e67e035d7aeb91442ffac7c07c1c2cceaa3"
            };

            var input = new HttpResponseMessage
            {
                Content = new ObjectContent(cipherContent.GetType(), cipherContent, new JsonMediaTypeFormatter())
            };

            var timestampProvider = MockRepository.GenerateStub<ITimestampProvider<long>>();
            using (var cryptoService = new DefaultCryptoService("secretKeyPassword", "initialVectorPassword", "hashKeyString"))
            using (var target = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider))
            {
                try
                {
                    target.Decrypt(input);
                    Assert.Fail("Did not throw expected exception InvalidHttpMessageException.");
                }
                catch (InvalidHttpMessageException ex)
                {
                    Assert.IsNotNull(ex.InnerException);
                    Assert.IsInstanceOfType(ex.InnerException, typeof(CipherTextNullException));
                }
            }
        }

        [TestMethod]
        public void DefaultHttpMessageCryptoServiceTest_DecryptResponse_NullCipherText()
        {
            var cipherContent = new
            {
                Timestamp = 12345,
                CipherText = (object)null,
                Signature = "c31553a7eaf726c50084791498a736305924db05f68640cfb5fc93c3e6482ca3c520d275f0118527fa1bace612738e67e035d7aeb91442ffac7c07c1c2cceaa3"
            };

            var input = new HttpResponseMessage
            {
                Content = new ObjectContent(cipherContent.GetType(), cipherContent, new JsonMediaTypeFormatter())
            };

            var timestampProvider = MockRepository.GenerateStub<ITimestampProvider<long>>();
            using (var cryptoService = new DefaultCryptoService("secretKeyPassword", "initialVectorPassword", "hashKeyString"))
            using (var target = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider))
            {
                try
                {
                    target.Decrypt(input);
                    Assert.Fail("Did not throw expected exception InvalidHttpMessageException.");
                }
                catch (InvalidHttpMessageException ex)
                {
                    Assert.IsNotNull(ex.InnerException);
                    Assert.IsInstanceOfType(ex.InnerException, typeof(CipherTextNullException));
                }
            }
        }

        [TestMethod]
        public void DefaultHttpMessageCryptoServiceTest_DecryptResponse_EmptyCipherText()
        {
            var cipherContent = new
            {
                Timestamp = 12345,
                CipherText = string.Empty,
                Signature = "c31553a7eaf726c50084791498a736305924db05f68640cfb5fc93c3e6482ca3c520d275f0118527fa1bace612738e67e035d7aeb91442ffac7c07c1c2cceaa3"
            };

            var input = new HttpResponseMessage
            {
                Content = new ObjectContent(cipherContent.GetType(), cipherContent, new JsonMediaTypeFormatter())
            };

            var timestampProvider = MockRepository.GenerateStub<ITimestampProvider<long>>();
            using (var cryptoService = new DefaultCryptoService("secretKeyPassword", "initialVectorPassword", "hashKeyString"))
            using (var target = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider))
            {
                try
                {
                    target.Decrypt(input);
                    Assert.Fail("Did not throw expected exception InvalidHttpMessageException.");
                }
                catch (InvalidHttpMessageException ex)
                {
                    Assert.IsNotNull(ex.InnerException);
                    Assert.IsInstanceOfType(ex.InnerException, typeof(InvalidCipherTextException));

                    var actualException = (InvalidCipherTextException)ex.InnerException;
                    Assert.AreEqual(string.Empty, actualException.CipherText);
                }
            }
        }

        [TestMethod]
        public void DefaultHttpMessageCryptoServiceTest_DecryptResponse_InvalidCipherText()
        {
            var cipherContent = new
            {
                Timestamp = 12345,
                CipherText = "This is not a legal BASE64 string.",
                Signature = "c31553a7eaf726c50084791498a736305924db05f68640cfb5fc93c3e6482ca3c520d275f0118527fa1bace612738e67e035d7aeb91442ffac7c07c1c2cceaa3"
            };

            var input = new HttpResponseMessage
            {
                Content = new ObjectContent(cipherContent.GetType(), cipherContent, new JsonMediaTypeFormatter())
            };

            var timestampProvider = MockRepository.GenerateStub<ITimestampProvider<long>>();
            using (var cryptoService = new DefaultCryptoService("secretKeyPassword", "initialVectorPassword", "hashKeyString"))
            using (var target = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider))
            {
                try
                {
                    target.Decrypt(input);
                    Assert.Fail("Did not throw expected exception InvalidHttpMessageException.");
                }
                catch (InvalidHttpMessageException ex)
                {
                    Assert.IsNotNull(ex.InnerException);
                    Assert.IsInstanceOfType(ex.InnerException, typeof(InvalidCipherTextException));

                    var actualException = (InvalidCipherTextException)ex.InnerException;
                    Assert.AreEqual("This is not a legal BASE64 string.", actualException.CipherText);
                    Assert.IsInstanceOfType(actualException.InnerException, typeof(FormatException));
                }
            }
        }

        [TestMethod]
        public void DefaultHttpMessageCryptoServiceTest_DecryptResponse_WithoutSignature()
        {
            var cipherContent = new
            {
                Timestamp = 12345,
                CipherText = "zcbpurnokr/F/dzilj9hqFhMbtNR9jN9plz0TkNeFAPj38ryATycD38BW98AaHUJgHtPiUSzdO8shhkEuidjq/2NJ/n4WlJKK4lKO/kx5J8=",
            };

            var input = new HttpResponseMessage
            {
                Content = new ObjectContent(cipherContent.GetType(), cipherContent, new JsonMediaTypeFormatter())
            };

            var timestampProvider = MockRepository.GenerateStub<ITimestampProvider<long>>();
            using (var cryptoService = new DefaultCryptoService("secretKeyPassword", "initialVectorPassword", "hashKeyString"))
            using (var target = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider))
            {
                try
                {
                    target.Decrypt(input);
                    Assert.Fail("Did not throw expected exception InvalidHttpMessageException.");
                }
                catch (InvalidHttpMessageException ex)
                {
                    Assert.IsNotNull(ex.InnerException);
                    Assert.IsInstanceOfType(ex.InnerException, typeof(SignatureNullException));
                }
            }
        }

        [TestMethod]
        public void DefaultHttpMessageCryptoServiceTest_DecryptResponse_NullSignature()
        {
            var cipherContent = new
            {
                Timestamp = 12345,
                CipherText = "zcbpurnokr/F/dzilj9hqFhMbtNR9jN9plz0TkNeFAPj38ryATycD38BW98AaHUJgHtPiUSzdO8shhkEuidjq/2NJ/n4WlJKK4lKO/kx5J8=",
                Signature = (object)null
            };

            var input = new HttpResponseMessage
            {
                Content = new ObjectContent(cipherContent.GetType(), cipherContent, new JsonMediaTypeFormatter())
            };

            var timestampProvider = MockRepository.GenerateStub<ITimestampProvider<long>>();
            using (var cryptoService = new DefaultCryptoService("secretKeyPassword", "initialVectorPassword", "hashKeyString"))
            using (var target = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider))
            {
                try
                {
                    target.Decrypt(input);
                    Assert.Fail("Did not throw expected exception InvalidHttpMessageException.");
                }
                catch (InvalidHttpMessageException ex)
                {
                    Assert.IsNotNull(ex.InnerException);
                    Assert.IsInstanceOfType(ex.InnerException, typeof(SignatureNullException));
                }
            }
        }

        [TestMethod]
        public void DefaultHttpMessageCryptoServiceTest_DecryptResponse_EmptySignature()
        {
            var cipherContent = new
            {
                Timestamp = 12345,
                CipherText = "zcbpurnokr/F/dzilj9hqFhMbtNR9jN9plz0TkNeFAPj38ryATycD38BW98AaHUJgHtPiUSzdO8shhkEuidjq/2NJ/n4WlJKK4lKO/kx5J8=",
                Signature = string.Empty
            };

            var input = new HttpResponseMessage
            {
                Content = new ObjectContent(cipherContent.GetType(), cipherContent, new JsonMediaTypeFormatter())
            };

            var timestampProvider = MockRepository.GenerateStub<ITimestampProvider<long>>();
            using (var cryptoService = new DefaultCryptoService("secretKeyPassword", "initialVectorPassword", "hashKeyString"))
            using (var target = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider))
            {
                try
                {
                    target.Decrypt(input);
                    Assert.Fail("Did not throw expected exception InvalidHttpMessageException.");
                }
                catch (InvalidHttpMessageException ex)
                {
                    Assert.IsNotNull(ex.InnerException);
                    Assert.IsInstanceOfType(ex.InnerException, typeof(InvalidSignatureException));

                    var actualException = (InvalidSignatureException)ex.InnerException;
                    Assert.AreEqual(string.Empty, actualException.Signature);
                }
            }
        }

        [TestMethod]
        public void DefaultHttpMessageCryptoServiceTest_DecryptResponse_InvalidSignature()
        {
            var cipherContent = new
            {
                Timestamp = 12345,
                CipherText = "zcbpurnokr/F/dzilj9hqFhMbtNR9jN9plz0TkNeFAPj38ryATycD38BW98AaHUJgHtPiUSzdO8shhkEuidjq/2NJ/n4WlJKK4lKO/kx5J8=",
                Signature = "The length of signature should be 128."
            };

            var input = new HttpResponseMessage
            {
                Content = new ObjectContent(cipherContent.GetType(), cipherContent, new JsonMediaTypeFormatter())
            };

            var timestampProvider = MockRepository.GenerateStub<ITimestampProvider<long>>();
            using (var cryptoService = new DefaultCryptoService("secretKeyPassword", "initialVectorPassword", "hashKeyString"))
            using (var target = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider))
            {
                try
                {
                    target.Decrypt(input);
                    Assert.Fail("Did not throw expected exception InvalidHttpMessageException.");
                }
                catch (InvalidHttpMessageException ex)
                {
                    Assert.IsNotNull(ex.InnerException);
                    Assert.IsInstanceOfType(ex.InnerException, typeof(InvalidSignatureException));

                    var actualException = (InvalidSignatureException)ex.InnerException;
                    Assert.AreEqual("The length of signature should be 128.", actualException.Signature);
                }
            }
        }

        #endregion Expected error tests
    }
}