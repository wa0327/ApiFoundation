using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiFoundation.Security.Cryptography
{
    [TestClass]
    public class DefaultEncryptorTest
    {
        [TestMethod]
        public void DefaultEncryptor_Encrypt()
        {
            var timestamp = "201310041855";
            var plainText = "`1234567890-=ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz~!@#$%^&*()_+";
            var plain = Encoding.UTF8.GetBytes(plainText);

            var secretKey = this.CreateSecretKey();
            var initialVector = this.CreateInitialVector();
            var hashKey = this.CreateHashKey();

            IEncryptor target = new DefaultEncryptor(secretKey, initialVector, hashKey);

            byte[] cipher;
            string signature;
            target.Encrypt(plain, timestamp, out cipher, out signature);

            var cipherText = Convert.ToBase64String(cipher);

            Assert.AreEqual(
                "xRANotL/km4vWpPwPw6P1H8T2UXptwMGyFShVH/BmXsnv6E+pOTtfeOPfsbfap5TD1DfMTdOn94t1j/sk9NU984ESJn1wi2QEDmgR/SI3B4=",
                cipherText);
            Assert.AreEqual(
                "f24d46a550e50a3e309e3b6aa27f12d9316af9db264c1986ebd9259c62c906f6b1bc7e8b2e3b15974a9c34c13e511cd5e03d24670799026aeb884f8d23746111",
                signature);
        }

        [TestMethod]
        public void DefaultEncryptor_Decrypt()
        {
            var timestamp = "201310041855";
            var cipherText = "xRANotL/km4vWpPwPw6P1H8T2UXptwMGyFShVH/BmXsnv6E+pOTtfeOPfsbfap5TD1DfMTdOn94t1j/sk9NU984ESJn1wi2QEDmgR/SI3B4=";
            string signature = "f24d46a550e50a3e309e3b6aa27f12d9316af9db264c1986ebd9259c62c906f6b1bc7e8b2e3b15974a9c34c13e511cd5e03d24670799026aeb884f8d23746111";
            var cipher = Convert.FromBase64String(cipherText);

            var secretKey = this.CreateSecretKey();
            var initialVector = this.CreateInitialVector();
            var hashKey = this.CreateHashKey();

            IEncryptor target = new DefaultEncryptor(secretKey, initialVector, hashKey);

            byte[] plain;
            target.Decrypt(cipher, timestamp, signature, out plain);

            var plainText = Encoding.UTF8.GetString(plain);

            Assert.AreEqual(
                "`1234567890-=ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz~!@#$%^&*()_+",
                plainText);
        }

        private byte[] CreateSecretKey()
        {
            var password = "123456789012345678901234";

            byte[] salt = new byte[32];
            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, 32))
            {
                return deriveBytes.GetBytes(32);
            }
        }

        private byte[] CreateInitialVector()
        {
            var password = "12345678";

            byte[] salt = new byte[16];
            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, 16))
            {
                return deriveBytes.GetBytes(16);
            }
        }

        private byte[] CreateHashKey()
        {
            return Encoding.UTF8.GetBytes("1234567890");
        }
    }
}