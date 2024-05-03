using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACE.Database.Models.Shard
{
    public class PkTrophyCooldown
    {
        public uint TrophyCooldownId { get; set; }
        public ulong KillerId { get; set; }
        public ulong VictimId { get; set; }
        public DateTime CooldownEndTime { get; set; }
    }
}
