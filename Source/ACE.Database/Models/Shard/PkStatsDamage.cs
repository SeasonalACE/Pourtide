using ACE.Entity.Enum;
using System;

namespace ACE.Database.Models.Shard // Adjust namespace as per your project structure
{
    public class PKStatsDamage
    {
        public uint PKDamageId { get; set; }
        public uint AttackerId { get; set; }
        public uint DefenderId { get; set; }
        public int DamageAmount { get; set; }
        public ushort HomeRealmId { get; set; }
        public ushort CurrentRealmId { get; set; }

        public bool IsCrit { get; set; }

        public uint CombatMode { get; set; }


        public DateTime EventTime { get; set; }
    }
}
