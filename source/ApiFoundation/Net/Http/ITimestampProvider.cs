using System;

namespace ApiFoundation.Net.Http
{
    public interface ITimestampProvider<T> : IDisposable
    {
        /// <summary>
        /// 取得時戳。
        /// </summary>
        /// <returns>時戳，不限定是何種型別資料，由實作者自行決定。</returns>
        T GetTimestamp();

        /// <summary>
        /// Validates the specified timestamp.
        /// </summary>
        /// <param name="timestamp">The timestamp.</param>
        /// <exception cref="ApiFoundation.Net.Http.InvalidTimestampException">當時戳無法辨識或時戳過期時擲出。</exception>
        void Validate(T timestamp);
    }
}