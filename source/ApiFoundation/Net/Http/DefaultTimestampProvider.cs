using System;

namespace ApiFoundation.Net.Http
{
    public class DefaultTimestampProvider : ITimestampProvider<long>
    {
        private TimeSpan duration;

        public DefaultTimestampProvider(TimeSpan duration)
        {
            if (duration == TimeSpan.Zero)
            {
                throw new ArgumentException("Parameter of duration cannot be Zero.", "duration");
            }

            this.duration = duration;
        }

        void IDisposable.Dispose()
        {
        }

        long ITimestampProvider<long>.GetTimestamp()
        {
            return DateTime.UtcNow.Ticks;
        }

        void ITimestampProvider<long>.Validate(long timestamp)
        {
            try
            {
                var now = DateTime.UtcNow;
                var expires = now.Add(this.duration);
                DateTime target;

                target = new DateTime(timestamp);
                if (now.Ticks > target.Ticks && target < expires)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidTimestampException(ex);
            }

            throw new InvalidTimestampException();
        }
    }
}