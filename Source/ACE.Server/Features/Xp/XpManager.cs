using ACE.Database;
using ACE.DatLoader.FileTypes;
using ACE.Entity.Enum;
using ACE.Server.Managers;
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
            { 2, 150013037 },
            { 3, 387419625 },
            { 4, 859755734 },
            { 5, 1709581309 },
            { 6, 3128116563 },
            { 7, 5362412965 },
            { 8, 8722524219 },
            { 9, 13588677261 },
            { 10, 20418443236 },
            { 11, 29753908491 },
            { 12, 42228845559 },
            { 13, 58575884147 },
            { 14, 79633682122 },
            { 15, 106354096497 }
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

                    // season ends at week 15
                    if (Week > 15)
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
            {
                player.SetProperty(ACE.Entity.Enum.Properties.PropertyInt64.QuestXp, 0);
                player.SetProperty(ACE.Entity.Enum.Properties.PropertyInt64.MonsterXp, 0);
                player.SetProperty(ACE.Entity.Enum.Properties.PropertyInt64.PvpXp, 0);

                var playerTotalXp = player.GetProperty(ACE.Entity.Enum.Properties.PropertyInt64.TotalExperience);
                var diff = (long)CurrentDailyXp.XpCap - (long)playerTotalXp;
                var xpPerCategory = diff / 3;
                var xpCategoryHalf = xpPerCategory / 2;

                player.SetProperty(ACE.Entity.Enum.Properties.PropertyInt64.QuestXpDailyMax, xpPerCategory + xpPerCategory);
                player.SetProperty(ACE.Entity.Enum.Properties.PropertyInt64.MonsterXpDailyMax, xpPerCategory - xpCategoryHalf);
                player.SetProperty(ACE.Entity.Enum.Properties.PropertyInt64.PvpXpDailyMax, xpPerCategory - xpCategoryHalf);
            }
        }

        public static double GetPlayerLevelXpModifier(int level)
        {
            var players = PlayerManager.GetAllPlayers()
               .Where(player => player.Account.AccessLevel == (uint)AccessLevel.Player && player.Level > 10)
               .OrderByDescending(player => player.Level)
               .Select(player => player.Level)
               .Distinct()
               .ToList();

            var average = players.Average();

            if (average == null)
                return 1.0;

            return (double)average / (double)level;
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
