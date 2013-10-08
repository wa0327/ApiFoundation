using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiFoundation.Security.Cryptography
{
    [TestClass]
    public class AESTest
    {
        [TestMethod]
        public void AESTest_Encrypt()
        {
            using (var target = new AES("123456789012345678901234", "12345678"))
            {
                var plainText = "`1234567890-=ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz~!@#$%^&*()_+";

                var plain = Encoding.UTF8.GetBytes(plainText);
                var cipher = target.Encrypt(plain);
                var cipherText = Convert.ToBase64String(cipher);

                Assert.AreEqual(
                    "xRANotL/km4vWpPwPw6P1H8T2UXptwMGyFShVH/BmXsnv6E+pOTtfeOPfsbfap5TD1DfMTdOn94t1j/sk9NU984ESJn1wi2QEDmgR/SI3B4=",
                    cipherText);
            }
        }

        [TestMethod]
        public void AESTest_Decrypt()
        {
            using (var target = new AES("123456789012345678901234", "12345678"))
            {
                var cipherText = "xRANotL/km4vWpPwPw6P1H8T2UXptwMGyFShVH/BmXsnv6E+pOTtfeOPfsbfap5TD1DfMTdOn94t1j/sk9NU984ESJn1wi2QEDmgR/SI3B4=";

                var cipher = Convert.FromBase64String(cipherText);
                byte[] plain = target.Decrypt(cipher);
                var plainText = Encoding.UTF8.GetString(plain);

                Assert.AreEqual(
                    "`1234567890-=ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz~!@#$%^&*()_+",
                    plainText);
            }
        }
    }
}