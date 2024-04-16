using ACE.Common;
using ACE.Database;
using ACE.Database.Models.Shard;
using ACE.Entity;
using ACE.Entity.Enum;
using ACE.Entity.Enum.Properties;
using ACE.Server.Factories;
using ACE.Server.HotDungeons.Managers;
using ACE.Server.Realms;
using ACE.Server.Rifts;
using ACE.Server.WorldObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACE.Server.Managers
{
    internal static class MutationsManager
    {
        public static uint GetMonsterTierByLevel(uint level)
        {
            uint tier = 0;

            if (level <= 300)
                tier = 6;
            if (level <= 220)
                tier = 5;
            if (level <= 150)
                tier = 4;
            if (level <= 115)
                tier = 3;
            if (level <= 100)
                tier = 2;
            if (level <= 50)
                tier = 1;
            if (level <= 20)
                tier = 0;

            return tier;
        }

        private static uint[] BlackListedLandblockIds =
        {
            0x8A02FFFF, // facility hub
        };

        private static List<ushort> BlackListedLandblocks = BlackListedLandblockIds.Select(r => new LandblockId(r).Landblock).ToList(); // Winthur Gate, Random Villas

        public static WorldObject ProcessWorldObject(WorldObject wo, AppliedRuleset ruleset, bool replace = true)
        {
            if (wo.WeenieClassId == 14)
            {
                wo.Destroy();
                return null;
            }

            var disableHousing = ruleset.Realm.Id != RealmManager.ServerBaseRealm.Realm.Id;

            if (wo is House || wo is Storage || wo is Hook || wo is Hooker || wo is HousePortal || wo is SlumLord)
            {
                if (disableHousing || !HouseManager.ValidatePourHousing(wo.Location.LandblockId.Landblock))

                {
                    wo = null;
                    return wo;
                }
            }

            if (wo.Location != null)
            {
                var lb = wo.Location.LandblockId.Landblock;
                if (BlackListedLandblocks.Contains(lb))
                {
                    wo = null;
                    return wo;
                }
            }


            if (ruleset.Realm.Id == 1016 && wo.WeenieType == WeenieType.Generic)
            {
                var creatureRespawnDuration = ruleset.GetProperty(RealmPropertyFloat.CreatureRespawnDuration);
                var creatureSpawnRateMultiplier = ruleset.GetProperty(RealmPropertyFloat.CreatureSpawnRateMultiplier);

                if (creatureRespawnDuration > 0)
                {
                    wo.RegenerationInterval = (int)((float)creatureRespawnDuration * creatureSpawnRateMultiplier);

                    wo.ReinitializeHeartbeats();

                    if (wo.Biota.PropertiesGenerator != null)
                    {
                        // While this may be ugly, it's done for performance reasons.
                        // Common weenie properties are not cloned into the bota on creation. Instead, the biota references simply point to the weenie collections.
                        // The problem here is that we want to update one of those common collection properties. If the biota is referencing the weenie collection,
                        // then we'll end up updating the global weenie (from the cache), instead of just this specific biota.
                        if (wo.Biota.PropertiesGenerator == wo.Weenie.PropertiesGenerator)
                        {
                            wo.Biota.PropertiesGenerator = new List<ACE.Entity.Models.PropertiesGenerator>(wo.Weenie.PropertiesGenerator.Count);

                            foreach (var record in wo.Weenie.PropertiesGenerator)
                                wo.Biota.PropertiesGenerator.Add(record.Clone());
                        }
                    }
                }

                return wo;
            }

            if (ruleset.Realm.Id == 1016 && wo.WeenieType == ACE.Entity.Enum.WeenieType.Creature && wo.Attackable && !wo.IsGenerator)
            {

                var lb = wo.Location.LandblockHex;

                if (RiftManager.TryGetActiveRift(lb, out Rift activeRift))
                    return ProcessRiftCreature(wo, activeRift);
            }

            return wo;
        }

        private static WorldObject RollForOre(Position position, uint tier = 1)
        {
            if (ThreadSafeRandom.Next(1, 100) == 1)
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
                ore.Location = new Position(position);
                return ore;
            }

            return null;
        }

        private static WorldObject ProcessRiftCreature(WorldObject wo, Rift rift)
        {
            var ore = RollForOre(wo.Location, rift.Tier);

            if (ore != null)
            {
                wo.Destroy();
                return ore;
            }

            if (ThreadSafeRandom.Next(1, 100) <= 25)
            {

                var riftCreatureId = rift.GetRandomCreature();
                var creature = WorldObjectFactory.CreateNewWorldObject(riftCreatureId);
                creature.Location = new Position(wo.Location);
                creature.Name = $"Rift {creature.Name}";
                wo.Destroy();
                return creature;
            }

            return wo;

        }
        
    }
}
