using ACE.Common;
using ACE.Entity.Enum;
using ACE.Server.Entity;
using ACE.Server.Managers;
using ACE.Server.Realms;
using ACE.Server.WorldObjects;
using System;
using System.Collections.Generic;

namespace ACE.Server.Features.Rifts
{
  internal class TimedOutPlayer
    {
        public ulong Guid { get; set; }
        public DateTime TimeoutTimeStamp { get; set; }
    }

    public class Rift : DungeonBase
    {
        public InstancedPosition DropPosition = null;
        public double BonuxXp
        {
            get
            {
                var rules = LandblockInstance?.RealmRuleset;
                if (rules != null)
                    return rules.GetProperty(ACE.Entity.Enum.Properties.RealmPropertyFloat.ExperienceMultiplierAll);
                else
                    return 1.0;
            }
        }

        private Dictionary<ulong, TimedOutPlayer> TimedOutPlayers = new Dictionary<ulong, TimedOutPlayer>();

        public List<WorldObject> RiftPortals = new List<WorldObject>();

        public Rift Next = null;

        public Rift Previous = null;

        public Landblock LandblockInstance = null;

        public List<uint> CreatureIds = new List<uint>();

        public uint Tier = 1;

        public uint Instance { get; set; } = 0;

        public uint HomeInstance { get; set; } = 0;

        public void AddPlayerTimeout(ulong playerGuid)
        {
            TimedOutPlayers.Add(playerGuid, new TimedOutPlayer()
            {
                Guid = playerGuid,
                TimeoutTimeStamp = DateTime.UtcNow
            });
        }

        public Rift(string landblock, string name, string coords, InstancedPosition dropPosition, uint instance, uint homeInstance, Landblock ephemeralRealm, List<uint> creatureIds, uint tier) : base(landblock, name, coords)
        {
            Landblock = landblock;
            Name = name;
            Coords = coords;
            DropPosition = dropPosition;
            Instance = instance;
            HomeInstance = homeInstance;
            LandblockInstance = ephemeralRealm;
            CreatureIds = creatureIds;
            Tier = tier;
        }

        public uint GetRandomCreature()
        {
            if (CreatureIds.Count == 0)
                return 0;

            var randomIndex = ThreadSafeRandom.Next(0, CreatureIds.Count - 1);
            return CreatureIds[randomIndex];
        }


        public void Close()
        {
            foreach (var portal in RiftPortals)
            {
                portal.Destroy();
            }

            foreach (var player in Players.Values)
            {
                if (player != null)
                    player.ExitInstance();
            }

            Instance = 0;
            HomeInstance = 0;
            Next = null;
            Previous = null;
            LandblockInstance.Permaload = false;
            LandblockInstance = null;
            Players.Clear();
            RiftPortals.Clear();
            TimedOutPlayers.Clear();
        }

        internal bool ValidateTimedOutPlayer(Player player)
        {
            var guid = player.Guid.Full;
            if (TimedOutPlayers.ContainsKey(guid))
            {
                var timedOutPlayer = TimedOutPlayers[guid];
                var timeoutDuration = PropertyManager.GetLong("rift_death_duration").Item;
                if (DateTime.UtcNow - timedOutPlayer.TimeoutTimeStamp < TimeSpan.FromMinutes(timeoutDuration))
                {
                    var message = $"You have died to a pk too recently in {Name}, you must wait {Formatting.FormatTimeRemaining(timedOutPlayer.TimeoutTimeStamp.AddMinutes(timeoutDuration) - DateTime.UtcNow)}!";
                    player?.Session.Network.EnqueueSend(new Network.GameMessages.Messages.GameMessageSystemChat(message, ChatMessageType.System));
                    return false;
                }
                else
                {
                    TimedOutPlayers.Remove(guid);
                    return true;
                }
            }

            return true;
        }
    }
}
