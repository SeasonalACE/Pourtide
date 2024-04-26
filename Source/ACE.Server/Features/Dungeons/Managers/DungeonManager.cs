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

        public ACE.Entity.Position DropPosition { get; set; }

        public Dungeon(string landblock, string name, string coords, ACE.Entity.Position drop) : base(landblock, name, coords)
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

        private static Dictionary<string, Dungeon> HotspotDungeons = new Dictionary<string, Dungeon>();

        private static Dictionary<string, Dungeon> PotentialHotspotCandidates = new Dictionary<string, Dungeon>();

        private static float MaxBonuxXp = 4.0f;

        private static uint MaxHotspots => (uint)GetMaxHotSpots();

        private static TimeSpan DungeonsInterval { get; set; }

        private static DateTime DungeonsLastCheck { get; set; } = DateTime.MinValue;

        public static TimeSpan DungeonsTimeRemaining => DungeonsLastCheck + DungeonsInterval - DateTime.UtcNow;

        public static void Initialize(uint intialDelay = 30, float maxBonuxXp = 4.0f)
        {
            DungeonRepository.Initialize();
            var duration = PropertyManager.GetLong("rift_duration").Item;
            DungeonsInterval = TimeSpan.FromMinutes(duration);
            MaxBonuxXp = maxBonuxXp;
            DungeonsLastCheck = DateTime.UtcNow - DungeonsInterval + TimeSpan.FromMinutes(intialDelay);
            log.Info($"Dungeons will be reset in {FormatTimeRemaining(DungeonsTimeRemaining)}");
        }

        public static int GetMaxHotSpots()
        {
            var count = (uint)PlayerManager.GetOnlineCount();
            return count <= 25 ? 2 : count <= 40 ? 3 : 4;
        }

        public static bool HasHotspotDungeon(string id)
        {
            return HotspotDungeons.ContainsKey(id);
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

                RiftManager.Close();

                foreach (var dungeon in HotspotDungeons.Values.ToList())
                {
                    dungeon.Close();
                    var message = $"{dungeon.Name} is no longer boosted xp!";
                    _ = WebhookRepository.SendGeneralChat(message);
                    
                    log.Info(message);
                    PlayerManager.BroadcastToAll(new GameMessageSystemChat(message, ChatMessageType.WorldBroadcast));
                }

                HotspotDungeons.Clear();

                var sorted = PotentialHotspotCandidates.Values
                    .OrderByDescending(d => d.TotalXpEarned)
                    .ThenByDescending(d => d.PlayerTouches)
                    .Take((int)MaxHotspots)
                    .ToList();

                PotentialHotspotCandidates.Clear();

                foreach (var dungeon in sorted)
                {
                    RiftManager.TryAddRift(dungeon.Landblock, dungeon, out Rift activeRift);
                    dungeon.BonuxXp = activeRift.BonuxXp;
                    dungeon.Instance = activeRift.Instance;
                    HotspotDungeons.Add(dungeon.Landblock, dungeon);

                    var message = $"{dungeon.Name} has been very active, a rift has been created in Town Network (Annex side), this dungeon has been boosted with {dungeon.BonuxXp.ToString("0.00")}x xp for {FormatTimeRemaining(DungeonsTimeRemaining)}";
                    _ = WebhookRepository.SendGeneralChat(message);
                    log.Info(message);
                    PlayerManager.BroadcastToAll(new GameMessageSystemChat(message, ChatMessageType.WorldBroadcast));
                }


            }

        }
        public static List<Dungeon> GetPotentialDungeons()
        {
            return PotentialHotspotCandidates.Values.ToList();
        }


        public static List<Dungeon> GetDungeons()
        {
            return HotspotDungeons.Values.ToList();
        }

        public static void RemoveDungeonPlayer(string lb, Player player)
        {
            var guid = player.Guid.Full;

            if (HotspotDungeons.TryGetValue(lb, out Dungeon currentDungeon))
                if (currentDungeon.Players.ContainsKey(guid))
                    currentDungeon.Players.Remove(guid);
        }

        public static void AddDungeonPlayer(string nextLb, Player player)
        {
            var guid = player.Guid.Full;

            if (HotspotDungeons.TryGetValue(nextLb, out Dungeon nextDungeon))
                if (!nextDungeon.Players.ContainsKey(guid))
                    nextDungeon.Players.TryAdd(guid, player);
        }

        internal static void ProcessCreaturesDeath(string currentLb, Player damager, int xpOverride, out double returnValue)
        {
            returnValue = 1; // Default value

            var damagerInstance = damager.Location.Instance;

            if (HotspotDungeons.TryGetValue(currentLb, out Dungeon currentDungeon))
            {
                if (damagerInstance == currentDungeon.Instance)
                {
                    //returnValue = currentDungeon.BonuxXp; // Assigning the total XP to the out parameter
                }

            }
            else if (!PotentialHotspotCandidates.ContainsKey(currentLb))
            {
                var dungeonLandblock = DungeonRepository.GetDungeon(currentLb);
                if (dungeonLandblock != null)
                {
                    var potentialDungeon = new Dungeon(dungeonLandblock.Landblock, dungeonLandblock.Name, dungeonLandblock.Coords, new ACE.Entity.Position(damager.Location));
                    PotentialHotspotCandidates.TryAdd(currentLb, potentialDungeon);
                    potentialDungeon.AddTotalXp(xpOverride);
                    potentialDungeon.PlayerTouches++;
                }
            }
            else
            {
                var potentialDungeon = PotentialHotspotCandidates[currentLb];
                if (potentialDungeon != null)
                {
                    potentialDungeon.AddTotalXp(xpOverride);
                    potentialDungeon.PlayerTouches++;
                    potentialDungeon.DropPosition = new ACE.Entity.Position(damager.Location);
                }
            }

            if (DungeonsTimeRemaining.TotalMilliseconds <= 0)
            {
                Reset();
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
