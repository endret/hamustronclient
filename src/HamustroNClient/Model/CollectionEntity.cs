using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamustroNClient.Model
{
    public class CollectionEntity : IEquatable<string>
    {
        public CollectionEntity(string collectionId)
        {
            this.Id = collectionId;
            this.Created = DateTime.UtcNow;
        }

        public string Id { get; private set; }

        public DateTime Created { get; private set; }

        public Collection Collection { get; set; }
        
        public bool Equals(string other)
        {
            return this.Id.ToLowerInvariant() == other.ToLowerInvariant();
        }
    }
}
