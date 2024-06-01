using ACE.Common;
using ACE.Database;
using ACE.Database.Models.Shard;
using ACE.Database.Models.World;
using ACE.Entity;
using ACE.Entity.Enum;
using ACE.Entity.Enum.Properties;
using ACE.Server.Factories;
using ACE.Server.Features.Rifts;
using ACE.Server.Realms;
using ACE.Server.WorldObjects;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace ACE.Server.Managers
{
    internal static class MutationsManager
    {

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static uint GetMonsterTierByLevel(uint level)
        {
            uint tier = 0;

            if (level < 300)
                tier = 6;
            if (level < 220)
                tier = 5;
            if (level < 150)
                tier = 4;
            if (level < 115)
                tier = 3;
            if (level < 100)
                tier = 2;
            if (level < 50)
                tier = 1;
            if (level < 20)
                tier = 0;

            return tier;
        }

        public static uint GetLootTierFromRiftTier(uint tier)
        {
            uint lootTier = 4;

            if (tier == 6)
                lootTier = 8;
            if (tier == 5)
                lootTier = 7;
            if (tier == 4)
                lootTier = 5;

            return lootTier;
        }

        private static uint[] BlackListedLandblockIds =
        {
            0x8A02FFFF, // facility hub
        };

        private static List<ushort> BlackListedLandblocks = BlackListedLandblockIds.Select(r => new LandblockId(r).Landblock).ToList(); // Winthur Gate, Random Villas

        public static ACE.Entity.Models.Weenie ProcessLandblockInstance(ACE.Entity.Models.Weenie weenie, AppliedRuleset ruleset, LandblockInstance instance, uint iid)
        {
            if (weenie == null)
                return null;

            if (weenie.WeenieClassId > 607000 && weenie.WeenieClassId < 608000)
                return null;

            var location = new Position(instance.ObjCellId, instance.OriginX, instance.OriginY, instance.OriginZ, instance.AnglesX, instance.AnglesY, instance.AnglesZ, instance.AnglesW, iid);

            var lb = location.LandblockId.Landblock;
            if (BlackListedLandblocks.Contains(lb))
            {
                return null;
            }

            return weenie;
        }
        public static int CantripRoll()
        {
            var cantripAmount = 1;

            if (ThreadSafeRandom.Next(1, 500) == 1)
            {
                cantripAmount = 2;
                if (ThreadSafeRandom.Next(1, 10) == 1)
                    cantripAmount = 3;
            }

            return cantripAmount;
        }

        public static WorldObject CreateOre(InstancedPosition position, int oreDropChance, uint tier = 1)
        {
            if (ThreadSafeRandom.Next(1, oreDropChance) == 1)
            {
                var ore = WorldObjectFactory.CreateNewWorldObject(603001);

                if (tier >= 2 && ThreadSafeRandom.Next(1, 10) == 1)
                {
                    ore?.Destroy();
                    ore = WorldObjectFactory.CreateNewWorldObject((uint)603002);
                }

                if (tier >= 4 && ThreadSafeRandom.Next(1, 20) == 1)
                {
                    ore?.Destroy();
                    ore = WorldObjectFactory.CreateNewWorldObject((uint)603003);
                }
                ore.Location = new InstancedPosition(position);
                return ore;
            }

            return null;
        }

        public static WorldObject ProcessRiftCreature(WorldObject wo, int oreDropChance, Rift rift)
        {
            var ore = CreateOre(wo.Location, oreDropChance, rift.Tier);

            if (ore != null)
            {
                wo.Destroy();
                return ore;
            }

            var riftCreatureChance = PropertyManager.GetLong("rift_creature_chance").Item;

            if (ThreadSafeRandom.Next(1, (int)riftCreatureChance) == 1)
            {
                try
                {
                    var riftCreatureId = rift.GetRandomCreature();
                    if (riftCreatureId == 0)
                        return wo;

                    Creature creature = (Creature)WorldObjectFactory.CreateNewWorldObject((uint)riftCreatureId);

                    if (creature.IsGenerator)
                        return wo;

                    creature.Location = new InstancedPosition(wo.Location);
                    creature.Name = $"Rift {creature.Name}";
                    creature.IsRiftMonster = true;
                    wo.Destroy();
                    return creature;
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message);
                    log.Error(ex.StackTrace);

                    return wo;
                }
            }

            return wo;
        }
    }
}
