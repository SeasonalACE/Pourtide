using ACE.Common;
using ACE.Database;
using ACE.Database.Models.Shard;
using ACE.Entity;
using ACE.Entity.Enum;
using ACE.Entity.Enum.Properties;
using ACE.Entity.Models;
using ACE.Server.Entity;
using ACE.Server.Entity.Actions;
using ACE.Server.Features.Discord;
using ACE.Server.Features.HotDungeons.Managers;
using ACE.Server.Features.Rifts;
using ACE.Server.Features.Xp;
using ACE.Server.Managers;
using ACE.Server.Network;
using ACE.Server.Network.GameEvent.Events;
using ACE.Server.Network.GameMessages.Messages;
using ACE.Server.Physics.Common;
using ACE.Server.Realms;
using ACE.Server.WorldObjects;
using System;
using System.Linq;
using System.Text;

namespace ACE.Server.Command.Handlers
{
    public static class CustomCommands
    {
        [CommandHandler("season-info", AccessLevel.Player, CommandHandlerFlag.RequiresWorld, 0, "Get info about the current season your character belongs to.")]
        public static void HandleSeasonInfo(Session session, params string[] paramters)
        {
            var player  = session?.Player;
            var season = RealmManager.CurrentSeason;
            
            session.Network.EnqueueSend(new GameMessageSystemChat($"\n<Season Information>", ChatMessageType.System));
            session.Network.EnqueueSend(new GameMessageSystemChat($"\n{season.Realm.Name} - Id: {season.Realm.Id} - Instance: {season.StandardRules.GetDefaultInstanceID()}", ChatMessageType.System));
            session.Network.EnqueueSend(new GameMessageSystemChat($"\n{season.StandardRules.DebugOutputString()}", ChatMessageType.System));
        }

        [CommandHandler("season-list", AccessLevel.Player, CommandHandlerFlag.None, 0, "Get a list of available seasons to choose from.")]
        public static void HandleSeasonList(Session session, params string[] paramters)
        {
            session.Network.EnqueueSend(new GameMessageSystemChat($"\n<Season List>", ChatMessageType.System));
            session.Network.EnqueueSend(new GameMessageSystemChat($"\n{RealmManager.GetSeasonList()}", ChatMessageType.System));
        }

        [CommandHandler("realm-list", AccessLevel.Player, CommandHandlerFlag.None, 0, "Get a list of available realms stored in RealmManager.")]
        public static void HandleRealmList(Session session, params string[] paramters)
        {
            session.Network.EnqueueSend(new GameMessageSystemChat($"\n<Realm List>", ChatMessageType.System));
            session.Network.EnqueueSend(new GameMessageSystemChat($"\n{RealmManager.GetRealmList()}", ChatMessageType.System));
        }

        [CommandHandler("ruleset-list", AccessLevel.Player, CommandHandlerFlag.None, 0, "Get a list of available rulesets stored in RealmManager.")]
        public static void HandleRulesetList(Session session, params string[] paramters)
        {
            session.Network.EnqueueSend(new GameMessageSystemChat($"\n<Ruleset List>", ChatMessageType.System));
            session.Network.EnqueueSend(new GameMessageSystemChat($"\n{RealmManager.GetRulesetsList()}", ChatMessageType.System));

        }

        [CommandHandler("set-home-realm", AccessLevel.Developer, CommandHandlerFlag.RequiresWorld, 0, "Sets the home realm of a player.")]
        public static void HandleSetHomeRealm(Session session, params string[] parameters)
        {
            if (parameters.Length < 1)
                return;
            if (!ushort.TryParse(parameters[0], out var realmid))
                return;

            var objectId = ObjectGuid.Invalid;

            var target = session.Player.CurrentAppraisalTarget;

            if (target.HasValue)
                objectId = new ObjectGuid((uint)session.Player.CurrentAppraisalTarget);
            else
                objectId = new ObjectGuid((uint)session.Player.Guid.Full);

            var wo = session.Player.CurrentLandblock?.GetObject(objectId);
            if (wo != null && wo is Player player)
            {
                var positionMessage = new GameMessageSystemChat($"Setting home realm of {player.Name} to realm {realmid}.", ChatMessageType.Broadcast);
                session.Network.EnqueueSend(positionMessage);
                RealmManager.SetHomeRealm(player, realmid);
            }
        }

