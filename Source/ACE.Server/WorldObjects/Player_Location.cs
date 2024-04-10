using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;

using ACE.Common;
using ACE.Database;
using ACE.DatLoader;
using ACE.DatLoader.FileTypes;
using ACE.Entity;
using ACE.Entity.Enum;
using ACE.Entity.Enum.Properties;
using ACE.Entity.Models;
using ACE.Server.Entity;
using ACE.Server.Entity.Actions;
using ACE.Server.Network.GameEvent.Events;
using ACE.Server.Network.GameMessages.Messages;
using ACE.Server.Managers;
using ACE.Server.Realms;
using ACE.Server.Rifts;
using ACE.Server.HotDungeons;
using ACE.Server.HotDungeons.Managers;

namespace ACE.Server.WorldObjects
{
    partial class Player
    {
        private static readonly Position MarketplaceDrop = DatabaseManager.World.GetCachedWeenie("portalmarketplace")?.GetPosition(PositionType.Destination) ?? new Position(0x016C01BC, 49.206f, -31.935f, 0.005f, 0, 0, -0.707107f, 0.707107f, 0);

        private uint HideoutInstanceId
        {
            get
            {
                //TODO: Support account IDs > 65535
                var realm = RealmManager.GetReservedRealm(ReservedRealm.hideout);
                return Position.InstanceIDFromVars(realm.Realm.Id, (ushort)Account.AccountId, false);
            }
        }

        public Position HideoutLocation => UlgrimsHideout;
        private Position UlgrimsHideout
        {
            get { return new Position(0x7308001Fu, 80f, 163.4f, 12.004999f, 0f, 0f, 0.4475889f, 0.8942394f, HideoutInstanceId); }
        }
        
        public bool DebugLoc { get; set; }

        /// <summary>
        /// Teleports the player to position
        /// </summary>
        /// <param name="positionType">PositionType to be teleported to</param>
        /// <returns>true on success (position is set) false otherwise</returns>
        public bool TeleToPosition(PositionType positionType)
        {
            var position = GetPosition(positionType);

            if (position != null)
            {
                var teleportDest = new Position(position);
                // not sure but maybe this should only be checked if instance is 0?
                if (teleportDest.Instance == 0)
                    teleportDest.SetToDefaultRealmInstance(Location.RealmID);
                AdjustDungeon(teleportDest);

                Teleport(teleportDest);
                return true;
            }

            return false;
        }

        private static readonly Motion motionLifestoneRecall = new Motion(MotionStance.NonCombat, MotionCommand.LifestoneRecall);

        private static readonly Motion motionHouseRecall = new Motion(MotionStance.NonCombat, MotionCommand.HouseRecall);

        public static float RecallMoveThreshold = 8.0f;
        public static float RecallMoveThresholdSq = RecallMoveThreshold * RecallMoveThreshold;

        public bool TooBusyToRecall
        {
            get => IsBusy || suicideInProgress;     // recalls could be started from portal space?
        }

        public void HandleActionTeleToHouse()
        {
            var recallsDisabled = !RealmRuleset.GetProperty(RealmPropertyBool.HasRecalls);
            if (recallsDisabled)
                return;


            if (IsOlthoiPlayer)
            {
                Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.OlthoiCanOnlyRecallToLifestone));
                return;
            }

