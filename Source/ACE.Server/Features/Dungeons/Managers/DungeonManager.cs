using ACE.Adapter.GDLE.Models;
using ACE.Common;
using ACE.Entity;
using ACE.Entity.Enum;
using ACE.Server.Entity;
using ACE.Server.Entity.Actions;
using ACE.Server.Factories;
using ACE.Server.Features.Discord;
using ACE.Server.Features.HotDungeons;
using ACE.Server.Features.Rifts;
using ACE.Server.Managers;
using ACE.Server.Network.GameMessages.Messages;
using ACE.Server.Physics.Common;
using ACE.Server.Realms;
using ACE.Server.WorldObjects;
using log4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACE.Server.Features.HotDungeons.Managers
{
    public class Dungeon : DungeonBase
    {
        public int TotalXpEarned { get; set; } = 0;

        public double BonuxXp { get; set; } = 1.0f;

        public uint PlayerTouches { get; set; } = 0;

        public uint Instance { get; set; } = 0;

        public InstancedPosition DropPosition { get; set; }

        public Dungeon(string landblock, string name, string coords, InstancedPosition drop) : base(landblock, name, coords)
        {
            Landblock = landblock;
            Name = name;
            Coords = coords;
            DropPosition = drop;
        }

        internal void AddTotalXp(int xpOverride)
        {
            TotalXpEarned += xpOverride;
        }

        internal void Close()
        {
            TotalXpEarned = 0;
            BonuxXp = 1.0f;
            PlayerTouches = 0;
            Players.Clear();
        }
    }


    public static class DungeonManager
    {
        private static object dungeonsLock = new object();

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static Dictionary<ushort, Dictionary<string, Dungeon>> HotspotDungeons = new Dictionary<ushort, Dictionary<string, Dungeon>>();

        private static Dictionary<ushort, Dictionary<string, Dungeon>> PotentialHotspotCandidates = new Dictionary<ushort, Dictionary<string, Dungeon>>();

        private static TimeSpan DungeonsInterval => TimeSpan.FromMinutes(PropertyManager.GetLong("rift_duration").Item);

        private static DateTime DungeonsLastCheck { get; set; } = DateTime.MinValue;

        public static TimeSpan DungeonsTimeRemaining => DungeonsLastCheck + DungeonsInterval - DateTime.UtcNow;

        public static void Initialize(uint intialDelay = 30)
        {
            DungeonRepository.Initialize();
            DungeonsLastCheck = DateTime.UtcNow - DungeonsInterval + TimeSpan.FromMinutes(intialDelay);
            log.Info($"Dungeons will be reset in {FormatTimeRemaining(DungeonsTimeRemaining)}");
        }

        public static int GetMaxHotSpots(ushort realmId)
        {
            var count = (uint)PlayerManager.GetOnlineCount(realmId);
            return count <= 10 ? 2 : count <= 20 ? 3 : 4;
        }

        public static bool HasHotspotDungeon(ushort realmId, string id)
        {
            if (HotspotDungeons.ContainsKey(realmId))
            {

                return HotspotDungeons[realmId].ContainsKey(id);
            }

            return false;
        }

        public static bool HasDungeon(string lb)
        {
            return DungeonRepository.HasDungeon(lb);
        }

        public static DungeonLandblock GetDungeonLandblock(string lb)
        {
            return DungeonRepository.ReadonlyLandblocks[lb];
        }

        public static bool HasDungeonLandblock(string lb)
        {
            return DungeonRepository.ReadonlyLandblocks.ContainsKey(lb);
        }


        public static bool TryGetDungeonLandblock(string lb, out DungeonLandblock landblock)
        {
            return DungeonRepository.ReadonlyLandblocks.TryGetValue(lb, out landblock);
        }

        public static void Reset(bool force = false)
        {
            lock (dungeonsLock)
            {
                if (!force && DungeonsTimeRemaining.TotalMilliseconds > 0)
                    return;

                DungeonsLastCheck = DateTime.UtcNow;

                CloseRealmDungeons();

                CreateRealmDungeons();
            }
        }

        private static void CloseRealmDungeons()
        {
            foreach (var realmDungeons in HotspotDungeons)
            {
                var dungeons = realmDungeons.Value.Values.ToList();

                foreach (var dungeon in dungeons)
                {
                    dungeon.Close();
                    var message = $"{dungeon.Name} is no longer boosted xp!";
                    _ = WebhookRepository.SendGeneralChat(message);
                    
                    log.Info(message);
                    PlayerManager.BroadcastToAll(new GameMessageSystemChat(message, ChatMessageType.WorldBroadcast));
                }

                realmDungeons.Value.Clear();
            }

            HotspotDungeons.Clear();

            RiftManager.Close();
        }

        private static void CreateRealmDungeons()
        {
            foreach(var realmDungeons in PotentialHotspotCandidates)
            {
                var sorted = realmDungeons.Value.Values
                    .OrderByDescending(d => d.TotalXpEarned)
                    .ThenByDescending(d => d.PlayerTouches)
                    .Take((int)GetMaxHotSpots(realmDungeons.Key))
                    .ToList();

                var dungeonsMap = new Dictionary<string, Dungeon>();

                HotspotDungeons.Add(realmDungeons.Key, dungeonsMap);

                foreach (var dungeon in sorted)
                {
                    if (RiftManager.TryAddRift(realmDungeons.Key, dungeon.Landblock, dungeon, out Rift activeRift))
                    {
                        dungeon.BonuxXp = activeRift.BonuxXp;
                        dungeon.Instance = activeRift.Instance;
                        dungeonsMap.Add(dungeon.Landblock, dungeon);

                        var message = $"{dungeon.Name} has been very active, a rift portal has been created in Subway (main hall, first room on the right), this dungeon has been boosted with {dungeon.BonuxXp.ToString("0.00")}x xp for {FormatTimeRemaining(DungeonsTimeRemaining)}";
                        _ = WebhookRepository.SendGeneralChat(message);
                        log.Info(message);
                        PlayerManager.BroadcastToAll(new GameMessageSystemChat(message, ChatMessageType.WorldBroadcast));
                    }
                }
            }

            PotentialHotspotCandidates.Clear();
        }

        public static List<(ushort, Dungeon)> GetPotentialDungeons()
        {
            return PotentialHotspotCandidates
               .SelectMany(outerPair => outerPair.Value.Values.Select(dungeon => (outerPair.Key, dungeon)))
               .ToList();
        }

        public static List<Dungeon> GetPotentialDungeons(ushort realmId)
        {
            if (PotentialHotspotCandidates.ContainsKey(realmId))
                return PotentialHotspotCandidates[realmId].Values.ToList();

            return new List<Dungeon>();
        }

        public static List<Dungeon> GetDungeons()
        {
            return HotspotDungeons.Values.SelectMany(kvp => kvp.Values).ToList();
        }

        public static List<Dungeon> GetDungeons(ushort realmId)
        {
            if (HotspotDungeons.ContainsKey(realmId))
                return HotspotDungeons[realmId].Values.ToList();

            return new List<Dungeon>();
        }

        public static void RemoveDungeonPlayer(string lb, Player player)
        {
            if (HotspotDungeons.ContainsKey(player.HomeRealm))
            {
                var guid = player.Guid.Full;

                if (HotspotDungeons[player.HomeRealm].TryGetValue(lb, out Dungeon currentDungeon))
                    if (currentDungeon.Players.ContainsKey(guid))
                        currentDungeon.Players.Remove(guid);
            }
        }

        public static void AddDungeonPlayer(string nextLb, Player player)
        {
            if (HotspotDungeons.ContainsKey(player.HomeRealm))
            {
                var guid = player.Guid.Full;

                if (HotspotDungeons[player.HomeRealm].TryGetValue(nextLb, out Dungeon nextDungeon))
                    if (!nextDungeon.Players.ContainsKey(guid))
                        nextDungeon.Players.TryAdd(guid, player);
            }
        }

        internal static void ProcessCreaturesDeath(string currentLb, Player damager, int xpOverride, out double returnValue)
        {
            returnValue = 1; // Default value

            var realmId = damager.HomeRealm;

            if (DungeonsTimeRemaining.TotalMilliseconds <= 0)
            {
                Reset();
                return;
            }

            if (HotspotDungeons.ContainsKey(realmId) && HotspotDungeons[realmId].ContainsKey(currentLb))
                return;
            else 
            {
                if (!PotentialHotspotCandidates.ContainsKey(realmId))
                    PotentialHotspotCandidates.Add(realmId, new Dictionary<string, Dungeon>());

                if (!PotentialHotspotCandidates[realmId].ContainsKey(currentLb))
                {
                    var dungeonLandblock = DungeonRepository.GetDungeon(currentLb);
                    if (dungeonLandblock != null)
                    {
                        var potentialDungeon = new Dungeon(dungeonLandblock.Landblock, dungeonLandblock.Name, dungeonLandblock.Coords, new InstancedPosition(damager.Location));
                        PotentialHotspotCandidates[realmId].TryAdd(currentLb, potentialDungeon);
                        potentialDungeon.AddTotalXp(xpOverride);
                        potentialDungeon.PlayerTouches++;
                    }
                }
                else
                {
                    var potentialDungeon = PotentialHotspotCandidates[realmId][currentLb];
                    if (potentialDungeon != null)
                    {
                        potentialDungeon.AddTotalXp(xpOverride);
                        potentialDungeon.PlayerTouches++;
                        potentialDungeon.DropPosition = new InstancedPosition(damager.Location);
                    }
                }
            }


        }

        public static string FormatTimeRemaining(TimeSpan timeRemaining)
        {
            if (timeRemaining.TotalSeconds < 1)
            {
                return "less than a second";
            }
            else if (timeRemaining.TotalMinutes < 1)
            {
                return $"{timeRemaining.Seconds} second{(timeRemaining.Seconds != 1 ? "s" : "")}";
            }
            else if (timeRemaining.TotalHours < 1)
            {
                return $"{timeRemaining.Minutes} minute{(timeRemaining.Minutes != 1 ? "s" : "")} and {timeRemaining.Seconds} second{(timeRemaining.Seconds != 1 ? "s" : "")}";
            }
            else
            {
                return $"{timeRemaining.Hours} hour{(timeRemaining.Hours != 1 ? "s" : "")}, {timeRemaining.Minutes} minute{(timeRemaining.Minutes != 1 ? "s" : "")}, and {timeRemaining.Seconds} second{(timeRemaining.Seconds != 1 ? "s" : "")}";
            }
        }
    }

}