        [CommandHandler("telerealm", AccessLevel.Developer, CommandHandlerFlag.RequiresWorld, 0, "Teleports the current player to another realm.")]
        public static void HandleMoveRealm(Session session, params string[] parameters)
        {
            if (parameters.Length < 1)
                return;
            if (!ushort.TryParse(parameters[0], out var realmid))
                return;

            var pos = session.Player.Location;
            var newpos = new InstancedPosition(pos, InstancedPosition.InstanceIDFromVars(realmid, 0, false));

            session.Player.Teleport(newpos);
            var positionMessage = new GameMessageSystemChat($"Teleporting to realm {realmid}.", ChatMessageType.Broadcast);
            session.Network.EnqueueSend(positionMessage);
        }

        [CommandHandler("realm-info", AccessLevel.Player, CommandHandlerFlag.RequiresWorld, "Lists all properties for the current realm.")]
        public static void HandleZoneInfo(Session session, params string[] parameters)
        {
            session.Network.EnqueueSend(new GameMessageSystemChat($"\n<Realm Information>", ChatMessageType.System));
            session.Network.EnqueueSend(new GameMessageSystemChat($"\n{session.Player.CurrentLandblock.RealmRuleset.Realm.Name} - Id: {session.Player.CurrentLandblock.RealmRuleset.Realm.Id} - Instance: {session.Player.Location.Instance} ", ChatMessageType.System));
            session.Network.EnqueueSend(new GameMessageSystemChat($"\n{session.Player.CurrentLandblock.RealmRuleset.DebugOutputString()}", ChatMessageType.System));
        }

        [CommandHandler("exitinstance", AccessLevel.Developer, CommandHandlerFlag.RequiresWorld, "Leaves the current instance, if the player is currently in one.")]
        public static void HandleExitInstance(Session session, params string[] parameters)
        {
            session.Player.ExitInstance();
        }

        [CommandHandler("exitinst", AccessLevel.Developer, CommandHandlerFlag.RequiresWorld, "Leaves the current instance, if the player is currently in one.")]
        public static void HandleExitInst(Session session, params string[] parameters)
        {
            session.Player.ExitInstance();
        }


        [CommandHandler("exiti", AccessLevel.Developer, CommandHandlerFlag.RequiresWorld, "Leaves the current instance, if the player is currently in one.")]
        public static void HandleExitI(Session session, params string[] parameters)
        {
            session.Player.ExitInstance();
        }

        [CommandHandler("leaveinstance", AccessLevel.Developer, CommandHandlerFlag.RequiresWorld, "Leaves the current instance, if the player is currently in one.")]
        public static void HandleLeaveInstance(Session session, params string[] parameters)
        {
            session.Player.ExitInstance();
        }

        [CommandHandler("leaveinst", AccessLevel.Developer, CommandHandlerFlag.RequiresWorld, "Leaves the current instance, if the player is currently in one.")]
        public static void HandleLeaveInst(Session session, params string[] parameters)
        {
            session.Player.ExitInstance();
        }

        [CommandHandler("leavei", AccessLevel.Developer, CommandHandlerFlag.RequiresWorld, "Leaves the current instance, if the player is currently in one.")]
        public static void HandleLeaveI(Session session, params string[] parameters)
        {
            session.Player.ExitInstance();
        }
      
        [CommandHandler("rebuff", AccessLevel.Developer, CommandHandlerFlag.RequiresWorld, 0,
            "Buffs you with all beneficial spells. Only usable in certain realms.")]
        public static void HandleRebuff(Session session, params string[] parameters)
        {
            var player = session.Player;
            var realm = RealmManager.GetRealm(player.HomeRealm);
            if (realm == null) return;
            if (!realm.StandardRules.GetProperty(RealmPropertyBool.IsDuelingRealm)) return;
            var ts = player.GetProperty(PropertyInt.LastRebuffTimestamp);
            if (ts != null)
            {
                var timesince = (int)Time.GetUnixTime() - ts.Value;
                if (timesince < 180)
                {
                    session.Network.EnqueueSend(new GameMessageSystemChat($"You may use this command again in {180 - timesince}s.", ChatMessageType.Broadcast));
                    return;
                }
            }
            player.SetProperty(PropertyInt.LastRebuffTimestamp, (int)Time.GetUnixTime());
            player.CreateSentinelBuffPlayers(new Player[] { player }, true);
        }

