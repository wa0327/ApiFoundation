using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ApiFoundation.Services
{
    /// <summary>
    /// Log writer
    /// </summary>
    public interface ILogWriter
    {
        /// <summary>
        /// 錯誤訊息寫入
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        void Error(string format, params object[] args);

        /// <summary>
        /// 警告訊息寫入
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        void Warning(string format, params object[] args);

        /// <summary>
        /// 告知性訊息寫入
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        void Information(string format, params object[] args);
    }
}