            if (PKTimerActive)
            {
                Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.YouHaveBeenInPKBattleTooRecently));
                return;
            }

            if (RecallsDisabled)
            {
                Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.ExitTrainingAcademyToUseCommand));
                return;
            }

            if (TooBusyToRecall)
            {
                Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.YoureTooBusy));
                return;
            }

            var house = House ?? GetAccountHouse();

            if (house == null)
            {
                Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.YouMustOwnHouseToUseCommand));
                return;
            }

            if (CombatMode != CombatMode.NonCombat)
            {
                // this should be handled by a different thing, probably a function that forces player into peacemode
                var updateCombatMode = new GameMessagePrivateUpdatePropertyInt(this, PropertyInt.CombatMode, (int)CombatMode.NonCombat);
                SetCombatMode(CombatMode.NonCombat);
                Session.Network.EnqueueSend(updateCombatMode);
            }

            EnqueueBroadcast(new GameMessageSystemChat($"{Name} is recalling home.", ChatMessageType.Recall), LocalBroadcastRange, ChatMessageType.Recall);

            SendMotionAsCommands(MotionCommand.HouseRecall, MotionStance.NonCombat);

            var startPos = new Position(Location);

            // Wait for animation
            var actionChain = new ActionChain();

            // Then do teleport
            var animLength = DatManager.PortalDat.ReadFromDat<MotionTable>(MotionTableId).GetAnimationLength(MotionCommand.HouseRecall);
            actionChain.AddDelaySeconds(animLength);
            IsBusy = true;
            actionChain.AddAction(this, () =>
            {
                IsBusy = false;
                var endPos = new Position(Location);
                if (startPos.SquaredDistanceTo(endPos) > RecallMoveThresholdSq)
                {
                    Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.YouHaveMovedTooFar));
                    return;
                }
                var instance = Location.Instance;
                var newLocation = new Position(house.SlumLord.Location);
                newLocation.Instance = instance;
                Teleport(newLocation);
            });

            actionChain.EnqueueChain();
        }

        public void HandleActionTeleToHideout()
        {
            var recallsDisabled = !RealmRuleset.GetProperty(RealmPropertyBool.HasRecalls);
            if (recallsDisabled)
                return;

            if (PKTimerActive)
            {
                Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.YouHaveBeenInPKBattleTooRecently));
                return;
            }

            if (RecallsDisabled)
            {
                Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.ExitTrainingAcademyToUseCommand));
                return;
            }

            if (TooBusyToRecall)
            {
                Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.YoureTooBusy));
                return;
            }

            if (CombatMode != CombatMode.NonCombat)
            {
                // this should be handled by a different thing, probably a function that forces player into peacemode
                var updateCombatMode = new GameMessagePrivateUpdatePropertyInt(this, PropertyInt.CombatMode, (int)CombatMode.NonCombat);
                SetCombatMode(CombatMode.NonCombat);
                Session.Network.EnqueueSend(updateCombatMode);
            }

            EnqueueBroadcast(new GameMessageSystemChat($"{Name} is recalling to the hideout.", ChatMessageType.Recall), LocalBroadcastRange, ChatMessageType.Recall);

            SendMotionAsCommands(MotionCommand.HouseRecall, MotionStance.NonCombat);

            var startPos = new Position(Location);

            // Wait for animation
            var actionChain = new ActionChain();

            // Then do teleport
            var animLength = DatManager.PortalDat.ReadFromDat<MotionTable>(MotionTableId).GetAnimationLength(MotionCommand.HouseRecall);
            actionChain.AddDelaySeconds(animLength);
            IsBusy = true;
            actionChain.AddAction(this, () =>
            {
                IsBusy = false;
                var endPos = new Position(Location);
                if (startPos.SquaredDistanceTo(endPos) > RecallMoveThresholdSq)
                {
                    Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.YouHaveMovedTooFar));
                    return;
                }
                TeleportToHideout();
            });

            actionChain.EnqueueChain();
        }

        /// <summary>
        /// Handles teleporting a player to the lifestone (/ls or /lifestone command)
        /// </summary>
        public void HandleActionTeleToLifestone()
        {
            var recallsDisabled = !RealmRuleset.GetProperty(RealmPropertyBool.HasRecalls);
            if (recallsDisabled)
                return;

            if (PKTimerActive)
            {
                Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.YouHaveBeenInPKBattleTooRecently));
                return;
            }

            if (RecallsDisabled)
            {
                Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.ExitTrainingAcademyToUseCommand));
                return;
            }

            if (TooBusyToRecall)
            {
                Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.YoureTooBusy));
                return;
            }

            if (Sanctuary == null)
            {
                Session.Network.EnqueueSend(new GameMessageSystemChat("Your spirit has not been attuned to a sanctuary location.", ChatMessageType.Broadcast));
                return;
            }

            // FIXME(ddevec): I should probably make a better interface for this
            UpdateVital(Mana, Mana.Current / 2);

            if (CombatMode != CombatMode.NonCombat)
            {
                // this should be handled by a different thing, probably a function that forces player into peacemode
                var updateCombatMode = new GameMessagePrivateUpdatePropertyInt(this, PropertyInt.CombatMode, (int)CombatMode.NonCombat);
                SetCombatMode(CombatMode.NonCombat);
                Session.Network.EnqueueSend(updateCombatMode);
            }

            EnqueueBroadcast(new GameMessageSystemChat($"{Name} is recalling to the lifestone.", ChatMessageType.Recall), LocalBroadcastRange, ChatMessageType.Recall);

            SendMotionAsCommands(MotionCommand.LifestoneRecall, MotionStance.NonCombat);

            var startPos = new Position(Location);

            // Wait for animation
            ActionChain lifestoneChain = new ActionChain();

            // Then do teleport
            IsBusy = true;
            lifestoneChain.AddDelaySeconds(DatManager.PortalDat.ReadFromDat<MotionTable>(MotionTableId).GetAnimationLength(MotionCommand.LifestoneRecall));
            lifestoneChain.AddAction(this, () =>
            {
                IsBusy = false;
                var endPos = new Position(Location);
                if (startPos.SquaredDistanceTo(endPos) > RecallMoveThresholdSq)
                {
                    Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.YouHaveMovedTooFar));
                    return;
                }

                Teleport(Sanctuary);
            });

            lifestoneChain.EnqueueChain();
        }

        private static readonly Motion motionMarketplaceRecall = new Motion(MotionStance.NonCombat, MotionCommand.MarketplaceRecall);

        public void HandleActionTeleToMarketPlace()
        {
            var recallsDisabled = !RealmRuleset.GetProperty(RealmPropertyBool.HasRecalls);
            if (recallsDisabled)
                return;
            if (IsOlthoiPlayer)
            {
                Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.OlthoiCanOnlyRecallToLifestone));
                return;
            }

            if (PKTimerActive)
            {
                Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.YouHaveBeenInPKBattleTooRecently));
                return;
            }

            if (RecallsDisabled)
            {
                Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.ExitTrainingAcademyToUseCommand));
                return;
            }

            if (TooBusyToRecall)
            {
                Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.YoureTooBusy));
                return;
            }

            if (CombatMode != CombatMode.NonCombat)
            {
                // this should be handled by a different thing, probably a function that forces player into peacemode
                var updateCombatMode = new GameMessagePrivateUpdatePropertyInt(this, PropertyInt.CombatMode, (int)CombatMode.NonCombat);
                SetCombatMode(CombatMode.NonCombat);
                Session.Network.EnqueueSend(updateCombatMode);
            }

            EnqueueBroadcast(new GameMessageSystemChat($"{Name} is recalling to the marketplace.", ChatMessageType.Recall), LocalBroadcastRange, ChatMessageType.Recall);

            SendMotionAsCommands(MotionCommand.MarketplaceRecall, MotionStance.NonCombat);

            var startPos = new Position(Location);

            // TODO: (OptimShi): Actual animation length is longer than in retail. 18.4s
            // float mpAnimationLength = MotionTable.GetAnimationLength((uint)MotionTableId, MotionCommand.MarketplaceRecall);
            // mpChain.AddDelaySeconds(mpAnimationLength);
            ActionChain mpChain = new ActionChain();
            mpChain.AddDelaySeconds(14);

            // Then do teleport
            IsBusy = true;
            mpChain.AddAction(this, () =>
            {
                IsBusy = false;
                var endPos = new Position(Location);
                if (startPos.SquaredDistanceTo(endPos) > RecallMoveThresholdSq)
                {
                    Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.YouHaveMovedTooFar));
                    return;
                }

                Teleport(MarketplaceDrop);
            });

            // Set the chain to run
            mpChain.EnqueueChain();
        }

        private static readonly Motion motionAllegianceHometownRecall = new Motion(MotionStance.NonCombat, MotionCommand.AllegianceHometownRecall);

        public void HandleActionRecallAllegianceHometown()
        {
            var recallsDisabled = !RealmRuleset.GetProperty(RealmPropertyBool.HasRecalls);
            if (recallsDisabled)
                return;
            //Console.WriteLine($"{Name}.HandleActionRecallAllegianceHometown()");

            if (IsOlthoiPlayer)
            {
                Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.OlthoiCanOnlyRecallToLifestone));
                return;
            }

            if (PKTimerActive)
            {
                Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.YouHaveBeenInPKBattleTooRecently));
                return;
            }

            if (RecallsDisabled)
            {
                Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.ExitTrainingAcademyToUseCommand));
                return;
            }

            if (TooBusyToRecall)
            {
                Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.YoureTooBusy));
                return;
            }

            // check if player is in an allegiance
            if (!VerifyRecallAllegianceHometown())
                return;

            if (CombatMode != CombatMode.NonCombat)
            {
                // this should be handled by a different thing, probably a function that forces player into peacemode
                var updateCombatMode = new GameMessagePrivateUpdatePropertyInt(this, PropertyInt.CombatMode, (int)CombatMode.NonCombat);
                SetCombatMode(CombatMode.NonCombat);
                Session.Network.EnqueueSend(updateCombatMode);
            }

            EnqueueBroadcast(new GameMessageSystemChat($"{Name} is going to the Allegiance hometown.", ChatMessageType.Recall), LocalBroadcastRange, ChatMessageType.Recall);

            SendMotionAsCommands(MotionCommand.AllegianceHometownRecall, MotionStance.NonCombat);

            var startPos = new Position(Location);

            // Wait for animation
            var actionChain = new ActionChain();

            // Then do teleport
            IsBusy = true;
            var animLength = DatManager.PortalDat.ReadFromDat<MotionTable>(MotionTableId).GetAnimationLength(MotionCommand.AllegianceHometownRecall);
            actionChain.AddDelaySeconds(animLength);
            actionChain.AddAction(this, () =>
            {
                IsBusy = false;
                var endPos = new Position(Location);
                if (startPos.SquaredDistanceTo(endPos) > RecallMoveThresholdSq)
                {
                    Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.YouHaveMovedTooFar));
                    return;
                }

                // re-verify
                if (!VerifyRecallAllegianceHometown())
                    return;

                Teleport(Allegiance.Sanctuary);
            });

            actionChain.EnqueueChain();
        }

        private bool VerifyRecallAllegianceHometown()
        {
            if (Allegiance == null)
            {
                Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.YouAreNotInAllegiance));
                return false;
            }

            if (Allegiance.Sanctuary == null)
            {
                Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.YourAllegianceDoesNotHaveHometown));
                return false;
            }

            return true;
        }

        /// <summary>
        /// Recalls you to your allegiance's Mansion or Villa
        /// </summary>
        public void HandleActionTeleToMansion()
        {
            var recallsDisabled = !RealmRuleset.GetProperty(RealmPropertyBool.HasRecalls);
            if (recallsDisabled)
                return;
            //Console.WriteLine($"{Name}.HandleActionTeleToMansion()");

            if (IsOlthoiPlayer)
            {
                Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.OlthoiCanOnlyRecallToLifestone));
                return;
            }

            if (PKTimerActive)
            {
                Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.YouHaveBeenInPKBattleTooRecently));
                return;
            }

            if (RecallsDisabled)
            {
                Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.ExitTrainingAcademyToUseCommand));
                return;
            }

            if (TooBusyToRecall)
            {
                Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.YoureTooBusy));
                return;
            }

            var allegianceHouse = VerifyTeleToMansion();

            if (allegianceHouse == null)
                return;

            if (CombatMode != CombatMode.NonCombat)
            {
                // this should be handled by a different thing, probably a function that forces player into peacemode
                var updateCombatMode = new GameMessagePrivateUpdatePropertyInt(this, PropertyInt.CombatMode, (int)CombatMode.NonCombat);
                SetCombatMode(CombatMode.NonCombat);
                Session.Network.EnqueueSend(updateCombatMode);
            }

            EnqueueBroadcast(new GameMessageSystemChat($"{Name} is recalling to the Allegiance housing.", ChatMessageType.Recall), LocalBroadcastRange, ChatMessageType.Recall);

            SendMotionAsCommands(MotionCommand.HouseRecall, MotionStance.NonCombat);

            var startPos = new Position(Location);

            // Wait for animation
            var actionChain = new ActionChain();

            // Then do teleport
            var animLength = DatManager.PortalDat.ReadFromDat<MotionTable>(MotionTableId).GetAnimationLength(MotionCommand.HouseRecall);
            actionChain.AddDelaySeconds(animLength);

            IsBusy = true;
            actionChain.AddAction(this, () =>
            {
                IsBusy = false;
                var endPos = new Position(Location);
                if (startPos.SquaredDistanceTo(endPos) > RecallMoveThresholdSq)
                {
                    Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.YouHaveMovedTooFar));
                    return;
                }

                // re-verify
                allegianceHouse = VerifyTeleToMansion();

                if (allegianceHouse == null)
                    return;

                Teleport(allegianceHouse.SlumLord.Location);
            }); 

            actionChain.EnqueueChain();
        }

        private House VerifyTeleToMansion()
        {
            // check if player is in an allegiance
            if (Allegiance == null)
            {
                Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.YouAreNotInAllegiance));
                return null;
            }

            var allegianceHouse = Allegiance.GetHouse(Location);

            if (allegianceHouse == null)
            {
                Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.YourMonarchDoesNotOwnAMansionOrVilla));
                return null;
            }

            if (allegianceHouse.HouseType < HouseType.Villa)
            {
                Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.YourMonarchsHouseIsNotAMansionOrVilla));
                return null;
            }

            // ensure allegiance housing has allegiance permissions enabled
            if (allegianceHouse.MonarchId == null)
            {
                Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.YourMonarchHasClosedTheMansion));
                return null;
            }

            return allegianceHouse;
        }

        private static readonly Motion motionPkArenaRecall = new Motion(MotionStance.NonCombat, MotionCommand.PKArenaRecall);

        private static List<Position> pkArenaLocs = new List<Position>()
        {
            new Position(DatabaseManager.World.GetCachedWeenie("portalpkarenanew1")?.GetPosition(PositionType.Destination) ?? new Position(0x00660117, 30, -50, 0.005f, 0, 0,  0.000000f,  1.000000f, 0)),
            new Position(DatabaseManager.World.GetCachedWeenie("portalpkarenanew2")?.GetPosition(PositionType.Destination) ?? new Position(0x00660106, 10,   0, 0.005f, 0, 0, -0.947071f,  0.321023f, 0)),
            new Position(DatabaseManager.World.GetCachedWeenie("portalpkarenanew3")?.GetPosition(PositionType.Destination) ?? new Position(0x00660103, 30, -30, 0.005f, 0, 0, -0.699713f,  0.714424f, 0)),
            new Position(DatabaseManager.World.GetCachedWeenie("portalpkarenanew4")?.GetPosition(PositionType.Destination) ?? new Position(0x0066011E, 50,   0, 0.005f, 0, 0, -0.961021f, -0.276474f, 0)),
            new Position(DatabaseManager.World.GetCachedWeenie("portalpkarenanew5")?.GetPosition(PositionType.Destination) ?? new Position(0x00660127, 60, -30, 0.005f, 0, 0,  0.681639f,  0.731689f, 0)),
        };

        public void HandleActionTeleToPkArena()
        {
            var recallsDisabled = !RealmRuleset.GetProperty(RealmPropertyBool.HasRecalls);
            if (recallsDisabled)
                return;
            //Console.WriteLine($"{Name}.HandleActionTeleToPkArena()");

            if (PlayerKillerStatus != PlayerKillerStatus.PK)
            {
                Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.OnlyPKsMayUseCommand));
                return;
            }

            if (PKTimerActive)
            {
                Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.YouHaveBeenInPKBattleTooRecently));
                return;
            }

            if (RecallsDisabled)
            {
                Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.ExitTrainingAcademyToUseCommand));
                return;
            }

            if (TooBusyToRecall)
            {
                Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.YoureTooBusy));
                return;
            }

            if (CombatMode != CombatMode.NonCombat)
            {
                // this should be handled by a different thing, probably a function that forces player into peacemode
                var updateCombatMode = new GameMessagePrivateUpdatePropertyInt(this, PropertyInt.CombatMode, (int)CombatMode.NonCombat);
                SetCombatMode(CombatMode.NonCombat);
                Session.Network.EnqueueSend(updateCombatMode);
            }

            EnqueueBroadcast(new GameMessageSystemChat($"{Name} is going to the PK Arena.", ChatMessageType.Recall), LocalBroadcastRange, ChatMessageType.Recall);

            SendMotionAsCommands(MotionCommand.PKArenaRecall, MotionStance.NonCombat);

            var startPos = new Position(Location);

            // Wait for animation
            var actionChain = new ActionChain();

            // Then do teleport
            var animLength = DatManager.PortalDat.ReadFromDat<MotionTable>(MotionTableId).GetAnimationLength(MotionCommand.PKArenaRecall);
            actionChain.AddDelaySeconds(animLength);

            IsBusy = true;
            actionChain.AddAction(this, () =>
            {
                IsBusy = false;
                var endPos = new Position(Location);
                if (startPos.SquaredDistanceTo(endPos) > RecallMoveThresholdSq)
                {
                    Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.YouHaveMovedTooFar));
                    return;
                }

                var rng = ThreadSafeRandom.Next(0, pkArenaLocs.Count - 1);
                var loc = pkArenaLocs[rng];

                Teleport(loc);
            });

            actionChain.EnqueueChain();
        }

        private static List<Position> pklArenaLocs = new List<Position>()
        {
            new Position(DatabaseManager.World.GetCachedWeenie("portalpklarenanew1")?.GetPosition(PositionType.Destination) ?? new Position(0x00670117, 30, -50, 0.005f, 0, 0,  0.000000f,  1.000000f, 0)),
            new Position(DatabaseManager.World.GetCachedWeenie("portalpklarenanew2")?.GetPosition(PositionType.Destination) ?? new Position(0x00670106, 10,   0, 0.005f, 0, 0, -0.947071f,  0.321023f, 0)),
            new Position(DatabaseManager.World.GetCachedWeenie("portalpklarenanew3")?.GetPosition(PositionType.Destination) ?? new Position(0x00670103, 30, -30, 0.005f, 0, 0, -0.699713f,  0.714424f, 0)),
            new Position(DatabaseManager.World.GetCachedWeenie("portalpklarenanew4")?.GetPosition(PositionType.Destination) ?? new Position(0x0067011E, 50,   0, 0.005f, 0, 0, -0.961021f, -0.276474f, 0)),
            new Position(DatabaseManager.World.GetCachedWeenie("portalpklarenanew5")?.GetPosition(PositionType.Destination) ?? new Position(0x00670127, 60, -30, 0.005f, 0, 0,  0.681639f,  0.731689f, 0)),
        };

        public void HandleActionTeleToPklArena()
        {
            var recallsDisabled = !RealmRuleset.GetProperty(RealmPropertyBool.HasRecalls);
            if (recallsDisabled)
                return;
            //Console.WriteLine($"{Name}.HandleActionTeleToPkLiteArena()");

            if (IsOlthoiPlayer)
            {
                Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.OlthoiCanOnlyRecallToLifestone));
                return;
            }

            if (PlayerKillerStatus != PlayerKillerStatus.PKLite)
            {
                Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.OnlyPKLiteMayUseCommand));
                return;
            }

            if (PKTimerActive)
            {
                Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.YouHaveBeenInPKBattleTooRecently));
                return;
            }

            if (RecallsDisabled)
            {
                Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.ExitTrainingAcademyToUseCommand));
                return;
            }

            if (TooBusyToRecall)
            {
                Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.YoureTooBusy));
                return;
            }

            if (CombatMode != CombatMode.NonCombat)
            {
                // this should be handled by a different thing, probably a function that forces player into peacemode
                var updateCombatMode = new GameMessagePrivateUpdatePropertyInt(this, PropertyInt.CombatMode, (int)CombatMode.NonCombat);
                SetCombatMode(CombatMode.NonCombat);
                Session.Network.EnqueueSend(updateCombatMode);
            }

            EnqueueBroadcast(new GameMessageSystemChat($"{Name} is going to the PKL Arena.", ChatMessageType.Recall), LocalBroadcastRange, ChatMessageType.Recall);

            SendMotionAsCommands(MotionCommand.PKArenaRecall, MotionStance.NonCombat);

            var startPos = new Position(Location);

            // Wait for animation
            var actionChain = new ActionChain();

            // Then do teleport
            var animLength = DatManager.PortalDat.ReadFromDat<MotionTable>(MotionTableId).GetAnimationLength(MotionCommand.PKArenaRecall);
            actionChain.AddDelaySeconds(animLength);

            IsBusy = true;
            actionChain.AddAction(this, () =>
            {
                IsBusy = false;
                var endPos = new Position(Location);
                if (startPos.SquaredDistanceTo(endPos) > RecallMoveThresholdSq)
                {
                    Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.YouHaveMovedTooFar));
                    return;
                }

                var rng = ThreadSafeRandom.Next(0, pklArenaLocs.Count - 1);
                var loc = pklArenaLocs[rng];

                Teleport(loc);
            });

            actionChain.EnqueueChain();
        }

        public void SendMotionAsCommands(MotionCommand motionCommand, MotionStance motionStance)
        {
            if (FastTick)
            {
                var actionChain = new ActionChain();
                EnqueueMotionAction(actionChain, new List<MotionCommand>() { motionCommand }, 1.0f, motionStance);
                actionChain.EnqueueChain();
            }
            else
            {
                var motion = new Motion(motionStance, MotionCommand.Ready);
                motion.MotionState.AddCommand(this, motionCommand);
                EnqueueBroadcastMotion(motion);
            }
        }

        public DateTime LastTeleportTime;

        /// <summary>
        /// This is not thread-safe. Consider using WorldManager.ThreadSafeTeleport() instead if you're calling this from a multi-threaded subsection.
        /// </summary>
        public void Teleport(Position _newPosition, bool teleportingFromInstance = false, bool fromPortal = false)
        {
            var newLbRaw = _newPosition.LandblockId.Raw;
            var nextLb = $"{newLbRaw:X8}".Substring(0, 4);

            var currentLbRaw = Location.LandblockId.Raw;
            var currentLb = $"{currentLbRaw:X8}".Substring(0, 4);

            if (RiftManager.TryGetActiveRift(nextLb, out Rift nextActiveRift))
            {
                var instance = _newPosition.Instance;
                _newPosition = new Position(nextActiveRift.DropPosition);
                if (teleportingFromInstance)
                    _newPosition.Instance = instance;
            }

            if (_newPosition.Instance == 0)
            {
                log.Warn($"Trying to teleporting character: {Name} from instance {Location.Instance} replacing destination with current instance! logging destination!!");
                log.Info(_newPosition.ToString());
                _newPosition.Instance = Location.Instance;
            }

            var apartments = LandblockManager.apartmentLandblocks.Select(r => new LandblockId(r).Landblock);
            var lb = _newPosition.LandblockId.Landblock;
            if (apartments.Contains(lb))
            {
                _newPosition = new Position(HouseManager.PourApartmentLoc);
                _newPosition.Instance = Location.Instance; 
            }

            Position.ParseInstanceID(Location.Instance, out var isTemporaryRuleset, out ushort _a, out ushort _b);
            if (isTemporaryRuleset)
            {
                if (!teleportingFromInstance && ExitInstance())
                    return;
            }

            if (RealmManager.GetRealm(_newPosition.RealmID) == null)
            {
                Session.Network.EnqueueSend(new GameMessageSystemChat($"Error: Realm at destination location does not exist.", ChatMessageType.System));
                return;
            }
            if (!ValidatePlayerRealmPosition(_newPosition))
            {
                if (IsAdmin)
                {
                    Session.Network.EnqueueSend(new GameMessageSystemChat($"Admin bypassing realm restriction.", ChatMessageType.System));
                }
                else
                { 
                    Session.Network.EnqueueSend(new GameMessageSystemChat($"Unable to teleport to that realm.", ChatMessageType.System));
                    return;
                }
            }

            var newPosition = new Position(_newPosition);
            //newPosition.PositionZ += 0.005f;
            newPosition.PositionZ += 0.005f * (ObjScale ?? 1.0f);

            if (_newPosition.Instance != Location.Instance)
            {
                if (!OnTransitionToNewRealm(Location.RealmID, _newPosition.RealmID, newPosition))
                    return;
            }

            //Console.WriteLine($"{Name}.Teleport() - Sending to {newPosition.ToLOCString()}");

            // Check currentFogColor set for player. If LandblockManager.GlobalFogColor is set, don't bother checking, dungeons didn't clear like this on retail worlds.
            // if not clear, reset to clear before portaling in case portaling to dungeon (no current way to fast check unloaded landblock for IsDungeon or current FogColor)
            // client doesn't respond to any change inside dungeons, and only queues for change if in dungeon, executing change upon next teleport
            // so if we delay teleport long enough to ensure clear arrives before teleport, we don't get fog carrying over into dungeon.

            if (currentFogColor.HasValue && currentFogColor != EnvironChangeType.Clear && !LandblockManager.GlobalFogColor.HasValue)
            {
                var delayTelport = new ActionChain();
                delayTelport.AddAction(this, () => ClearFogColor());
                delayTelport.AddDelaySeconds(1);
                delayTelport.AddAction(this, () => WorldManager.ThreadSafeTeleport(this, _newPosition, teleportingFromInstance));

                delayTelport.EnqueueChain();

                return;
            }

            Teleporting = true;
            LastTeleportTime = DateTime.UtcNow;
            LastTeleportStartTimestamp = Time.GetUnixTime();

            if (fromPortal)
                LastPortalTeleportTimestamp = LastTeleportStartTimestamp;

            Session.Network.EnqueueSend(new GameMessagePlayerTeleport(this));

            // load quickly, but player can load into landblock before server is finished loading

            // send a "fake" update position to get the client to start loading asap,
            // also might fix some decal bugs
            var prevLoc = Location;
            Location = newPosition;
            SendUpdatePosition();
            Location = prevLoc;

            DoTeleportPhysicsStateChanges();

            // force out of hotspots
            PhysicsObj.report_collision_end(true);

            if (UnderLifestoneProtection)
                LifestoneProtectionDispel();

            HandlePreTeleportVisibility(newPosition);

            UpdatePlayerPosition(new Position(newPosition), true);
        }

        // Assumes instance is loaded! Do not call directly
        private bool OnTransitionToNewRealm(ushort prevRealmId, ushort newRealmId, Position newLocation)
        {
            var prevrealm = RealmManager.GetRealm(prevRealmId);
            var newRealm = RealmManager.GetRealm(newRealmId);

            if (newLocation.IsEphemeralRealm && !Location.IsEphemeralRealm)
            {
                SetPosition(PositionType.EphemeralRealmExitTo, new Position(Location.InFrontOf(-5f)));
                SetPosition(PositionType.EphemeralRealmLastEnteredDrop, new Position(newLocation));
            }
            else if (!newLocation.IsEphemeralRealm)
            {
                SetPosition(PositionType.EphemeralRealmExitTo, null);
                SetPosition(PositionType.EphemeralRealmLastEnteredDrop, null);
            }

            var pk = false;
            if (newLocation.IsEphemeralRealm)
            {
                var lb = LandblockManager.GetLandblockUnsafe(newLocation.LandblockId, newLocation.Instance);
                if (lb.RealmHelpers.IsDuel || lb.RealmHelpers.IsPkOnly)
                    pk = true;
            }

            if (newRealm.StandardRules.GetProperty(RealmPropertyBool.IsPKOnly))
                pk = true;

            PlayerKillerStatus = pk ? PlayerKillerStatus.PK : PlayerKillerStatus.NPK;
            EnqueueBroadcast(new GameMessagePublicUpdatePropertyInt(this, PropertyInt.PlayerKillerStatus, (int)PlayerKillerStatus));

            if (newLocation.IsEphemeralRealm)
                Session.Network.EnqueueSend(new GameMessageSystemChat($"Entering ephemeral instance. Type /realm-info to view realm properties.", ChatMessageType.System));
            else if (Location.IsEphemeralRealm && !newLocation.IsEphemeralRealm)
                Session.Network.EnqueueSend(new GameMessageSystemChat($"Leaving instance and returning to realm {newRealm.Realm.Name}.", ChatMessageType.System));
            else
            {
                if (prevrealm.Realm.Id != HomeRealm)
                {
                    if (newRealm == RealmManager.ServerBaseRealm && prevrealm.GetDefaultInstanceID(this) == Account.AccountId)
                        Session.Network.EnqueueSend(new GameMessageSystemChat($"You have chosen {newRealm.Realm.Name} as your home realm.", ChatMessageType.System));
                    else
                        Session.Network.EnqueueSend(new GameMessageSystemChat($"You are temporarily leaving your home realm.", ChatMessageType.System));
                }
                else if (newRealm.Realm.Id == HomeRealm)
                    Session.Network.EnqueueSend(new GameMessageSystemChat($"Returning to home realm.", ChatMessageType.System));
                else
                    Session.Network.EnqueueSend(new GameMessageSystemChat($"Switching from realm {prevrealm.Realm.Name} to {newRealm.Realm.Name}.", ChatMessageType.System));
            }
            return true;
        }

        public ushort HomeRealm
        {
            get
            {
                int intid = GetProperty(PropertyInt.HomeRealm) ?? 0;
                if ((intid < 0) || (uint)intid > 0x7FFF)
                {
                    log.Error("Player " + Name + " HomeRealm out of range.");
                    return 0;
                }
                return (ushort)intid;
            }
            set
            {
                if (value == 0)
                {
                    RemoveProperty(PropertyInt.HomeRealm);
                    return;
                }
                if (value > 0x7FFF)
                {
                    log.Error("Cannot set HomeRealm for Player " + Name + ". Must be between 0 and 32767");
                    return;
                }
                SetProperty(PropertyInt.HomeRealm, value);
            }
        }

        public void ValidateCurrentRealm()
        {
            if (IsAdmin)
                return;
            if (!ValidatePlayerRealmPosition(Location))
                TeleportToHomeRealm();
        }

        public void TeleportToHomeRealm()
        {
            var homerealm = RealmManager.GetRealm(HomeRealm);
            if (homerealm == null)
                homerealm = RealmManager.GetRealm(0);

            var iid = homerealm.GetDefaultInstanceID(this);
            var pos = new Position(Home) { Instance = iid };
            Teleport(pos);
        }

        private void TeleportToHideout()
        {
            if (Account.AccountId > 0xFFFFu)
            {
                //TODO: Support account IDs > 65535
                Session.Network.EnqueueSend(new GameMessageSystemChat($"Unable to teleport to hideout.", ChatMessageType.System));
                return;
            }

            Teleport(HideoutLocation);
        }

        public bool ValidatePlayerRealmPosition(Position newPosition)
        {
            Position.ParseInstanceID(newPosition.Instance, out var isTemporaryRuleset, out ushort newRealmId, out ushort shortInstanceId);
            var homerealm = RealmManager.GetRealm(HomeRealm);
            var destrealm = RealmManager.GetRealm(newPosition.RealmID);
            if (destrealm == null)
                return false;
            if (RealmManager.TryParseReservedRealm(destrealm.Realm.Id, out var reservedRealm))
            {
                switch (reservedRealm)
                {
                    case ReservedRealm.@default:
                        if (homerealm.Realm.Id != (ushort)ReservedRealm.@default)
                            return false;
                        return shortInstanceId == Account.AccountId;
                    case ReservedRealm.hideout:
                        if (shortInstanceId != Account.AccountId)
                            return false;
                        if (!homerealm.StandardRules.GetProperty(RealmPropertyBool.HideoutEnabled))
                            return false;
                        return new ushort[] { 0x7308, 0x7309 }.Contains((ushort)newPosition.LandblockShort); //Ulgrims only, todo: add other landblocks
                    default:
                        return false;
                }
            }
            if (!destrealm.IsWhitelistedLandblock((ushort)newPosition.LandblockShort))
                return false;

            if (isTemporaryRuleset)
            {
                var lb = LandblockManager.GetLandblockUnsafe(newPosition.LandblockId, newPosition.Instance);
                if (lb?.InnerRealmInfo == null)
                    return false;
                /*if (lb.InnerRealmInfo.Owner == this)
                    return true;
                if (lb.InnerRealmInfo.AllowedPlayers.Contains(this))
                    return true;
                if (lb.InnerRealmInfo.OpenToFellowship)
                {
                    if (lb.InnerRealmInfo.Owner.Fellowship?.GetFellowshipMembers().Values.Contains(this) == true)
                        return true;
                }*/
                return true;
            }
            else
            {
                if (homerealm.StandardRules.GetProperty(RealmPropertyBool.CanInteractWithNeutralZone) == true &&
                    destrealm.StandardRules.GetProperty(RealmPropertyBool.IsNeutralZone) == true)
                    return true;
            }
            
            return homerealm.Realm.Id == destrealm.Realm.Id;
        }

        internal bool ExitInstance()
        {
            Position.ParseInstanceID(Location.Instance, out var isTemporaryRuleset, out ushort newRealmId, out ushort shortInstanceId);
            if (!isTemporaryRuleset)
            {
                Session.Network.EnqueueSend(new GameMessageSystemChat($"You are not in an instance!", ChatMessageType.System));
                return false;
            }
            var loc = GetPosition(PositionType.EphemeralRealmExitTo);
            if (loc == null || !ValidatePlayerRealmPosition(loc))
            {
                loc = GetPosition(PositionType.Sanctuary) ?? GetPosition(PositionType.Home);
                loc.Instance = Position.InstanceIDFromVars(HomeRealm, 0, false);
            }
            WorldManager.ThreadSafeTeleport(this, loc, true, new ActionEventDelegate(() =>
            {
                this.SetPosition(PositionType.EphemeralRealmExitTo, null);
                this.SetPosition(PositionType.EphemeralRealmLastEnteredDrop, null);
            }));
            return true;
        }

        public void DoPreTeleportHide()
        {
            if (Teleporting) return;
            PlayParticleEffect(PlayScript.Hide, Guid);
        }

        public void DoTeleportPhysicsStateChanges()
        {
            var broadcastUpdate = false;

            var oldHidden = Hidden.Value;
            var oldIgnore = IgnoreCollisions.Value;
            var oldReport = ReportCollisions.Value;

            Hidden = true;
            IgnoreCollisions = true;
            ReportCollisions = false;

            if (Hidden != oldHidden || IgnoreCollisions != oldIgnore || ReportCollisions != oldReport)
                broadcastUpdate = true;

            if (broadcastUpdate)
                EnqueueBroadcastPhysicsState();
        }

        /// <summary>
        /// Prevent message spam
        /// </summary>
        public double? LastPortalTeleportTimestampError;

        public void OnTeleportComplete()
        {
            if (CurrentLandblock != null && !CurrentLandblock.CreateWorldObjectsCompleted)
            {
                // If the critical landblock resources haven't been loaded yet, we keep the player in the pink bubble state
                // We'll check periodically to see when it's safe to let them materialize in
                var actionChain = new ActionChain();
                actionChain.AddDelaySeconds(0.1);
                actionChain.AddAction(this, OnTeleportComplete);
                actionChain.EnqueueChain();
                return;
            }

            // set materialize physics state
            // this takes the player from pink bubbles -> fully materialized
            if (CloakStatus != CloakStatus.On)
                ReportCollisions = true;

            IgnoreCollisions = false;
            Hidden = false;
            Teleporting = false;
            
            CheckMonsters();
            CheckHouse();

            EnqueueBroadcastPhysicsState();

            // hijacking this for both start/end on portal teleport
            if (LastTeleportStartTimestamp == LastPortalTeleportTimestamp)
                LastPortalTeleportTimestamp = Time.GetUnixTime();
        }

        public void SendTeleportedViaMagicMessage(WorldObject itemCaster, Spell spell)
        {
            if (itemCaster == null || itemCaster is Gem)
                Session.Network.EnqueueSend(new GameMessageSystemChat($"You have been teleported.", ChatMessageType.Magic));
            else if (this != itemCaster && !(itemCaster is Gem) && !(itemCaster is Switch) && !(itemCaster.GetProperty(PropertyBool.NpcInteractsSilently) ?? false))
                Session.Network.EnqueueSend(new GameMessageSystemChat($"{itemCaster.Name} teleports you with {spell.Name}.", ChatMessageType.Magic));
            //else if (itemCaster is Gem)
            //    Session.Network.EnqueueSend(new GameEventWeenieError(Session, WeenieError.ITeleported));
        }

        public void NotifyLandblocks()
        {
            // the original implementations of this were done on landblock heartbeat,
            // with checks for players in the current landblock, as well as adjacent outdoor landblocks

            // for performance reasons, this is being reimplemented in the reverse manner,
            // with players notifying landblocks of their activity

            // notify current landblock of player activity
            if (CurrentLandblock != null)
                CurrentLandblock?.SetActive();
        }

        public static readonly float RunFactor = 1.5f;

        /// <summary>
        /// Returns the amount of time for player to rotate by the # of degrees
        /// from the input angle, using the omega speed from its MotionTable
        /// </summary>
        public override float GetRotateDelay(float angle)
        {
            return base.GetRotateDelay(angle) / RunFactor;
        }

        /// <summary>
        /// A list of landblocks the player cannot relog directly into
        /// 
        /// If a regular player logs out in one of these landblocks,
        /// they will be transported back to the lifestone when they log back in.
        /// </summary>
        public static HashSet<ushort> NoLog_Landblocks = new HashSet<ushort>()
        {
            // https://asheron.fandom.com/wiki/Special:Search?query=Lifestone+on+Relog%3A+Yes+
            // https://docs.google.com/spreadsheets/d/122xOw3IKCezaTDjC_hggWSVzYJ_9M_zUUtGEXkwNXfs/edit#gid=846612575

            0x0002,     // Viamontian Garrison
            0x0007,     // Town Network
            0x0056,     // Augmentation Realm Main Level
            0x005F,     // Tanada House of Pancakes (Seasonal)
            0x0067,     // PKL Arena
            0x006D,     // Augmentation Realm Upper Level
            0x007D,     // Augmentation Realm Lower Level
            0x00AB,     // Derethian Combat Arena
            0x00AC,     // Derethian Combat Arena
            0x00C3,     // Blighted Putrid Moarsman Tunnels
            0x00D7,     // Jester's Prison
            0x00EA,     // Mhoire Armory
            0x015D,     // Mountain Cavern
            0x027F,     // East Fork Dam Hive
            0x03A7,     // Mount Elyrii Hive
            0x5764,     // Oubliette of Mhoire Castle
            0x634C,     // Tainted Grotto
            0x6544,     // Greater Battle Dungeon
            0x6651,     // Hoshino Tower
            0x7E04,     // Thug Hideout
            0x8A04,     // Night Club (Seasonal Anniversary)
            0x8B04,     // Frozen Wight Lair
            0x9EE5,     // Northwatch Castle Black Market
            0xB5F0,     // Aerfalle's Sanctum
            0xF92F,     // Freebooter Keep Black Market
            0x00B0,     // Colosseum Arena One
            0x00B1,     // Colosseum Arena Two
            0x00B2,     // Colosseum Arena Three
            0x00B3,     // Colosseum Arena Four
            0x00B4,     // Colosseum Arena Five
            0x00B6,     // Colosseum Arena Mini-Bosses
            0x5960,     // Gauntlet Arena One (Celestial Hand)
            0x5961,     // Gauntlet Arena Two (Celestial Hand)
            0x5962,     // Gauntlet Arena One (Eldritch Web)
            0x5963,     // Gauntlet Arena Two (Eldritch Web)
            0x5964,     // Gauntlet Arena One (Radiant Blood)
            0x5965,     // Gauntlet Arena Two (Radiant Blood)
        };

        /// <summary>
        /// Called when a player first logs in
        /// </summary>
        public static void HandleNoLogLandblock(Biota biota, out bool playerWasMovedFromNoLogLandblock)
        {
            playerWasMovedFromNoLogLandblock = false;

            if (biota.WeenieType == WeenieType.Sentinel || biota.WeenieType == WeenieType.Admin) return;

            if (!biota.PropertiesPosition.TryGetValue(PositionType.Location, out var location))
                return;

            var landblock = (ushort)(location.ObjCellId >> 16);

            if (!NoLog_Landblocks.Contains(landblock))
                return;

            if (!biota.PropertiesPosition.TryGetValue(PositionType.Sanctuary, out var lifestone))
                return;

            location.ObjCellId = lifestone.ObjCellId;
            location.PositionX = lifestone.PositionX;
            location.PositionY = lifestone.PositionY;
            location.PositionZ = lifestone.PositionZ;
            location.RotationX = lifestone.RotationX;
            location.RotationY = lifestone.RotationY;
            location.RotationZ = lifestone.RotationZ;
            location.RotationW = lifestone.RotationW;

            playerWasMovedFromNoLogLandblock = true;

            return;
        }
    }
}
