using System;

namespace ApiFoundation.Security.Cryptography
{
    public class DefaultTimestampService : ITimestampService
    {
        private readonly TimeSpan duration;

        public DefaultTimestampService(TimeSpan duration)
        {
            this.duration = duration;
        }

        public virtual void GetTimestamp(out string timestamp, out DateTime expires)
        {
            timestamp = DateTime.UtcNow.Ticks.ToString("d");
            expires = DateTime.UtcNow.Add(this.duration);
        }

        public virtual void Validate(string timestamp)
        {
            var now = DateTime.UtcNow;
            var expires = now.Add(this.duration);
            DateTime target;

            try
            {
                target = new DateTime(long.Parse(timestamp));
            }
            catch
            {
                throw new InvalidTimestampException();
            }

            if (now.Ticks < target.Ticks || target >= expires)
            {
                throw new InvalidTimestampException();
            }
        }
    }
}