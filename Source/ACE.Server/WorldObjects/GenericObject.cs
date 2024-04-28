using System;
using System.Collections.Generic;
using ACE.Entity;
using ACE.Entity.Enum.Properties;
using ACE.Entity.Models;
using ACE.Server.Network.GameMessages.Messages;
using ACE.Server.Realms;

namespace ACE.Server.WorldObjects
{
    public class GenericObject : WorldObject
    {
        /// <summary>
        /// A new biota be created taking all of its values from weenie.
        /// </summary>
        public GenericObject(Weenie weenie, ObjectGuid guid, Realms.AppliedRuleset ruleset) : base(weenie, guid)
        {
            MutateGenerator(ruleset);
            SetEphemeralValues();
        }

        private void MutateGenerator(AppliedRuleset ruleset)
        {
            if (ruleset.Realm.Id == 1016 && WeenieType == ACE.Entity.Enum.WeenieType.Generic)
            {
                var creatureRespawnDuration = ruleset.GetProperty(RealmPropertyFloat.CreatureRespawnDuration);
                var creatureSpawnRateMultiplier = ruleset.GetProperty(RealmPropertyFloat.CreatureSpawnRateMultiplier);

                if (creatureRespawnDuration > 0)
                {
                    RegenerationInterval = (int)((float)creatureRespawnDuration * creatureSpawnRateMultiplier);

                    ReinitializeHeartbeats();

                    if (Biota.PropertiesGenerator != null)
                    {
                        // While this may be ugly, it's done for performance reasons.
                        // Common weenie properties are not cloned into the bota on creation. Instead, the biota references simply point to the weenie collections.
                        // The problem here is that we want to update one of those common collection properties. If the biota is referencing the weenie collection,
                        // then we'll end up updating the global weenie (from the cache), instead of just this specific biota.
                        if (Biota.PropertiesGenerator == Weenie.PropertiesGenerator)
                        {
                            Biota.PropertiesGenerator = new List<ACE.Entity.Models.PropertiesGenerator>(Weenie.PropertiesGenerator.Count);

                            foreach (var record in Weenie.PropertiesGenerator)
                                Biota.PropertiesGenerator.Add(record.Clone());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Restore a WorldObject from the database.
        /// </summary>
        public GenericObject(Biota biota) : base(biota)
        {
            SetEphemeralValues();
        }

        private void SetEphemeralValues()
        {
            //StackSize = null;
            //StackUnitEncumbrance = null;
            //StackUnitValue = null;
            //MaxStackSize = null;

            // Linkable Item Generator (linkitemgen2minutes) fix
            if (WeenieClassId == 4142)
            {
                MaxGeneratedObjects = 0;
                InitGeneratedObjects = 0;
            }
        }

        public override void ActOnUse(WorldObject activator)
        {
            if (!(activator is Player player))
                return;

            if (UseSound > 0)
                player.Session.Network.EnqueueSend(new GameMessageSound(player.Guid, UseSound));
        }
    }
}
