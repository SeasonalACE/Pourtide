using ACE.Database;
using ACE.Server.Managers;
using ACE.Server.WorldObjects;
using log4net;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACE.Server.Xp
{
    internal class XpManager
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly object xpLock = new object();
        private static DateTime DailyTimestamp { get; set; }
        private static DateTime WeeklyTimestamp { get; set; }
        private static uint Week { get; set; }
        public static ulong DailyXpCap { get; private set; }

        private static bool Initialized = false;

        private static readonly Dictionary<uint, ulong> WeeklyLevelWithCapXp = new Dictionary<uint, ulong>()
        {
            { 1, 10024047 },
            { 2, 46465302 },
            { 3, 150013037 },
            { 4, 387419625 },
            { 5, 859755734 },
            { 6, 1709581309 },
            { 7, 3128116563 },
            { 8, 5362412965 },
            { 9, 8722524219 },
            { 10, 13588677261 },
            { 11, 20418443236 },
            { 12, 29753908491 },
            { 13, 42228845559 },
            { 14, 58575884147 },
            { 15, 79633682122 },
            { 16, 106354096497 }
        };

        public static void Initialize()
        {
            GetXpCapTimestamps();
            CalculateCurrentDailyXpCap();
            Initialized = true;
        }

        private static void CalculateCurrentDailyXpCap()
        {
            var totalWeeklyXp = WeeklyLevelWithCapXp[Week];

            // Divide by 3 for pvpxp, monsterxp, questxp
            var weeklyCap = totalWeeklyXp / 3;
            var dailyCap = weeklyCap / 7;
            DailyXpCap = dailyCap;
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
                    var previousDaily = DailyXpCap;
                    GetXpCapTimestamps();
                    log.Info($"[XpManager]: DailyXpCap has expired showing timestamps");
                    log.Info($"[XpManager]: PreviousDaily: {previousDaily}");
                    log.Info($"[XpManager]: Now: {DateTime.UtcNow}");
                    log.Info($"[XpManager]: NextDaily: {DailyXpCap}");

                    // season ends at week 16
                    if (Week > 16)
                        return;

                    CalculateCurrentDailyXpCap();
                    var players = PlayerManager.GetAllPlayers();
                    foreach (var player in players)
                    {
                        var questXp = player.GetProperty(ACE.Entity.Enum.Properties.PropertyInt64.QuestXp) ?? 0;
                        var monsterXp = player.GetProperty(ACE.Entity.Enum.Properties.PropertyInt64.MonsterXp) ?? 0;
                        var pvpXp = player.GetProperty(ACE.Entity.Enum.Properties.PropertyInt64.PvpXp) ?? 0;

                        var questXpMax = player.GetProperty(ACE.Entity.Enum.Properties.PropertyInt64.QuestXpDailyMax) ?? 0;
                        var monsterXpMax = player.GetProperty(ACE.Entity.Enum.Properties.PropertyInt64.MonsterXpDailyMax) ?? 0;
                        var pvpXpMax = player.GetProperty(ACE.Entity.Enum.Properties.PropertyInt64.PvpXpDailyMax) ?? 0;

                        // handle rollover
                        var questDiff = (long)questXpMax - questXp;
                        var monsterDiff = (long)monsterXpMax - monsterXp;
                        var pvpDiff = (long)pvpXpMax - pvpXp;

                        player.SetProperty(ACE.Entity.Enum.Properties.PropertyInt64.QuestXp, 0);
                        player.SetProperty(ACE.Entity.Enum.Properties.PropertyInt64.MonsterXp, 0);
                        player.SetProperty(ACE.Entity.Enum.Properties.PropertyInt64.PvpXp, 0);

                        player.SetProperty(ACE.Entity.Enum.Properties.PropertyInt64.QuestXpDailyMax, (long)DailyXpCap + questDiff);
                        player.SetProperty(ACE.Entity.Enum.Properties.PropertyInt64.MonsterXpDailyMax, (long)DailyXpCap + monsterDiff);
                        player.SetProperty(ACE.Entity.Enum.Properties.PropertyInt64.PvpXpDailyMax, (long)DailyXpCap + pvpDiff);
                    }
                }
            }

        }

        public static void GetXpCapTimestamps()
        {
            var (daily, weekly, week) = DatabaseManager.Shard.BaseDatabase.GetXpCapTimestamps();
            DailyTimestamp = daily;
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
