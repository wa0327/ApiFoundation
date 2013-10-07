using System;

namespace ApiFoundation.Security.Cryptography
{
    /// <summary>
    /// 將對稱演算法
    /// </summary>
    public interface ISymmetricAlgorithm : IDisposable
    {
        byte[] Encrypt(byte[] plain);

        byte[] Decrypt(byte[] cipher);
    }
}