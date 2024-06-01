using ACE.Entity.Enum;
using ACE.Entity.Enum.Properties;
using ACE.Server.Factories;
using ACE.Server.Network.GameMessages.Messages;
using ACE.Server.WorldObjects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ACE.Database;
using ACE.Server.Entity.Actions;
using ACE.Server.Entity;
using ACE.Entity;
using ACE.Server.Managers;
using ACE.Server.Features.HotDungeons;
using ACE.Entity.Models;
using ACE.Server.Network;

namespace ACE.Server.Realms
{

    public class DuelDungeon
    {
        public InstancedPosition Destination { get; set; }
    }

    public static class DuelRealmHelpers
    {
        private static Dictionary<string, DuelDungeon> Dungeons = new Dictionary<string, DuelDungeon>();

        static void TeachAugmentations(Player player)
        {
            foreach(var augtype in RealmConstants.DuelAugmentations)
            {
                AugmentationDevice.DoAugmentation(player, augtype, null, false, false);
                player.SaveBiotaToDatabase();
            }
        }

        public static DuelDungeon GetDuelDungeon(InstancedPosition position)
        {
            var landblockHex = position.LandblockHex;
            if (Dungeons.ContainsKey(landblockHex))
                return Dungeons[landblockHex];

            if (!DungeonRepository.HasDungeon(landblockHex))
                return null;

            var rules = new List<Realm>()
            {
                RealmManager.GetRealm(1017).Realm
            };

            var lb = RealmManager.GetNewEphemeralLandblock(RealmManager.DuelRealm.Realm.Id, position.LandblockId, rules, true);
            var pos = new InstancedPosition(position, lb.Instance);
            var dungeon = new DuelDungeon()
            {
                Destination = pos
            };

            Dungeons.Add(landblockHex, dungeon);
            return dungeon;
        }

        static void GiveGear(Player player)
        {
            var gear = new List<string>()
            {
                "realm-duel-gear-1",
                "realm-duel-gear-2",
                "realm-duel-gear-3",
                "realm-duel-gear-4",
                "realm-duel-gear-5",
                "realm-duel-gear-6"
            }.Select(DatabaseManager.World.GetCachedWeenie)
            .Where(w => w != null)
            .Select(w => WorldObjectFactory.CreateNewWorldObject(w, null))
            .ToList();

            foreach(var item in gear)
            {
                player.TryCreateInInventoryWithNetworking(item);
            }
        }

        static void DisableSpellComponentRequirement(Player player)
        {
            player.SpellComponentsRequired = false;
            player.EnqueueBroadcast(new GameMessagePublicUpdatePropertyBool(player, PropertyBool.SpellComponentsRequired, player.SpellComponentsRequired));
            player.Session.Network.EnqueueSend(new GameMessageSystemChat("You can now cast spells without components.", ChatMessageType.Broadcast));
        }

        public static void SetupNewCharacter(Player player)
        {
            player.GrantXP((long)player.GetXPBetweenLevels(1, 275), XpType.Admin, ShareType.None);
            TeachAugmentations(player);
            SpendAllXp(player);
            GiveGear(player);
            AddScarabsToInventory(player);
            LearnAllNonAdminSpells(player);
            DisableSpellComponentRequirement(player);
        }

        private static void AddScarabsToInventory(Player player)
        {
            var weenieIds = new HashSet<int>() { 686, 687, 688, 689, 690, 691, 7299, 8897, 37155 };

            foreach (uint weenieId in weenieIds)
            {
                var loot = WorldObjectFactory.CreateNewWorldObject(weenieId);

                if (loot == null) 
                    continue;

                var stackSizeForThisWeenieId = loot.MaxStackSize;

                if (stackSizeForThisWeenieId > 1)
                    loot.SetStackSize(stackSizeForThisWeenieId);

                player.TryCreateInInventoryWithNetworking(loot);
            }
        }

        static void SpendAllXp(Player player)
        {
            player.SpendAllXp(true);
            player.Health.Current = player.Health.MaxValue;
            player.Stamina.Current = player.Stamina.MaxValue;
            player.Mana.Current = player.Mana.MaxValue;
        }


        static void LearnAllNonAdminSpells(Player player)
        {
            player.Session.Network.EnqueueSend(new GameMessageSystemChat($"Teaching all spells. There may be some lag for a few seconds.", ChatMessageType.System));

            var actionChain = new ActionChain();
            actionChain.AddDelaySeconds(1.0f);
            actionChain.AddAction(player, () =>
            {
                var spellTable = DatLoader.DatManager.PortalDat.SpellTable;

                foreach (var spellID in Player.AllNonAdminSpellTable)
                {
                    if (!spellTable.Spells.ContainsKey(spellID))
                    {
                        continue;
                    }
                    var spell = new Spell(spellID, false);
                    player.LearnSpellWithNetworking(spell.Id, false);
                }
            });
            actionChain.EnqueueChain();
        }

        public static InstancedPosition GetDuelingAreaDrop()
        {
            return GetDuelingAreaDrop(RealmManager.DuelRealm);
        }

        public static InstancedPosition GetDuelingAreaDrop(WorldRealm realm)
        {
            return new InstancedPosition(RealmConstants.DuelStagingAreaDrop, realm.StandardRules.GetDefaultInstanceID());
        }
    }
}
