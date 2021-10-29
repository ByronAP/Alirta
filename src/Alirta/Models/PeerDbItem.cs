using System.ComponentModel.DataAnnotations;

namespace Alirta.Models
{
    internal class PeerDbItem
    {
        [Key]
        public string Host { get; set; }

        public uint Port { get; set; }
    }
}