        /** HotDungeons Start **/

        [CommandHandler("rifts", AccessLevel.Player, CommandHandlerFlag.None, 0, "Get a list of available dungeons.")]
        [CommandHandler("dungeons", AccessLevel.Player, CommandHandlerFlag.None, 0, "Get a list of available dungeons.")]
        public static void HandleCheckDungeons(Session session, params string[] parameters)
        {
            ulong discordChannel = 0;
            if (parameters.Length > 1 && parameters[0] == "discord")
                ulong.TryParse(parameters[1], out discordChannel);

            StringBuilder message = new StringBuilder();

            message.Append("<Active Rift List>\n");
            message.Append("-----------------------\n");

            foreach (var rift in RiftManager.GetRifts())
            {
                var oreDropChance = rift.LandblockInstance.RealmRuleset.GetProperty(RealmPropertyInt.OreDropChance);
                var oreSlayerDropChance = rift.LandblockInstance.RealmRuleset.GetProperty(RealmPropertyInt.OreSlayerDropChance);
                var oreSalvageDropAmount = rift.LandblockInstance.RealmRuleset.GetProperty(RealmPropertyInt.OreSalvageDropAmount);
                Position.ParseInstanceID(rift.HomeInstance, out var isEphemeralRealm, out var realmId, out var instanceId);
                var realm = RealmManager.GetRealm(realmId).Realm;
                message.Append($"Rift {rift.Name} from realm {realm.Name} is active!\n");
                message.Append($"With an xp bonus of {rift.BonuxXp.ToString("0.00")}x.\n");
                message.Append($"With an ore drop chance of 1/{oreDropChance}.\n");
                message.Append($"With an ore slayer drop chance of 1/{oreSlayerDropChance}.\n");
                message.Append($"With an ore salvage drop amount of {oreSalvageDropAmount * 2}.\n");
                message.Append("-----------------------\n");
            }

            message.Append("-----------------------\n");
            message.Append($"The Rift Entrance Portal in Subway (main hall, first room on the right) will bring you to a random Rift\n");
            message.Append($"<Time Remaining before reset: {DungeonManager.FormatTimeRemaining(DungeonManager.DungeonsTimeRemaining)}>\n");

            if (discordChannel == 0)
                CommandHandlerHelper.WriteOutputInfo(session, message.ToString(), ChatMessageType.Broadcast);
            else
                DiscordChatBridge.SendMessage(discordChannel, $"`{message.ToString()}`");

            if (DungeonManager.DungeonsTimeRemaining.TotalMilliseconds <= 0)
                DungeonManager.Reset();
        }

        [CommandHandler("reset-dungeons", AccessLevel.Admin, CommandHandlerFlag.None, 0, "Get a list of available dungeons.")]
        public static void HandleResetDungeons(Session session, params string[] paramters)
        {
            session.Network.EnqueueSend(new GameMessageSystemChat($"\n<Resettinga Hot Dungeons>", ChatMessageType.System));
            DungeonManager.Reset(true);
        }

