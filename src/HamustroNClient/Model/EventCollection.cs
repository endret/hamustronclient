using System;

namespace HamustroNClient.Model
{
    public class EventCollection
    {
        public string SessionId { get; set; }

        public CollectionEntity Collection { get; set; }

        public override bool Equals(object obj)
        {
            var ec = obj as EventCollection;

            if (ec == null) return false;

            return SessionId.Equals(ec.SessionId);
        }

        public override int GetHashCode()
        {
            if (SessionId == null) return 0;

            return SessionId.GetHashCode();
        }
    }
}
