using ACE.Entity;
using ACE.Entity.Enum.Properties;
using ACE.Entity.Enum.RealmProperties;
using ACE.Entity.Models;
using ACE.Server.Managers;
using ACE.Server.WorldObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACE.Server.Realms
{
    public class WorldRealm(Realm realm, RulesetTemplate rulesetTemplate)
    {
        public Realm Realm { get; } = realm;
        public RulesetTemplate RulesetTemplate { get; } = rulesetTemplate;
        public AppliedRuleset StandardRules { get; } = AppliedRuleset.MakeRerolledRuleset(rulesetTemplate);
        public bool NeedsRefresh { get; internal set; }

        internal InstancedPosition DefaultStartingLocation(Player player)
        {

            if (StandardRules.GetProperty(RealmPropertyBool.IsDuelingRealm))
            {
                //Adventurer's Haven
                //0x01AC0118[29.684622 - 30.072382 0.010000] - 0.027857 0.999612 0.000000 0.000000
                return DuelRealmHelpers.GetDuelingAreaDrop(player);
            }
            else
            {
                //Holtburg
                return new LocalPosition(0xA9B40019, 84f, 7.1f, 94.005005f, 0f, 0f, -0.078459f, 0.996917f).AsInstancedPosition(player, PlayerInstanceSelectMode.HomeRealm);
            }
        }

        internal bool IsWhitelistedLandblock(ushort landblock)
        {
            if (StandardRules.GetProperty(RealmPropertyBool.IsDuelingRealm))
                return RealmConstants.DuelLandblocks.Contains(landblock);
            return true;
        }
    }
}
