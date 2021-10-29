using System.ComponentModel.DataAnnotations;

namespace Alirta.Models
{
    internal class MonitorAddress
    {
        [Key]
        public string Address { get; set; }

        public ulong LastUpdated { get; set; }
    }
}