        [CommandHandler("dungeons-potential", AccessLevel.Developer, CommandHandlerFlag.None, 0, "Get a list of available potential dungeons.")]
        public static void HandleCheckDungeonsPotential(Session session, params string[] paramters)
        {
            session.Network.EnqueueSend(new GameMessageSystemChat($"\n<Active Potential Dungeon List>", ChatMessageType.System));
            foreach (var (realmId, dungeon) in DungeonManager.GetPotentialDungeons())
            {
                var realm = RealmManager.GetRealm(realmId).Realm.Name;
                if (realm == null)
                    continue;

                var at = dungeon.Coords.Length > 0 ? $"at {dungeon.Coords}" : "";
                var message = $"Dungeon {dungeon.Name} from realm {realm} has potential {at}";
                session.Network.EnqueueSend(new GameMessageSystemChat($"\n{message}", ChatMessageType.System));

                var xp = dungeon.TotalXpEarned;
                var playersTouched = dungeon.PlayerTouches;

                var xpMessage = $"--> Xp Earned: {Formatting.FormatIntWithCommas((uint)xp)}";
                session.Network.EnqueueSend(new GameMessageSystemChat($"\n{xpMessage}", ChatMessageType.System));

                var playersTouchedMessage = $"--> Amount of creatures killed by players: {Formatting.FormatIntWithCommas(playersTouched)}";
                session.Network.EnqueueSend(new GameMessageSystemChat($"\n{playersTouchedMessage}", ChatMessageType.System));
            }

            session.Network.EnqueueSend(new GameMessageSystemChat($"\nTime Remaining before reset: {DungeonManager.FormatTimeRemaining(DungeonManager.DungeonsTimeRemaining)}", ChatMessageType.System));
        }



        /** Hot Dungeons End **/

        [CommandHandler("fi", AccessLevel.Player, CommandHandlerFlag.RequiresWorld, "Resends all visible items and creatures to the client")]
        [CommandHandler("fixinvisible", AccessLevel.Player, CommandHandlerFlag.RequiresWorld, "Resends all visible items and creatures to the client")]
        public static void HandleFixInvisible(Session session, params string[] parameters)
        {
            session.Player.FixInvis();

        }

        /** Xp Cap Start **/
        [CommandHandler("reset-xp", AccessLevel.Admin, CommandHandlerFlag.None, 0, "Reset xp cap.")]
        public static void HandleResetXpCap(Session session, params string[] paramters)
        {
            session.Network.EnqueueSend(new GameMessageSystemChat($"\n<Resetting Daily Xp Cap>", ChatMessageType.System));
            XpManager.ResetPlayersForDaily();
        }

        [CommandHandler("show-xp", AccessLevel.Player, CommandHandlerFlag.None, 0, "Show xp cap information.")]
        public static void HandleShowXp(Session session, params string[] paramters)
        {
            var player = session.Player;
            session.Network.EnqueueSend(new GameMessageSystemChat($"\n<Showing Xp Cap Information>", ChatMessageType.System));
            var queryXp = player.QuestXp;
            var pvpXp = player.PvpXp;
            var monsterXp = player.MonsterXp;

            var queryXpDailyCap = player.QuestXpDailyMax;
            var pvpXpDailyCap = player.PvpXpDailyMax;
            var monsterXpDailyCap = player.MonsterXpDailyMax;

            var globalAverageModifier = XpManager.GetPlayerLevelXpModifier((int)player.Level).ToString("0.00");

            session.Network.EnqueueSend(new GameMessageSystemChat($"\n--> The daily xp cap today for all players is {Formatting.FormatIntWithCommas(XpManager.CurrentDailyXp.XpCap)}.", ChatMessageType.System));
            session.Network.EnqueueSend(new GameMessageSystemChat($"\n--> The current highest level player for the server is {(uint)XpManager.MaxLevel}.", ChatMessageType.System));
            session.Network.EnqueueSend(new GameMessageSystemChat($"\n--> Your current global xp modifier is {globalAverageModifier}x.", ChatMessageType.System));
            session.Network.EnqueueSend(new GameMessageSystemChat($"\n--> You have currently earned {Formatting.FormatIntWithCommas((ulong)queryXp)} / {Formatting.FormatIntWithCommas((ulong)queryXpDailyCap)} quest xp for the day.", ChatMessageType.System));
            session.Network.EnqueueSend(new GameMessageSystemChat($"\n--> You have currently earned {Formatting.FormatIntWithCommas((ulong)pvpXp)} / {Formatting.FormatIntWithCommas((ulong)pvpXpDailyCap)} pvp xp for the day.", ChatMessageType.System));
            session.Network.EnqueueSend(new GameMessageSystemChat($"\n--> You have currently earned {Formatting.FormatIntWithCommas((ulong)monsterXp)} / {Formatting.FormatIntWithCommas((ulong)monsterXpDailyCap)} monster xp for the day.", ChatMessageType.System));
            session.Network.EnqueueSend(new GameMessageSystemChat($"\n--> The next daily xp reset will happen on {Formatting.FormatUtcToPst(XpManager.CurrentDailyXp.DailyExpiration)}.", ChatMessageType.System));
            session.Network.EnqueueSend(new GameMessageSystemChat($"\n--> The next weekly xp reset will happen on {Formatting.FormatUtcToPst(XpManager.WeeklyTimestamp)}.", ChatMessageType.System));
        }
        /** Xp Cap End **/

