using ACE.Database;
using ACE.Entity;
using ACE.Entity.Enum;
using ACE.Entity.Models;
using ACE.Server.Entity;
using ACE.Server.Entity.Actions;
using ACE.Server.Factories;
using ACE.Server.HotDungeons.Managers;
using ACE.Server.Managers;
using ACE.Server.Network;
using ACE.Server.Network.GameMessages.Messages;
using ACE.Server.Realms;
using ACE.Server.WorldObjects;
using log4net;
using Microsoft.Cci.Pdb;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACE.Server.Rifts
{
    internal class Rift : DungeonBase
    {
        public Position DropPosition = null;
        public double BonuxXp
        {
            get
            {
                var rules = LandblockInstance?.RealmRuleset;
                if (rules != null)
                    return rules.GetProperty(ACE.Entity.Enum.Properties.RealmPropertyFloat.ExperienceMultiplierAll);
                else
                    return 1.0;
            }
        } 

        public Rift Next = null;

        public Rift Previous = null;

        public Landblock? LandblockInstance = null;

        public Player Creator;

        public uint Instance { get; set; } = 0;

        public Rift(string landblock, string name, string coords, Position dropPosition, uint instance, Landblock ephemeralRealm) : base(landblock, name, coords)
        {
            Landblock = landblock;
            Name = name;
            Coords = coords;
            DropPosition = dropPosition;
            Instance = instance;
            LandblockInstance = ephemeralRealm;
        }

        public void Close()
        {
            foreach (var player in Players.Values)
            {
                if (player != null)
                {
                    player.ExitInstance();
                }
            }

            Instance = 0;
            DropPosition = null;
            Next = null;
            Previous = null;
            LandblockInstance.Permaload = false;
            LandblockInstance = null;
            Players.Clear();
        }
    }

    internal static class RiftManager
    {
        private static readonly object lockObject = new object();

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static Dictionary<string, Rift> ActiveRifts = new Dictionary<string, Rift>();

        public static void Close()
        {
            lock (lockObject)
            {
                var message = $"Rifts are currently resetting!";
                log.Info(message);
                foreach (var rift in ActiveRifts)
                    rift.Value.Close();

                var landblocks = LandblockManager.GetLoadedLandblocks();

                var ephemeralRealms = landblocks.Where(lb => lb.InnerRealmInfo != null).ToList();

                foreach (var realm in ephemeralRealms)
                {
                    var objects = realm.GetAllWorldObjectsForDiagnostics();
                    var players = objects.Where(o => o is Player).ToList();

                    foreach (var player in players)
                    {
                        if (player is Player pl)
                            pl.ExitInstance();
                    }
                }

                ActiveRifts.Clear();
            }
        }

        public static void RemoveRiftPlayer(string lb, Player player)
        {
            var guid = player.Guid.Full;

            if (ActiveRifts.TryGetValue(lb, out Rift rift))
            {
                if (rift.Players.ContainsKey(guid))
                {
                    rift.Players.Remove(guid);
                    log.Info($"Removed {player.Name} from {rift.Name}");
                }
            }
        }

        public static void AddRiftPlayer(string nextLb, Player player)
        {
            var guid = player.Guid.Full;

            if (ActiveRifts.TryGetValue(nextLb, out Rift rift))
            {
                if (!rift.Players.ContainsKey(guid))
                {
                    rift.Players.TryAdd(guid, player);
                    log.Info($"Added {player.Name} to {rift.Name}");
                }
            }
        }

        public static bool HasActiveRift(string lb)
        {
            return ActiveRifts.ContainsKey(lb);
        }

        public static bool TryGetActiveRift(string lb, out Rift activeRift)
        {
            if (ActiveRifts.TryGetValue(lb, out activeRift))
            {
                return true;
            }
            else
            {
                activeRift = null;
                return false;
            }
        }

        public static Rift CreateRiftInstance(Dungeon dungeon)
        {
            var rules = new List<Realm>()
            {
                RealmManager.GetRealm(1016).Realm // rift ruleset
            };
            var ephemeralRealm = RealmManager.GetNewEphemeralLandblock(dungeon.DropPosition.LandblockId, rules, true);
            var instance = ephemeralRealm.Instance;

            var dropPosition = new Position(dungeon.DropPosition)
            {
                Instance = instance,
            };

            var rift = new Rift(dungeon.Landblock, dungeon.Name, dungeon.Coords, dropPosition, instance, ephemeralRealm);

            log.Info($"Creating Rift instance for {rift.Name} - {instance}");

            return rift;
        }

        private static List<WorldObject> FindRandomCreatures(Rift rift)
        {
            var lb = rift.LandblockInstance;

            var worldObjects = lb.GetAllWorldObjectsForDiagnostics();

            var portal = worldObjects.Where(wo => wo is Portal).FirstOrDefault();

            var filteredWorldObjects = worldObjects
                .Where(wo => wo is Creature && !(wo is Player) && !wo.IsGenerator)
                .OrderBy(creature => creature, new DistanceComparer(rift.DropPosition))
                .ToList(); // To prevent multiple enumeration

            return filteredWorldObjects;
        }

        private class DistanceComparer : IComparer<WorldObject>
        {
            private Position Location;
            public DistanceComparer(Position location)
            {
                Location = location;
            }
            public int Compare(WorldObject x, WorldObject y)
            {
                return (int)(x.Location.DistanceTo(Location) - y.Location.DistanceTo(Location));
            }
        }

        internal static bool TryAddRift(string currentLb, Dungeon dungeon, out Rift addedRift)
        {
            addedRift = null;


            if (ActiveRifts.ContainsKey(currentLb))
                return false;

            var rift = CreateRiftInstance(dungeon);
            var rifts = ActiveRifts.Values.ToList();

            var last = rifts.LastOrDefault();

            if (last != null)
            {
                rift.Previous = last;
                last.Next = rift;

                 SpawnNextAsync(last);
                 SpawnPreviousAsync(rift);
            }

            ActiveRifts[currentLb] = rift;

            addedRift = rift;

            var at = rift.Coords.Length > 0 ? $"at {rift.Coords}" : "";
            var message = $"Dungeon {rift.Name} {at} is now an activated Rift";
            log.Info(message);

            return true;
        }


        internal static void SpawnPreviousAsync(Rift rift)
        {
            var landblock = rift.LandblockInstance;
            var chain = new ActionChain();
            chain.AddDelaySeconds(5);

            chain.AddAction(landblock, () =>
            {

                if (!landblock.CreateWorldObjectsCompleted)
                {
                    SpawnPreviousAsync(rift);
                    return;
                }

                List<WorldObject> creatures;

                creatures = FindRandomCreatures(rift);

                if (rift.Previous != null)
                {
                    log.Info($"Creatures Count {creatures.Count} in {rift.Name}");

                    foreach (var wo in creatures)
                    {
                        var portal = WorldObjectFactory.CreateNewWorldObject(600004);
                        portal.Name = $"Rift Portal {rift.Previous.Name}";
                        portal.Location = new Position(wo.Location);
                        portal.Destination = rift.Previous.DropPosition;
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

        internal static void SpawnNextAsync(Rift rift)
        {
            var landblock = rift.LandblockInstance;
            var chain = new ActionChain();
            chain.AddDelaySeconds(5);

            chain.AddAction(landblock, () =>
            {
                if (!landblock.CreateWorldObjectsCompleted)
                {
                    SpawnNextAsync(rift);
                    return;
                }

                List<WorldObject> creatures;

                creatures = FindRandomCreatures(rift);

                if (rift.Next != null)
                {
                    log.Info($"Creatures Count {creatures.Count} in {rift.Name}");
                    foreach (var wo in creatures)
                    {
                        var portal = WorldObjectFactory.CreateNewWorldObject(600004);
                        portal.Name = $"Rift Portal {rift.Next.Name}";
                        portal.Location = new Position(wo.Location);
                        portal.Destination = rift.Next.DropPosition;
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
