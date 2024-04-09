using ACE.Adapter.GDLE.Models;
using ACE.Common;
using ACE.Entity.Enum;
using ACE.Server.Entity;
using ACE.Server.Managers;
using ACE.Server.Network.GameMessages.Messages;
using ACE.Server.WorldObjects;
using log4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACE.Server.HotDungeons.Managers
{
    public class Dungeon : DungeonBase
    {
        public int TotalXpEarned { get; set; } = 0;

        public double BonuxXp { get; set; } = 1.0f;

        public uint PlayerTouches { get; set; } = 0;
        public Dungeon(string landblock, string name, string coords) : base(landblock, name, coords)
        {
            Landblock = landblock;
            Name = name;
            Coords = coords;
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

        private static string WebhookGeneral = "";

        private static float MaxBonuxXp = 4.0f;

        private static uint MaxHotspots { get; set; }

        private static TimeSpan DungeonsInterval { get; set; }

        private static DateTime DungeonsLastCheck { get; set; } = DateTime.MinValue;

        public static TimeSpan DungeonsTimeRemaining => DungeonsLastCheck + DungeonsInterval - DateTime.UtcNow;

        public static void Initialize(uint interval = 1, uint intialDelay = 20, float maxBonuxXp = 4.0f, uint maxHotspots = 3, string webhookGeneral = "")
        {
            log.Info("Initializing DungeonManager");
            DungeonRepository.Initialize();
            DungeonsInterval = TimeSpan.FromHours(interval);
            MaxBonuxXp = maxBonuxXp;
            MaxHotspots = maxHotspots;
            DungeonsLastCheck = DateTime.UtcNow - DungeonsInterval + TimeSpan.FromMinutes(intialDelay);
            WebhookGeneral = webhookGeneral;
            log.Info($"Dungeons will be reset in {FormatTimeRemaining(DungeonsTimeRemaining)}");
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

        public static void Reset()
        {

            lock (dungeonsLock)
            {
                if (DungeonsTimeRemaining.TotalMilliseconds > 0)
                    return;

                DungeonsLastCheck = DateTime.UtcNow;

                foreach (var dungeon in HotspotDungeons.Values.ToList())
                {
                    dungeon.Close();
                    var message = $"{dungeon.Name} is no longer boosted xp!";
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
                    dungeon.BonuxXp = ThreadSafeRandom.Next(1.5f, MaxBonuxXp);
                    HotspotDungeons.Add(dungeon.Landblock, dungeon);
                    var at = dungeon.Coords.Length > 0 ? $"at {dungeon.Coords}" : "";
                    var message = $"{dungeon.Name} {at} has been very active, this dungeon has been boosted with {dungeon.BonuxXp.ToString("0.00")}x xp for {FormatTimeRemaining(DungeonsTimeRemaining)}";
                    log.Info(message);
                    PlayerManager.BroadcastToAll(new GameMessageSystemChat(message, ChatMessageType.WorldBroadcast));

                    /*try
                    {
                        var channel = ChatType.General;
                        Player sender = null;
                        var formattedMessage = $"[CHAT][{channel.ToString().ToUpper()}] {(sender != null ? sender.Name : "[SYSTEM]")} says on the {channel} channel, \"{message}\"";
                        _ = WebhookRepository.SendWebhookChat(DiscordChatChannel.General, formattedMessage, WebhookGeneral);
                    }
                    catch (Exception ex)
                    {
                        log.Info(ex.StackTrace, log.InfoLevel.Error);
                    }*/
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

        internal static void ProcessCreaturesDeath(string currentLb, int xpOverride, out double returnValue)
        {
            returnValue = 1.0; // Default value

            if (HotspotDungeons.TryGetValue(currentLb, out Dungeon currentDungeon))
            {
                currentDungeon.AddTotalXp(xpOverride);
                returnValue = currentDungeon.BonuxXp; // Assigning the total XP to the out parameter
            }
            else if (!PotentialHotspotCandidates.ContainsKey(currentLb))
            {
                var dungeonLandblock = DungeonRepository.GetDungeon(currentLb);
                if (dungeonLandblock != null)
                {
                    var potentialDungeon = new Dungeon(dungeonLandblock.Landblock, dungeonLandblock.Name, dungeonLandblock.Coords);
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