        /** Leaderboards/Stats Start **/
        [CommandHandler("leaderboards-kills", AccessLevel.Player, CommandHandlerFlag.None, 0, "Show top 10 kills leaderboard.")]
        public static void HandleLeaderboardsKills(Session session, params string[] parameters)
        {
            if (session != null)
            {
                if (session.AccessLevel == AccessLevel.Player && DateTime.UtcNow - session.Player.PrevLeaderboardPvPKillsCommandRequestTimestamp < TimeSpan.FromMinutes(1))
                {
                    session.Network.EnqueueSend(new GameMessageSystemChat("You have used this command too recently!", ChatMessageType.Broadcast));
                    return;
                }
                session.Player.PrevLeaderboardPvPKillsCommandRequestTimestamp = DateTime.UtcNow;
            }

            ulong discordChannel = 0;
            if (parameters.Length > 1 && parameters[0] == "discord")
                ulong.TryParse(parameters[1], out discordChannel);

            StringBuilder message = new StringBuilder();

            message.Append("<Showing Top 10 Kills Leaderboard>\n");
            message.Append("-----------------------\n");

            var players = DatabaseManager.Shard.BaseDatabase.GetTopTenPlayersWithMostKills();

            for (var i = 0; i < players.Count; i++)
            {
                var stats = players[i];
                var player = PlayerManager.FindByGuid(stats.PlayerId);
                if (player == null)
                    continue;
                message.Append($"{i + 1}. Name = {player.Name}, Kills = {stats.KillCount}\n");
            }

            message.Append("-----------------------\n");

            if (discordChannel == 0)
                CommandHandlerHelper.WriteOutputInfo(session, message.ToString(), ChatMessageType.Broadcast);
            else
                DiscordChatBridge.SendMessage(discordChannel, $"`{message.ToString()}`");
        }

        /** Leaderboards/Stats Start **/
        [CommandHandler("my-kills", AccessLevel.Player, CommandHandlerFlag.None, 0, "Show my kills.")]
        public static void HandlePersonalKills(Session session, params string[] parameters)
        {
            if (session != null)
            {
                if (session.AccessLevel == AccessLevel.Player && DateTime.UtcNow - session.Player.PrevPersonalPvPKillsCommandRequestTimestamp < TimeSpan.FromMinutes(1))
                {
                    session.Network.EnqueueSend(new GameMessageSystemChat("You have used this command too recently!", ChatMessageType.Broadcast));
                    return;
                }
                session.Player.PrevPersonalPvPKillsCommandRequestTimestamp = DateTime.UtcNow;
            }

            ulong discordChannel = 0;
            if (parameters.Length > 1 && parameters[0] == "discord")
                ulong.TryParse(parameters[1], out discordChannel);

            StringBuilder message = new StringBuilder();

            message.Append("<Showing Kill Count>\n");
            message.Append("-----------------------\n");

            var (count, last10Victims) = DatabaseManager.Shard.BaseDatabase.GetPersonalKillStats((uint)session.Player.Guid.Full);

            message.Append($"{session.Player.Name}, Total Kills = {count}\n");

            message.Append("-----------------------\n");
            message.Append("<Showing Last 10 Kills>\n");

            var victims = last10Victims
                .Select(guid => PlayerManager.FindByGuid(guid))
                .Where(player => player != null)
                .ToList();

            for (var i = 0; i < victims.Count; i++)
            {
                var victim = victims[i];
                message.Append($"{i + 1}. Name = {victim.Name}\n");
            }

            message.Append("-----------------------\n");

            if (discordChannel == 0)
                CommandHandlerHelper.WriteOutputInfo(session, message.ToString(), ChatMessageType.Broadcast);
            else
                DiscordChatBridge.SendMessage(discordChannel, $"`{message.ToString()}`");
        }

