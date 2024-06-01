using ACE.Database;
using ACE.DatLoader.FileTypes;
using ACE.Entity.Enum;
using ACE.Server.Entity;
using ACE.Server.Managers;
using ACE.Server.Network.GameAction.Actions;
using ACE.Server.WorldObjects;
using log4net;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACE.Server.Features.Xp
{
    internal class XpManager
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly object xpLock = new object();
        public static DateTime DailyTimestamp { get; set; }
        public static DateTime WeeklyTimestamp { get; set; }

        public static List<DailyXp> DailyXpCache { get; set; } = new List<DailyXp>();
        public static uint Week { get; private set; }

        public static DailyXp CurrentDailyXp
        {
            get
            {
                DailyXp firstDay = DailyXpCache[0];
                for (var i = 0; i < DailyXpCache.Count; i++)
                    if (DailyXpCache[i].DailyExpiration > DateTime.UtcNow)
                        return DailyXpCache[i];

                return firstDay;
            }
        }

        private static bool Initialized = false;

        public static readonly Dictionary<uint, ulong> WeeklyLevelWithCapXp = new Dictionary<uint, ulong>()
        {
            { 1, 46465302 },
            { 2, 246555428 },
            { 3, 859755734  },
            { 4, 2333712089 },
            { 5, 10940644110 },
            { 6, 35555554692 },
            { 7, 92221953273 },
            { 8, 191226310247 }
        };

        public static void Initialize()
        {
            GetXpCapTimestamps();
            CalculateCurrentDailyXpCap();
            Initialized = true;
        }

        public class DailyXp
        {
            public readonly DateTime DailyExpiration;
            public readonly ulong XpCap;

            public DailyXp(DateTime dailyExpiration, ulong xpCap)
            {
                DailyExpiration = dailyExpiration;
                XpCap = xpCap;
            }
        }

        public static void CalculateCurrentDailyXpCap()
        {
            DailyXpCache.Clear();
            var week = Week;
            var totalWeeklyXp = week > 1 ? WeeklyLevelWithCapXp[week] - WeeklyLevelWithCapXp[week - 1] : WeeklyLevelWithCapXp[week];
            var dailyxp = totalWeeklyXp / 7;
            var endOfWeek = WeeklyTimestamp;

            ulong previous = week > 1 ? WeeklyLevelWithCapXp[week - 1] : 0;
            for (var i = 7; i >= 1; i--)
            {
                var day = endOfWeek.AddDays(-i);
                day = day.AddDays(1);
                var newDaily = dailyxp + previous;
                previous = newDaily;
                var dailyXp = new DailyXp(day, newDaily);
                DailyXpCache.Add(dailyXp);
            }

            DailyTimestamp = CurrentDailyXp.DailyExpiration;
        }

        public static void Tick()
        {

            lock (xpLock)
            {
                if (!Initialized)
                {
                    log.Warn("Warning, XpManager was not initialized, daily cap tick will not be processed!");
                    return;
                }

                if (IsDailyTimestampExpired())
                {
                    GetUpdatedXpCapTimestamps();

                    // season ends at week 8
                    if (Week > 8)
                        return;

                    CalculateCurrentDailyXpCap();
                    ResetPlayersForDaily();
                }
            }

        }

        public static void ResetPlayersForDaily()
        {
            var players = PlayerManager.GetAllPlayers();
            foreach (var player in players)
                SetPlayerXpCap(player);
        }
        public static void ResetPlayersForDaily(string name)
        {
            var player = PlayerManager.FindByName(name);
            if (player != null)
                SetPlayerXpCap(player);
        }

        public static void SetPlayerXpCap(IPlayer player)
        {
            var homeRealm = player.GetProperty(ACE.Entity.Enum.Properties.PropertyInt.HomeRealm);
            if (homeRealm == null)
                return;

            if ((ushort)homeRealm != RealmManager.CurrentSeason.Realm.Id)
                return;

            player.SetProperty(ACE.Entity.Enum.Properties.PropertyInt64.QuestXp, 0);
            player.SetProperty(ACE.Entity.Enum.Properties.PropertyInt64.MonsterXp, 0);
            player.SetProperty(ACE.Entity.Enum.Properties.PropertyInt64.PvpXp, 0);

            var playerTotalXp = player.GetProperty(ACE.Entity.Enum.Properties.PropertyInt64.TotalExperience);
            var diff = (long)CurrentDailyXp.XpCap - (long)playerTotalXp;

            player.SetProperty(ACE.Entity.Enum.Properties.PropertyInt64.QuestXpDailyMax, (long)(diff * 0.4));
            player.SetProperty(ACE.Entity.Enum.Properties.PropertyInt64.MonsterXpDailyMax, (long)(diff * 0.4));
            player.SetProperty(ACE.Entity.Enum.Properties.PropertyInt64.PvpXpDailyMax, (long)(diff * 0.2));
        }

        public static double? MaxLevel = null;

        private static DateTime GetAverageModifierTimestamp;

        public static double GetPlayerLevelXpModifier(int level)
        {
            var duration = PropertyManager.GetLong("xp_average_check_duration").Item;
            if (MaxLevel == null || DateTime.UtcNow - GetAverageModifierTimestamp > TimeSpan.FromMinutes(duration))
            {
                GetAverageModifierTimestamp = DateTime.UtcNow;
                var maxLevel = PlayerManager.GetAllPlayers()
                    .Where(player => {
                        var homeRealm = player.GetProperty(ACE.Entity.Enum.Properties.PropertyInt.HomeRealm);
                        return homeRealm != null &&
                            player.Account.AccessLevel == (uint)AccessLevel.Player &&
                            (ushort)homeRealm == RealmManager.CurrentSeason.Realm.Id;
                    })
                    .OrderByDescending(player => player.Level)
                    .Select(player => player.Level)
                    .Take(1)
                    .FirstOrDefault();

                if (maxLevel == null)
                    return 1;

               MaxLevel = maxLevel;
            }

           return (double)MaxLevel / (double)level;
        }

        public static void GetXpCapTimestamps()
        {
            var (_, weekly, week) = DatabaseManager.Shard.BaseDatabase.GetXpCapTimestamps();
            WeeklyTimestamp = weekly;
            Week = week;
        }

        public static void GetUpdatedXpCapTimestamps()
        {
            var (_, weekly, week) = DatabaseManager.Shard.BaseDatabase.UpdateXpCap();
            WeeklyTimestamp = weekly;
            Week = week;
        }


        public static bool IsDailyTimestampExpired()
        {
            // Check if the daily timestamp is before the current time
            return DailyTimestamp < DateTime.UtcNow;
        }
    }
}
