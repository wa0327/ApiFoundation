using System;

namespace ApiFoundation.Security.Cryptography
{
    public class DefaultTimestampProvider : ITimestampProvider
    {
        private TimeSpan duration;

        public DefaultTimestampProvider()
        {
            this.duration = TimeSpan.FromMinutes(15);
        }

        public TimeSpan Duration
        {
            get { return this.duration; }
            set { this.duration = value; }
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
            catch (Exception ex)
            {
                throw new InvalidTimestampException(ex);
            }

            if (now.Ticks < target.Ticks || target >= expires)
            {
                throw new InvalidTimestampException();
            }
        }
    }
}