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

        public static readonly Dictionary<uint, WeeklyXp> WeeklyLevelWithCapXp = new Dictionary<uint, WeeklyXp>()
        {
            { 1, new WeeklyXp(46465302, 48) },
            { 2, new WeeklyXp(150013037,62) },
            { 3, new WeeklyXp(387419625, 76) },
            { 4, new WeeklyXp(859755734, 90) },
            { 5, new WeeklyXp(1709581309, 104) },
            { 6, new WeeklyXp(3128116563, 118) },
            { 7, new WeeklyXp(5362412965, 132) },
            { 8, new WeeklyXp(8722524219, 146) },
            { 9, new WeeklyXp(13588677261, 160) },
            { 10, new WeeklyXp(20418443236, 174) },
            { 11, new WeeklyXp(29753908491, 188) },
            { 12, new WeeklyXp(42228845559, 202) },
            { 13, new WeeklyXp(58575884147, 216) },
            { 14, new WeeklyXp(79633682122, 230) },
            { 15, new WeeklyXp(106354096497, 244) },
        };

        public static void Initialize()
        {
            GetXpCapTimestamps();
            CalculateCurrentDailyXpCap();
            Initialized = true;
        }

        public class WeeklyXp
        {
            public readonly ulong TotalXp;
            public readonly int Level;
            public WeeklyXp(ulong totalXp, int level)
            {
                TotalXp = totalXp;
                Level = level;
            }
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
            var totalWeeklyXp = week > 1 ? WeeklyLevelWithCapXp[week].TotalXp - WeeklyLevelWithCapXp[week - 1].TotalXp : WeeklyLevelWithCapXp[week].TotalXp;
            var dailyxp = totalWeeklyXp / 7;
            var endOfWeek = WeeklyTimestamp;

            ulong previous = week > 1 ? WeeklyLevelWithCapXp[week - 1].TotalXp : 0;
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
                SetPlayerXpCap(player);
        }

        public static void SetPlayerXpCap(IPlayer player)
        {
            player.SetProperty(ACE.Entity.Enum.Properties.PropertyInt64.QuestXp, 0);
            player.SetProperty(ACE.Entity.Enum.Properties.PropertyInt64.MonsterXp, 0);
            player.SetProperty(ACE.Entity.Enum.Properties.PropertyInt64.PvpXp, 0);

            var playerTotalXp = player.GetProperty(ACE.Entity.Enum.Properties.PropertyInt64.TotalExperience);
            var diff = (long)CurrentDailyXp.XpCap - (long)playerTotalXp;
            var xpPerCategory = diff / 3;
            var xpCategoryHalf = xpPerCategory / 2;

            player.SetProperty(ACE.Entity.Enum.Properties.PropertyInt64.QuestXpDailyMax, (int)(diff * 0.4));
            player.SetProperty(ACE.Entity.Enum.Properties.PropertyInt64.MonsterXpDailyMax, (int)(diff * 0.4));
            player.SetProperty(ACE.Entity.Enum.Properties.PropertyInt64.PvpXpDailyMax, (int)(diff * 0.2));
        }

        public static double GetPlayerLevelXpModifier(Player player)
        {
            return (double)WeeklyLevelWithCapXp[Week].Level / (double)player.Level;
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
