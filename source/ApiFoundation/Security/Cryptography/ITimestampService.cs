using System;

namespace ApiFoundation.Security.Cryptography
{
    public interface ITimestampService
    {
        void GetTimestamp(out string timestamp, out DateTime expires);

        /// <summary>
        /// Validates the specified timestamp.
        /// </summary>
        /// <param name="timestamp">The timestamp.</param>
        /// <exception cref="ApiFoundation.Net.Http.InvalidTimestampException">當時戳無法辨識或時戳過期時擲出。</exception>
        void Validate(string timestamp);
    }
}