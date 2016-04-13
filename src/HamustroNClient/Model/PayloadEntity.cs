using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamustroNClient.Model
{
    public class PayloadEntity
    {
        public ulong At { get; set; }
        public string Event { get; set; }
        public uint Nr { get; set; }
        public uint UserId { get; set; }
        public string Ip { get; set; }
        public string Parameters { get; set; }
        public bool IsTesting { get; set; }
    }
}
