using ACE.Entity.Models;
using ACE.Server.Managers;
using ACE.Server.WorldObjects;
using System;
using System.Collections.Generic;

namespace ACE.Server.Realms
{
    /// <summary>
    /// Represents a temporary realm created on a landblock by landblock basis (such as an 'instance' in the traditional sense)
    /// </summary>
    public class EphemeralRealm
    {
        public RulesetTemplate RulesetTemplate { get; set; }
        //public bool IsDuelInstance => RulesetTemplate.

        private EphemeralRealm() { }
        private EphemeralRealm(RulesetTemplate template)
        {
            this.RulesetTemplate = template;
        }

        public static EphemeralRealm Initialize(WorldRealm baseRealm, List<Realm> appliedRealms)
        {
            string key = baseRealm.Realm.Id.ToString();
            RulesetTemplate template = null;
            RulesetTemplate prevTemplate = baseRealm.RulesetTemplate;
            for(int i = 0; i < appliedRealms.Count; i++)
            {
                var appliedRealm = appliedRealms[i];
                key += $".{appliedRealm.Id}";
                template = RealmManager.GetEphemeralRealmRulesetTemplate(key);
                if (template == null)
                {
                    template = RulesetTemplate.MakeRuleset(prevTemplate, appliedRealm);
                    RealmManager.CacheEphemeralRealmTemplate(key, template);
                }
                prevTemplate = template;
            }

            if (template == null)
                template = prevTemplate;
            return new EphemeralRealm(template);
        }
    }
}