        /** Leaderboards/Stats Start **/
        [CommandHandler("leaderboards-deaths", AccessLevel.Player, CommandHandlerFlag.None, 0, "Show top 10 pvp deaths leaderboard.")]
        public static void HandleLeaderboardsDeaths(Session session, params string[] parameters)
        {
            if (session != null)
            {
                if (session.AccessLevel == AccessLevel.Player && DateTime.UtcNow - session.Player.PrevLeaderboardPvPDeathsCommandRequestTimestamp < TimeSpan.FromMinutes(1))
                {
                    session.Network.EnqueueSend(new GameMessageSystemChat("You have used this command too recently!", ChatMessageType.Broadcast));
                    return;
                }
                session.Player.PrevLeaderboardPvPDeathsCommandRequestTimestamp = DateTime.UtcNow;
            }

            ulong discordChannel = 0;
            if (parameters.Length > 1 && parameters[0] == "discord")
                ulong.TryParse(parameters[1], out discordChannel);

            StringBuilder message = new StringBuilder();

            message.Append("<Showing Top 10 Deaths Leaderboard>\n");
            message.Append("-----------------------\n");


            var players = DatabaseManager.Shard.BaseDatabase.GetPlayerWithMostDeaths();

            for (var i = 0; i < players.Count; i++)
            {
                var stats = players[i];
                var player = PlayerManager.FindByGuid(stats.PlayerId);
                if (player == null)
                    continue;
                message.Append($"{i + 1}. Name = {player.Name}, Deaths = {stats.DeathCount}\n");
            }

            message.Append("-----------------------\n");

            if (discordChannel == 0)
                CommandHandlerHelper.WriteOutputInfo(session, message.ToString(), ChatMessageType.Broadcast);
            else
                DiscordChatBridge.SendMessage(discordChannel, $"`{message.ToString()}`");

        }

        [CommandHandler("leaderboards-Xp", AccessLevel.Player, CommandHandlerFlag.None, 0, "Show top 10 Levels leaderboard.")]
        public static void HandleLeaderboardsXp(Session session, params string[] parameters)
        {
            if (session != null)
            {
                if (session.AccessLevel == AccessLevel.Player && DateTime.UtcNow - session.Player.PrevLeaderboardXPCommandRequestTimestamp < TimeSpan.FromMinutes(1))
                {
                    session.Network.EnqueueSend(new GameMessageSystemChat("You have used this command too recently!", ChatMessageType.Broadcast));
                    return;
                }
                session.Player.PrevLeaderboardXPCommandRequestTimestamp = DateTime.UtcNow;
            }

            ulong discordChannel = 0;
            if (parameters.Length > 1 && parameters[0] == "discord")
                ulong.TryParse(parameters[1], out discordChannel);

            StringBuilder message = new StringBuilder();

            message.Append("<Showing Top 10 Xp Leaderboard>\n");
            message.Append("-----------------------\n");

            var players = PlayerManager.GetAllPlayers()
                .Where(player => player.Account.AccessLevel == (uint)AccessLevel.Player)
                .OrderByDescending(player => player.Level)
                .Take(10)
                .ToList();

            for (var i = 0; i < players.Count; i++)
            {
                var info = players[i];
                var player = PlayerManager.FindByGuid(info.Guid);
                if (player == null)
                    continue;
                message.Append($"{i + 1}. Name = {player.Name}, Level = {player.Level}\n");
            }

            message.Append("-----------------------\n");

            if (discordChannel == 0)
                CommandHandlerHelper.WriteOutputInfo(session, message.ToString(), ChatMessageType.Broadcast);
            else
                DiscordChatBridge.SendMessage(discordChannel, $"`{message.ToString()}`");



        }

        /** Leadearboards/Stats End **/

