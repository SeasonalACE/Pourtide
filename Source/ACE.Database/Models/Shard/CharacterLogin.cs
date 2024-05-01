using System;
using System.Collections.Generic;

namespace ACE.Database.Models.Shard
{
    public partial class CharacterLogin
    {
        public uint Id { get; set; }
        public uint AccountId { get; set; }
        public string AccountName { get; set; }
        public string SessionIP { get; set; }
        public ulong CharacterId { get; set; }
        public string CharacterName { get; set; }
        public DateTime LoginDateTime { get; set; }

        public DateTime? LogoutDateTime { get; set; }
    }
}

