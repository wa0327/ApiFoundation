using System;

namespace ApiFoundation.Net.Http
{
    public interface ITimestampProvider
    {
        void GetTimestamp(out string timestamp, out DateTime expires);
    }
}