        /** Player Utility Commands Start **/
        [CommandHandler("fl", AccessLevel.Player, CommandHandlerFlag.RequiresWorld, "Force log off of a character that's stuck in game. Is only allowed when initiated from a character that is on the same account as the target character.")]
        [CommandHandler("ForceLogoffStuckCharacter", AccessLevel.Player, CommandHandlerFlag.RequiresWorld, "Force log off of character that's stuck in game. Is only allowed when initiated from a character that is on the same account as the target character.")]
        public static void HandleForceLogoffStuckCharacter(Session session, params string[] parameters)
        {
            try
            {
                if (session == null)
                    return;

                var playerName = "";
                if (parameters.Length > 0)
                    playerName = string.Join(" ", parameters);

                Player target = null;

                if (!string.IsNullOrEmpty(playerName))
                {
                    var plr = PlayerManager.FindByName(playerName);
                    if (plr != null)
                    {
                        target = PlayerManager.GetOnlinePlayer(plr.Guid);

                        if (target == null)
                        {
                            CommandHandlerHelper.WriteOutputInfo(session, $"Unable to force log off for {plr.Name}: Player is not online.");
                            return;
                        }

                        //Verify the target is not the current player
                        if (session.Player.Guid == target.Guid)
                        {
                            CommandHandlerHelper.WriteOutputInfo(session, $"Unable to force log off for {plr.Name}: You cannot target yourself, please try with a different character on same account.");
                            return;
                        }

                        //Verify the target is on the same account as the current player
                        if (session.AccountId != target.Account.AccountId)
                        {
                            CommandHandlerHelper.WriteOutputInfo(session, $"Unable to force log off for {plr.Name}: Target must be within same account as the player who issues the logoff command. Please reach out for admin support.");
                            return;
                        }

                        DeveloperCommands.HandleForceLogoff(session, parameters);
                    }
                    else
                    {
                        CommandHandlerHelper.WriteOutputInfo(session, $"Unable to force log off for {playerName}: Player not found.");
                        return;
                    }
                }
                else
                {
                    CommandHandlerHelper.WriteOutputInfo(session, $"Invalid parameters, please provide a player name for the player that needs to be logged off.");
                    return;
                }

            } catch (Exception ex)
            {
                CommandHandlerHelper.WriteOutputError(session, $"Error: Failed to force logout player");
                CommandHandlerHelper.WriteOutputError(session, ex.Message);
                CommandHandlerHelper.WriteOutputError(session, System.Environment.StackTrace);
            }
        }

        /// <summary>
        /// List online players within the character's allegiance.
        /// </summary>
        [CommandHandler("who", AccessLevel.Player, CommandHandlerFlag.RequiresWorld, 0, "List online players within the character's allegiance.")]
        public static void HandleWho(Session session, params string[] parameters)
        {
            if (!PropertyManager.GetBool("command_who_enabled").Item)
            {
                session.Network.EnqueueSend(new GameMessageSystemChat("The command \"who\" is not currently enabled on this server.", ChatMessageType.Broadcast));
                return;
            }

            if (session.Player.MonarchId == null)
            {
                session.Network.EnqueueSend(new GameMessageSystemChat("You must be in an allegiance to use this command.", ChatMessageType.Broadcast));
                return;
            }

            if (DateTime.UtcNow - session.Player.PrevWho < TimeSpan.FromMinutes(1))
            {
                session.Network.EnqueueSend(new GameMessageSystemChat("You have used this command too recently!", ChatMessageType.Broadcast));
                return;
            }

            session.Player.PrevWho = DateTime.UtcNow;

            StringBuilder message = new StringBuilder();
            message.Append("Allegiance Members: \n");


            uint playerCounter = 0;
            foreach (var player in PlayerManager.GetAllOnline().OrderBy(p => p.Name))
            {
                if (player.MonarchId == session.Player.MonarchId)
                {
                    message.Append($"{player.Name} - Level {player.Level}\n");
                    playerCounter++;
                }
            }

            message.Append("Total: " + playerCounter + "\n");

            CommandHandlerHelper.WriteOutputInfo(session, message.ToString(), ChatMessageType.Broadcast);
        }
        /** Player Utility Commands End **/

