using ACE.Common;
using ACE.Server.WorldObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACE.Server.Factories.Tables
{
    public static class SalvageChance
    {
        private static readonly Dictionary<uint, (int, int)> MaterialRanges = new Dictionary<uint, (int, int)>()
        {
            {0, (4, 8) }, // Cloth
            {1, (10, 51) }, // Gems
            {2, (52, 55) }, // Hide
            {3, (57, 64) }, // Metal
            {4, (66, 71) }, // Stone
            {5, (73, 77) } // Wood
        };

        // 50% chance a random bag of salvage for each type will be returned
        public static List<int> Roll()
        {
            var values = Player.MaterialSalvageUseable.Values.ToList();
            var wcids = new List<int>();
            for (var i = 0; i < 2; i++)
            {
                var item = ThreadSafeRandom.Next(0, values.Count - 1);
                wcids.Add(values[item]);

            }

            return wcids;
        }


    }
}

