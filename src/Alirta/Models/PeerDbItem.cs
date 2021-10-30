using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Alirta.Models
{
    [Index(nameof(Host), nameof(Port), IsUnique = true)]
    internal class PeerDbItem
    {
        [Key]
        public uint Id { get; set; }

        public string Host { get; set; }

        public uint Port { get; set; }
    }
}
