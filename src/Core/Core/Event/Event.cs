using System;

namespace QuantumCore.Core.Event
{
    public class Event
    {
        public long Id { get; }
        public Func<int> Callback { get; set; }
        public int Time { get; set; }

        public Event(long id, int timeout)
        {
            Id = id;
            Time = timeout;
        }
    }
}