using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiFoundation.Entities;

namespace ApiFoundation.Services
{
    /// <summary>
    /// 當呼叫端送出的格式有問題時擲出。
    /// </summary>
    public sealed class BadInvocationException : Exception
    {
        internal BadInvocationException(Dictionary<string, IEnumerable<string>> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
        }
    }
}