using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Http;

namespace ApiFoundation.Web.Http.Controllers
{
    internal class TimestampController : ApiController
    {
        /// <summary>
        /// 提供timeStamp給client呼叫
        /// </summary>
        /// <returns>回傳timestamp字串,以明文格式回傳,不做加工處理</returns>
        [HttpGet]
        internal string GetTimeStamp()
        {
            return DateTime.UtcNow.Ticks.ToString();
        }
    }
}