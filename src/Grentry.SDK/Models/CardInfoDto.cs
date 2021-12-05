using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grentry.SDK.Models
{
    public class CardInfoDto
    {
        public byte[] UidRaw { get; set; }
        public string Uid { get; set; }
        public string UidHex { get; set; }
        public string UidReversed { get; set; }
        public string UidHexReversed { get; set; }
    }
}
