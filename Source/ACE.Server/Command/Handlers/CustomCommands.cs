using ACE.Common;
using ACE.Entity;
using ACE.Entity.Enum;
using ACE.Entity.Enum.Properties;
using ACE.Server.Entity.Actions;
using ACE.Server.HotDungeons.Managers;
using ACE.Server.Managers;
using ACE.Server.Network;
using ACE.Server.Network.GameEvent.Events;
using ACE.Server.Network.GameMessages.Messages;
using ACE.Server.Realms;
using ACE.Server.Rifts;
using ACE.Server.Tar;
using ACE.Server.WorldObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACE.Server.Command.Handlers
{
    public static class CustomCommands
    {
        [CommandHandler("change-season", AccessLevel.Admin, CommandHandlerFlag.None, 1, "Updates the season for the server by providing a new realmId from `Content/json/realms.jsonc`", "change-season <realmId>")]
        public static void HandleChangeSeason(Session session, params string[] paramters)
        {
            try
            {
                var longVal = long.Parse(paramters[0]);

                var realm = RealmManager.GetRealm((ushort)longVal);

                if (realm == null)
                {
                    CommandHandlerHelper.WriteOutputInfo(session, $"RealmId: {longVal} does not exist, please provide a valid realmId from `Content/json/realms.jsonc`, type: /season-list for a list of available seasons to choose from.");
                    return;
                }

                if (longVal != RealmManager.ServerBaseRealm.Realm.Id)
                {
                     var modifyParams = new string[] { "server_base_realm", paramters[0] };
                    AdminCommands.HandleModifyServerLongProperty(session, modifyParams);
                    var message = $"The season has changed to {RealmManager.ServerBaseRealm.Realm.Name}";
                    PlayerManager.BroadcastToAuditChannel(session?.Player, message);
                    PlayerManager.BroadcastToAll(new GameMessageSystemChat(message, ChatMessageType.WorldBroadcast));
                } else

                    CommandHandlerHelper.WriteOutputInfo(session, $"RealmId: {longVal} is already applied, Please input a new RealmId", ChatMessageType.Help);
            }
            catch (Exception)
            {
                CommandHandlerHelper.WriteOutputInfo(session, "Please input a valid long", ChatMessageType.Help);
            }
        }

        [CommandHandler("season-info", AccessLevel.Player, CommandHandlerFlag.RequiresWorld, 0, "Get info about the current season your character belongs to.")]
        public static void HandleSeasonInfo(Session session, params string[] paramters)
        {
            var player  = session?.Player;
            var season = RealmManager.ServerBaseRealm;
            
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

        [CommandHandler("telerealm", AccessLevel.Developer, CommandHandlerFlag.RequiresWorld, 0, "Teleports the current player to another realm.")]
        public static void HandleMoveRealm(Session session, params string[] parameters)
        {
            if (parameters.Length < 1)
                return;
            if (!ushort.TryParse(parameters[0], out var realmid))
                return;

            var pos = session.Player.GetPosition(PositionType.Location);
            var newpos = new Position(pos);
            newpos.SetToDefaultRealmInstance(realmid);

            session.Player.Teleport(newpos);
            var positionMessage = new GameMessageSystemChat($"Teleporting to realm {realmid}.", ChatMessageType.Broadcast);
            session.Network.EnqueueSend(positionMessage);
        }

        [CommandHandler("realm-info", AccessLevel.Player, CommandHandlerFlag.RequiresWorld, "Lists all properties for the current realm.")]
        public static void HandleZoneInfo(Session session, params string[] parameters)
        {
            session.Network.EnqueueSend(new GameMessageSystemChat($"\n<Realm Information>", ChatMessageType.System));
            session.Network.EnqueueSend(new GameMessageSystemChat($"\n{session.Player.CurrentLandblock.RealmRuleset.Realm.Name} - Id: {session.Player.CurrentLandblock.RealmRuleset.Realm.Id} - Instance: {session.Player.CurrentLandblock.RealmRuleset.GetDefaultInstanceID()} ", ChatMessageType.System));
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

        [CommandHandler("dungeons", AccessLevel.Player, CommandHandlerFlag.None, 0, "Get a list of available dungeons.")]
        public static void HandleCheckDungeonsNew(Session session, params string[] paramters)
        {
            session.Network.EnqueueSend(new GameMessageSystemChat($"\n<Active Dungeon List>", ChatMessageType.System));
            foreach (var dungeon in DungeonManager.GetDungeons())
            {
                var at = dungeon.Coords.Length > 0 ? $"at {dungeon.Coords}" : "";
                var message = $"Dungeon {dungeon.Name} is active {at}, and has a an xp bonus of {dungeon.BonuxXp.ToString("0.00")}x";
                session.Network.EnqueueSend(new GameMessageSystemChat($"\n{message}", ChatMessageType.System));
            }

            session.Network.EnqueueSend(new GameMessageSystemChat($"\nTime Remaining before reset: {DungeonManager.FormatTimeRemaining(DungeonManager.DungeonsTimeRemaining)}", ChatMessageType.System));
        }

        [CommandHandler("dungeons-potential", AccessLevel.Developer, CommandHandlerFlag.None, 0, "Get a list of available potential dungeons.")]
        public static void HandleCheckDungeonsPotential(Session session, params string[] paramters)
        {
            session.Network.EnqueueSend(new GameMessageSystemChat($"\n<Active Potential Dungeon List>", ChatMessageType.System));
            foreach (var dungeon in DungeonManager.GetPotentialDungeons())
            {
                var at = dungeon.Coords.Length > 0 ? $"at {dungeon.Coords}" : "";
                var message = $"Dungeon {dungeon.Name} has potential {at}";
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

        /** Rifts Start **/

        [CommandHandler("active-rifts", AccessLevel.Player, CommandHandlerFlag.None, 0, "Get a list of available rifts.")]
        public static void HandleCheckRifts(Session session, params string[] paramters)
        {
            HandleCheckRiftsNew(session, paramters);
        }

        [CommandHandler("rifts", AccessLevel.Player, CommandHandlerFlag.None, 0, "Get a list of available rifts.")]
        public static void HandleCheckRiftsNew(Session session, params string[] paramters)
        {
            session.Network.EnqueueSend(new GameMessageSystemChat($"\n<Active Rift List>", ChatMessageType.System));
            foreach (var rift in RiftManager.ActiveRifts.Values.ToList())
            {
                var at = rift.Coords.Length > 0 ? $"at {rift.Coords}" : "";
                var message = $"Rift {rift.Name} is active {at}";
                session.Network.EnqueueSend(new GameMessageSystemChat($"\n{message}", ChatMessageType.System));
            }

            session.Network.EnqueueSend(new GameMessageSystemChat($"\nTime Remaining: {RiftManager.FormatTimeRemaining()}", ChatMessageType.System));
        }

        /** Rifts End **/

        /** Tar Start **/

        [CommandHandler("tar", AccessLevel.Player, CommandHandlerFlag.None, 0, "Get a list of available rifts.")]
        public static void HandleCheckTar(Session session, params string[] paramters)
        {
            var player = session.Player;


            var id = player.Location.LandblockId.Raw;
            var lb = $"{id:X8}".Substring(0, 4);

            if (RiftManager.HasActiveRift(lb))
            {
                var message = $"This landblock is a rift, it does not have tar";
                session.Network.EnqueueSend(new GameMessageSystemChat(message, ChatMessageType.System));
                return;
            }

            var tarLandblock = TarManager.GetTarLandblock(lb);

            if (player != null && tarLandblock != null)
            {
                if (tarLandblock.Active)
                {
                    var message = $"The current tar xp modifier for this landblock is {tarLandblock.TarXpModifier}, when this landblock reaches 0.1 and is inside a dungeon, it may be eligible to be upgraded to a Rift!";
                    session.Network.EnqueueSend(new GameMessageSystemChat(message, ChatMessageType.System));
                }
                else
                {
                    var message = $"The current tar xp modifier for this landblock is {tarLandblock.TarXpModifier}, it will be resetting in {TarManager.FormatTimeRemaining(tarLandblock.TimeRemaining)}";
                    session.Network.EnqueueSend(new GameMessageSystemChat(message, ChatMessageType.System));
                }

                var msg = $"The current time remaining before a Rift can possibly spawn is {TarManager.FormatTimeRemaining(tarLandblock.RiftTimeRemaining)}";
                session.Network.EnqueueSend(new GameMessageSystemChat(msg, ChatMessageType.System));


            }
            else
            {
                var message = $"This landblock has not been hunted yet!";
                session.Network.EnqueueSend(new GameMessageSystemChat(message, ChatMessageType.System));
            }
        }

        /** Tar End **/

    }
}
