using System;

namespace ApiFoundation.Security.Cryptography
{
    /// <summary>
    /// 雜湊演算法
    /// </summary>
    public interface IHashAlgorithm : IDisposable
    {
        byte[] ComputeHash(byte[] source);
    }
}