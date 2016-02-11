using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamustroNClient.Model
{
    public class CollectionEntity
    {
        public CollectionEntity()
        {
            this.Id = new Guid();
        }

        public Guid Id { get; private set; }

        public Collection Collection { get; set; }
    }
}