        [CommandHandler("bounty", AccessLevel.Player, CommandHandlerFlag.RequiresWorld, 0, "Get bounty information from Pour Collector")]
        public static void HandleBountyInfo(Session session, params string[] paramters)
        {
            var player = session?.Player;
            var holtburg = DatabaseManager.World.GetCachedPointOfInterest("holtburg");
            var weenie = DatabaseManager.World.GetCachedWeenie(holtburg.WeenieClassId);

            if (weenie == null)
            {
                session.Network.EnqueueSend(new GameMessageSystemChat("Pour Collector is not available at this time!", ChatMessageType.Broadcast));
                return;
            }

            var loc = new LocalPosition(weenie.GetPosition(PositionType.Destination)).AsInstancedPosition(session.Player, ACE.Entity.Enum.RealmProperties.PlayerInstanceSelectMode.Same);

            var lbId = new LandblockId(loc.GetCell());
            var lb = LandblockManager.GetLandblock(lbId, loc.Instance, null, false);
            var collector = lb.GetAllWorldObjectsForDiagnostics().Where(wo => wo.WeenieClassId == 3000381).FirstOrDefault();

            if (collector == null)
            {
                session.Network.EnqueueSend(new GameMessageSystemChat("Pour Collector is not available at this time!", ChatMessageType.Broadcast));
                return;
            }

            Player.GetBounty((Creature)collector, player, true);
        }

        [CommandHandler("reload-all-landblocks", AccessLevel.Admin, CommandHandlerFlag.None, 0, "Reloads all landblocks currently loaded.")]
        public static void HandleReloadAllLandblocks(Session session, params string[] parameters)
        {
            ActionChain lbResetChain = new ActionChain();
            var lbs = LandblockManager.GetLoadedLandblocks().Select(x => (id: x.Id, instance: x.Instance));
            var enumerator = lbs.GetEnumerator();

            ActionEventDelegate resetLandblockAction = null;
            resetLandblockAction = new ActionEventDelegate(() =>
            {
                if (!enumerator.MoveNext())
                    return;
                if (LandblockManager.IsLoaded(enumerator.Current.id, enumerator.Current.instance))
                {
                    var lb = LandblockManager.GetLandblockUnsafe(enumerator.Current.id, enumerator.Current.instance);
                    if (lb != null)
                    {
                        if (session?.Player?.CurrentLandblock != lb)
                            CommandHandlerHelper.WriteOutputInfo(session, $"Reloading 0x{lb.LongId:X16}", ChatMessageType.Broadcast);
                        lb.Reload();
                    }
                }
                lbResetChain.AddDelayForOneTick();
                lbResetChain.AddAction(WorldManager.ActionQueue, resetLandblockAction);
            });
            lbResetChain.AddAction(WorldManager.ActionQueue, resetLandblockAction);
            lbResetChain.EnqueueChain();
        }

        [CommandHandler("spl", AccessLevel.Developer, CommandHandlerFlag.None, 0, "Show player locations.")]
        [CommandHandler("show-player-locations", AccessLevel.Developer, CommandHandlerFlag.RequiresWorld, 0, "Show player locations.")]
        public static void HandleShowPlayerLocations(Session session, params string[] parameters)
        {

            foreach (var player in PlayerManager.GetAllOnline())
            {
                if (player != null && player.Location != null)
                {
                    RiftManager.TryGetActiveRift(player.HomeRealm, player.Location.LandblockHex, out Rift rift);
                    DungeonManager.TryGetDungeonLandblock(player.Location.LandblockHex, out DungeonLandblock dungeon);

                    var at = rift != null ? $"Rift {rift.Name}" : dungeon != null ? $"Dungeon {dungeon.Name}" : player.Location.GetMapCoordStr();
                    CommandHandlerHelper.WriteOutputInfo(session, $"Name = {player.Name}, At = {at}, RealmId = {player.Location.RealmID}, Instance = {player.Location.Instance} ", ChatMessageType.WorldBroadcast);
                }
            }
        }

        [CommandHandler("scarabs", AccessLevel.Player, CommandHandlerFlag.RequiresWorld, 0, "Add scarabs to inventory.")]
        public static void HandleAddScarabs(Session session, params string[] parameters)
        {
            if (!session.Player.CurrentLandblock.RealmRuleset.GetProperty(RealmPropertyBool.IsDuelingRealm))
                return;

            DuelRealmHelpers.AddScarabsToInventory(session.Player);
        }
    }
}
