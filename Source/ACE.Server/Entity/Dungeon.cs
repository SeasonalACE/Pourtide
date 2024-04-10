using ACE.Server.WorldObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACE.Server.Entity
{
    public class DungeonLandblock
    {
        public string Landblock { get; set; }
        public string Name { get; set; }
        public string Coords { get; set; }

        public DungeonLandblock(string landblock, string name, string coords)
        {
            Landblock = landblock;
            Name = name;
            Coords = coords;
        }

    }

    public class DungeonBase : DungeonLandblock
    {
        public DungeonBase(string landblock, string name, string coords) : base(landblock, name, coords)
        {
        }

        public Dictionary<uint, Player> Players { get; set; } = new Dictionary<uint, Player>();
    }
}
