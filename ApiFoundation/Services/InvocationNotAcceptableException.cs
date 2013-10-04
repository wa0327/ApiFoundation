using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiFoundation.Entities;

namespace ApiFoundation.Services
{
    /// <summary>
    /// 當被呼叫端發生商業邏輯錯誤時擲出。
    /// </summary>
    public sealed class InvocationNotAcceptableException : Exception
    {
        private readonly InvocationNotAcceptable source;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvocationNotAcceptableException" /> class.
        /// </summary>
        /// <param name="source">The source.</param>
        internal InvocationNotAcceptableException(InvocationNotAcceptable source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            this.source = source;
        }

        public override string Message
        {
            get { return this.source.ErrorMessage; }
        }

        /// <summary>
        /// Return code
        /// </summary>
        public string ReturnCode
        {
            get { return this.source.ReturnCode; }
        }
    }
}