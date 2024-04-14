using System;

namespace ACE.Database.Models.Shard // Adjust namespace as per your project structure
{
    public class PKStatsDamage
    {
        public uint PKDamageId { get; set; }
        public uint AttackerId { get; set; }
        public uint DefenderId { get; set; }
        public int DamageAmount { get; set; }
        public DateTime EventTime { get; set; }
    }
}
