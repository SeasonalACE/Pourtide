using ACE.Common;
using ACE.Database;
using ACE.Entity;
using ACE.Entity.Enum;
using ACE.Entity.Models;
using ACE.Server.Entity;
using ACE.Server.Entity.Actions;
using ACE.Server.Factories;
using ACE.Server.Features.HotDungeons.Managers;
using ACE.Server.Managers;
using ACE.Server.Realms;
using ACE.Server.WorldObjects;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACE.Server.Features.Rifts
{
    internal static class RiftManager
    {
        private static readonly object lockObject = new object();

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static Dictionary<ushort, Dictionary<string, Rift>> ActiveRifts = new Dictionary<ushort, Dictionary<string, Rift>>();

        public static Dictionary<uint, Rift> ActiveRiftsByInstance = new Dictionary<uint, Rift>();

        public static Position RiftEntryPortal = InstancedPosition.slocToPosition("0x00070104 [70.125999 -169.860001 -5.995000] 0.999909 0.000000 0.000000 0.013459 393216");

        public static void Close()
        {
            lock (lockObject)
            {
                log.Info($"Rifts are currently resetting!");
                var rifts = ActiveRifts.Values.SelectMany(kvp => kvp.Values).ToList();
                foreach (var rift in rifts)
                    rift.Close();

                ActiveRifts.Clear();
                ActiveRiftsByInstance.Clear();
            }
        }

        public static void RemoveRiftPlayer(string lb, Player player)
        {
            var guid = player.Guid.Full;

            if (ActiveRifts.ContainsKey(player.HomeRealm))
            {
                if (ActiveRifts[player.HomeRealm].TryGetValue(lb, out Rift rift))
                {
                    if (rift.Players.ContainsKey(guid))
                    {
                        rift.Players.Remove(guid);
                        log.Info($"Removed {player.Name} from {rift.Name}");
                    }
                }
            }
        }

        public static void AddRiftPlayer(string nextLb, Player player)
        {
            var guid = player.Guid.Full;

            if (ActiveRifts.ContainsKey(player.HomeRealm))
            {
                if (ActiveRifts[player.HomeRealm].TryGetValue(nextLb, out Rift rift))
                {
                    if (!rift.Players.ContainsKey(guid))
                    {
                        rift.Players.TryAdd(guid, player);
                        log.Info($"Added {player.Name} to {rift.Name}");
                    }
                }
            }
        }

        public static bool HasActiveRift(ushort realmId, string lb)
        {
            if (ActiveRifts.ContainsKey(realmId))
                return ActiveRifts[realmId].ContainsKey(lb);

            return false;
        }

        public static List<Rift> GetRifts()
        {
            return ActiveRifts.Values.SelectMany(kvp => kvp.Values).ToList();
        }

        public static bool TryGetActiveRift(ushort realmId, string lb, out Rift activeRift)
        {
            if (ActiveRifts.ContainsKey(realmId))
            {
                if (ActiveRifts[realmId].TryGetValue(lb, out activeRift))
                {
                    return true;
                }
                else
                {
                    activeRift = null;
                    return false;
                }
            }

            activeRift = null;
            return false;
        }

        public static bool TryGetActiveRift(uint instance, out Rift activeRift)
        {
            if (ActiveRiftsByInstance.ContainsKey(instance))
            {
                activeRift = ActiveRiftsByInstance[instance];
                return true;
            }

            activeRift = null;
            return false;
        }

        public static List<WorldObject> GetDungeonObjectsFromPosition(InstancedPosition position)
        {
            var Id = new LandblockId(position.LandblockId.Raw);

            var objects = DatabaseManager.World.GetCachedInstancesByLandblock(Id.Landblock);
            return objects.Select(link => WorldObjectFactory.CreateNewWorldObject(link.WeenieClassId)).ToList();
        }

        private static List<Creature> GetGeneratorCreaturesObjectsFromDungeon(List<WorldObject> dungeonObjects)
        {
            var objects = dungeonObjects
                .Where(wo => wo.IsGenerator)
                .SelectMany(wo => wo.Biota.PropertiesGenerator.Select(prop => prop.WeenieClassId))
                .Select(wcid => WorldObjectFactory.CreateNewWorldObject(wcid))
                .OfType<Creature>()
                .Where(creature => creature is not Player && !creature.IsGenerator && !creature.IsNPC && creature.DeathTreasure != null)
                .ToList();

            return objects;
        }

        public static (uint, List<uint>) GetRiftCreatureIds(InstancedPosition dropPosition)
        {
            var dungeonObjects = GetDungeonObjectsFromPosition(dropPosition);

            var generatorCreatureObjects = GetGeneratorCreaturesObjectsFromDungeon(dungeonObjects);

            var spawnedCreatures = dungeonObjects
                .OfType<Creature>()
                .Where(creature => creature is not Player && !creature.IsGenerator && !creature.IsNPC && creature.DeathTreasure != null);

            var creatures = generatorCreatureObjects.Concat(spawnedCreatures).Distinct().ToList();

            var groupedCreaturesByLootTier = creatures.GroupBy(c => c.DeathTreasure.Tier).Select(g => new { Tier = g.Key, Count = g.Count() });
            var groupedCreaturesByLevel = creatures.GroupBy(c => c.Level).Select(g => new { Level = g.Key, Count = g.Count() });

            var mostCommonLoot = groupedCreaturesByLootTier.OrderByDescending(l => l.Count).Select(l => l.Tier).FirstOrDefault();
            var mostCommonLevel = groupedCreaturesByLevel.OrderByDescending(l => l.Count).Select(l => l.Level).FirstOrDefault();

            if (mostCommonLoot == 0)
                mostCommonLoot = 1;

            if (mostCommonLevel == null || mostCommonLevel == 0)
                mostCommonLevel = 1;

            var tier = MutationsManager.GetMonsterTierByLevel((uint)mostCommonLevel);

            var creatureWeenieIds = DatabaseManager.World.GetDungeonCreatureWeenieIds(mostCommonLoot);

            var creatureIds = creatureWeenieIds
                .Where(c => c.Level >= mostCommonLevel && c.Level <= mostCommonLevel + 20)
                .Select(c => c.Id)
                .ToList();

            creatures.ForEach(c => c.Destroy()); ;
            creatures.Clear();

            return (tier, creatureIds);
        }

        public static Rift CreateRiftInstance(ushort realmId, Dungeon dungeon)
        {
            var rules = new List<Realm>()
            {
                RealmManager.GetRealm(1016).Realm // rift ruleset
            };
            var ephemeralRealm = RealmManager.GetNewEphemeralLandblock(realmId, dungeon.DropPosition.LandblockId, rules, true);

            var instance = ephemeralRealm.Instance;

            var dropPosition = new InstancedPosition(dungeon.DropPosition, instance);

            var (tier, creatureIds) = GetRiftCreatureIds(dropPosition);

            var rift = new Rift(dungeon.Landblock, dungeon.Name, dungeon.Coords, dropPosition, instance, dungeon.DropPosition.Instance, ephemeralRealm, creatureIds, tier);

            log.Info($"Creating Rift instance for {rift.Name} - {instance}");

            return rift;
        }

        private static List<WorldObject> FindRandomCreatures(Rift rift)
        {
            var lb = rift.LandblockInstance;

            var worldObjects = lb.GetAllWorldObjectsForDiagnostics();

            var filteredWorldObjects = worldObjects
                .Where(wo => wo is Creature creature && !(creature is Player) && !wo.IsGenerator)
                .OrderBy(creature => creature, new DistanceComparer(rift.DropPosition))
                .ToList(); 

            return filteredWorldObjects;
        }

        private class DistanceComparer : IComparer<WorldObject>
        {
            private InstancedPosition Location;
            public DistanceComparer(InstancedPosition location)
            {
                Location = location;
            }
            public int Compare(WorldObject x, WorldObject y)
            {
                return (int)(x.Location.DistanceTo(Location) - y.Location.DistanceTo(Location));
            }
        }

        internal static bool TryAddRift(ushort realmId, string currentLb, Dungeon dungeon, out Rift addedRift)
        {
            addedRift = null;

            if (!ActiveRifts.ContainsKey(realmId))
                ActiveRifts.TryAdd(realmId, new Dictionary<string, Rift>());


            if (ActiveRifts[realmId].ContainsKey(currentLb))
                return false;

            var rift = CreateRiftInstance(realmId, dungeon);

            SpawnHomeToRiftPortalAsync(rift);
            SpawnRiftToHomePortalAsync(rift);

            var rifts = ActiveRifts[realmId].Values.ToList();

            var last = rifts.LastOrDefault();

            if (last != null)
            {
                rift.Previous = last;
                last.Next = rift;

                SpawnNextLinkAsync(last);
                SpawnPreviousLinkAsync(rift);
            }

            ActiveRifts[realmId][currentLb] = rift;
            ActiveRiftsByInstance[rift.Instance] = rift;

            addedRift = rift;

            var at = rift.Coords.Length > 0 ? $"at {rift.Coords}" : "";
            var message = $"Dungeon {rift.Name} {at} is now an activated Rift";
            log.Info(message);

            return true;
        }

        internal static void SpawnRiftToHomePortalAsync(Rift rift)
        {
            if (rift.DropPosition == null)
                return;

            var landblock = rift.LandblockInstance;
            var chain = new ActionChain();
            chain.AddDelaySeconds(5);

            chain.AddAction(landblock, () =>
            {

                if (!landblock.CreateWorldObjectsCompleted)
                {
                    SpawnRiftToHomePortalAsync(rift);
                    return;
                }

                List<WorldObject> creatures;

                creatures = FindRandomCreatures(rift);

                foreach (var wo in creatures.Skip(3).ToList())
                {
                    Portal portal = (Portal)WorldObjectFactory.CreateNewWorldObject(600004);
                    portal.Name = $"Dungeon Portal {rift.Name}";
                    portal.Location = new InstancedPosition(wo.Location);
                    portal.Destination = new InstancedPosition(wo.Location, wo.Location.Instance).AsLocalPosition();
                    portal.Lifespan = int.MaxValue;

                    var name = "Portal to " + rift.Name;
                    portal.SetProperty(ACE.Entity.Enum.Properties.PropertyString.AppraisalPortalDestination, name);
                    portal.ObjScale *= 0.25f;

                    log.Info($"Attempting to link dungeon in rift {rift.Name}");
                    if (portal.EnterWorld())
                    {
                        log.Info($"Added dungeon portal for rift {rift.Name}");
                        return;
                    }
                }
            });
            chain.EnqueueChain();
        }


        public static void SpawnHomeToRiftPortalAsync(Rift rift)
        {
            if (rift.DropPosition == null)
                return;

            var landblock = LandblockManager.GetLandblock(rift.LandblockInstance.Id, rift.HomeInstance, null, false);
            var chain = new ActionChain();
            chain.AddDelaySeconds(5);

            chain.AddAction(landblock, () =>
            {

                var toRiftDrop = new InstancedPosition(rift.DropPosition, rift.HomeInstance);

                Portal portal = (Portal)WorldObjectFactory.CreateNewWorldObject(600004);
                portal.Name = $"Rift Portal {rift.Name}";
                portal.Location = new InstancedPosition(toRiftDrop);

                var dest = new InstancedPosition(rift.DropPosition, rift.Instance);

                portal.Destination = new InstancedPosition(dest).AsLocalPosition();
                portal.IsEphemeralRealmPortal = true;
                portal.EphemeralRealmPortalInstanceID = rift.Instance;
                portal.Lifespan = int.MaxValue;

                var name = "Portal to " + rift.Name;
                portal.SetProperty(ACE.Entity.Enum.Properties.PropertyString.AppraisalPortalDestination, name);
                portal.ObjScale *= 0.25f;

                rift.RiftPortals.Add(portal);

                portal.EnterWorld();
            });
            chain.EnqueueChain();
        }

        internal static void SpawnPreviousLinkAsync(Rift rift)
        {
            if (rift.DropPosition == null)
                return;

            var landblock = rift.LandblockInstance;
            var chain = new ActionChain();
            chain.AddDelaySeconds(5);

            chain.AddAction(landblock, () =>
            {

                if (!landblock.CreateWorldObjectsCompleted)
                {
                    SpawnPreviousLinkAsync(rift);
                    return;
                }

                List<WorldObject> creatures;

                creatures = FindRandomCreatures(rift);

                if (rift.Previous != null)
                {
                    log.Info($"Creatures Count {creatures.Count} in {rift.Name}");

                    foreach (var wo in creatures)
                    {
                        Portal portal = (Portal)WorldObjectFactory.CreateNewWorldObject(600004);
                        portal.Name = $"Rift Portal {rift.Previous.Name}";
                        portal.Location = new InstancedPosition(wo.Location);
                        portal.Destination = rift.Previous.DropPosition.AsLocalPosition();
                        portal.IsEphemeralRealmPortal = true;
                        portal.EphemeralRealmPortalInstanceID = rift.Previous.Instance;
                        portal.Lifespan = int.MaxValue;

                        var name = "Portal to " + rift.Previous.Name;
                        portal.SetProperty(ACE.Entity.Enum.Properties.PropertyString.AppraisalPortalDestination, name);
                        portal.ObjScale *= 0.25f;

                        log.Info($"Attempting to link Portal for previous in {rift.Name}");
                        if (portal.EnterWorld())
                        {
                            log.Info($"Added Linked Portal for previous in {rift.Name}");
                            return;
                        }
                    }
                }
            });
            chain.EnqueueChain();
        }

        internal static void SpawnNextLinkAsync(Rift rift)
        {
            if (rift.DropPosition == null)
                return;

            var landblock = rift.LandblockInstance;
            var chain = new ActionChain();
            chain.AddDelaySeconds(5);

            chain.AddAction(landblock, () =>
            {
                if (!landblock.CreateWorldObjectsCompleted)
                {
                    SpawnNextLinkAsync(rift);
                    return;
                }

                List<WorldObject> creatures;

                creatures = FindRandomCreatures(rift);

                if (rift.Next != null)
                {
                    log.Info($"Creatures Count {creatures.Count} in {rift.Name}");
                    foreach (var wo in creatures)
                    {
                        Portal portal = (Portal)WorldObjectFactory.CreateNewWorldObject(600004);
                        portal.Name = $"Rift Portal {rift.Next.Name}";
                        portal.Location = new InstancedPosition(wo.Location);
                        portal.Destination = rift.Next.DropPosition.AsLocalPosition();
                        portal.IsEphemeralRealmPortal = true;
                        portal.EphemeralRealmPortalInstanceID = rift.Next.Instance;
                        portal.Lifespan = int.MaxValue;

                        var name = "Portal to " + rift.Next.Name;
                        portal.SetProperty(ACE.Entity.Enum.Properties.PropertyString.AppraisalPortalDestination, name);
                        portal.ObjScale *= 0.25f;

                        log.Info($"Attempting to link Portal for next in {rift.Name}");
                        if (portal.EnterWorld())
                        {
                            log.Info($"Added Linked Portal for next in {rift.Name}");
                            return;
                        }
                    }
                }
            });

            chain.EnqueueChain();
        }
    }
}
