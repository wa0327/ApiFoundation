using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Http;

namespace ApiFoundation.Web.Http.Controllers
{
    public class TimestampController : ApiController
    {
        /// <summary>
        /// 提供timeStamp給client呼叫
        /// </summary>
        /// <returns>回傳timestamp字串,以明文格式回傳,不做加工處理</returns>
        [HttpGet]
        public string Get()
        {
            return DateTime.UtcNow.Ticks.ToString();
        }

        /// <summary>
        /// 為了與舊版相容所提供的方法。
        /// </summary>
        /// <returns></returns>
        [HttpGet, Obsolete]
        public string GetTimestamp()
        {
            return this.Get();
        }
    }
}