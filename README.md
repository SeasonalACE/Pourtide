# ACRealms V2.1

This is a fork of https://github.com/ACEmulator/ACE/. The repo was not originally set up a fork and therefore could not be turned into one, but there is a mirror repo here for visibility https://github.com/ACRealms/ACRealmsForkMirror.

It is recommended to fork from the mirror repo instead of this one. The mirror repo may eventually become the main one. 

# Looking to get started?

Before you do anything, note that the master branch in this project is a development branch and is not considered to be stable. Please use the latest even-numbered versioned branch for the most stability.

Currently, that branch is `v2.0`  


# Mission Statement

**The mission of ACRealms is to be the recommended Asheron's Call server emulator for servers with heavy customization needs.**

Focus areas:
 - Instanced Landblocks (multiple logical dungeons in same 'physical' landblock space)
 - Ruleset Composition (Realms are composed of ruleset definitions chained together recursively)
 - Ephemeral Realms (Temporary landblocks that may be assigned additional rulesets, including those defined by the player through crafting, inspired by the map device in Path of Exile)
 - Automated testing - ACE traditionally lacked this capability. Due to the sheer amount and complexity of changes, and customizability of rulesets, automated tests are more important here.
 

## Contributing

Contributions (time and development, **not money**) and feedback greatly appreciated. Contributors and server operators will have more of a say in development direction and priorities.

The best way to start contributing is to join the discord (https://discord.gg/pN65pYqZeS) and introduce yourself. Or alternatively, clone the repo and review the implementation and start experimenting!
You'll likely find something that can be improved. There are still many features not implemented as a realm property. 

For complex changes, it is recommended to get in touch via Discord before attempting them.

If you don't use Discord and want to contribute, email me and let me know what mode of communication works best for you.


## Caveats

1. `LandblockInstance` entities are shared across seasons. This means all static guids belonging to a `LandblockInstance` are the same no matter which realm  your character exists in. 
2. Because all static guids are shared, housing cannot exist in multiple seasons independently. SeasonalACE only supports housing in the current active season, past seasons that are no longer active have all housing related biotas removed and housing storage cleared (characters can still be played in past seasons, just without housing).
3. There are still unforseen bugs. ACRealms never had much exposure and is not very battle tested but there should be no serious impact on performance or player experience. 


#### Realms.jsonc
UPDATE May 2024: This file is now auto generated and doesn't need to be edited manually.  

Previously:

realms.jsonc contains a list of realm and ruleset names mapped to realm ids. These ids can be changed to anything between 1 and 0x7FFE (32766), and may not be changed to new values after the first run of the server.  
New realms can still be added to the list as long as they are not changed after the next time the server is started.
The realm name must match the name specified in the realm file (not the filename).
If a realm file exists, it must have a corresponding entry in this file, and vice versa. It's not the most user-friendly process but there is room for improvement.

For example the default season for SeasonalACE is the `realms-pvp.jsonc` file:

It is recommended to use visual studio code to edit these files, using the "Content" folder as the root. A json-schema folder exists and realm properties will be populated in this schema after successful build of the ACE.Server project.  
This allows for autocomplete and tooltip functionality to be integrated with the editor. 

A realm file exists under `Content/json/realms/realm/xxx.jsonc`, and a ruleset file exists under `Content/json/realms/ruleset/xxx.jsonc`. They have the same basic structure.
The key difference between a realm and a ruleset is that a realm may exist as a permanent home location for a player. A ruleset does not. Rulesets are intended to be composed on top of realms, to produce "ephemeral realms" (temporary rulesets).
Realm definitions may also be composed in a similar manner, but the result is a permanent world

```json
{
  "name": "Modern Realm (PvP)",
  "type": "Realm",
  "parent": "Modern Realm",
  "properties": {
    "Description": "The Customized PvP Realm with the latest and greatest features.",
    "IsPKOnly": true
  }
}
```


Property entry hash valid keys:
- `value`: A default value, matching the type of the corresponding property. May not be present if `low` or `high` is present. This takes precedence over the default value defined in the enum type.
- `low`: The minimum value when rerolled. If present, `high` must also be defined. The absolute minimum defined in the enum type will take precedence if there is a conflict. 
- `high`: The maximum value when rerolled. If present, `low` must also be defined. The absolute maximum defined in the enum type will take precedence if there is a conflict.
- `reroll`: One of:
  - `landblock` (default): Reroll once during landblock load
  - `always`: Reroll each time the property is accessed by game logic
  - `never`: Use the default value
- `compose`: One of:
  - `add`: Add the result of the two rulesets together
  - `multiply`: Multiply the result of the two rulesets together
  - `replace`: Discard the previous value and replace it with the result from this ruleset.
- `locked`: Rulesets that inherit from this ruleset will ignore any properties that are locked here instead of composing them. If applying a ruleset with `apply_rulesets` or `apply_rulesets_random`, that value will become locked if specified.
- `probability`: A floating-point number between 0 and 1 representing the probability of this property taking effect.

#### RealmProperty Enums

These are shared by all realms, application-wide, and list the possible realm properties that can be used. If the property is in this list, it can be used in a realm definition.
```C#
    public enum RealmPropertyFloat : ushort
    {
        [RealmPropertyFloat(defaultValue: 0f, minValue: 0f, maxValue: 0f)]
        Undef                          = 0,

        // First param is the default, second is the min, third is the max.
        [RealmPropertyFloat(1f, 0.1f, 5f)]
        SpellCasting_MoveToState_UpdatePosition_Threshold = 1,

        // If defaultFromServerProperty is defined, the corresponding property from PropertyManager (/fetchdouble in this case) will used in place of the default.
        // If the server property with the given name is missing from the database, the defaultValue parameter will be used as a fallback. 
        [RealmPropertyFloat(defaultFromServerProperty: "spellcast_max_angle", 20f, 0f, 360f)]
        Spellcasting_Max_Angle = 2,
        ...
    }
```

## Known Issues

- Ruleset and realm files were originally intended to be updatable without a restart of the server, and a very early version of this project allowed it, but there were issues with caching. I still want to fix that because restarting the server is not convenient when experimenting with ideas for new rulesets.
- The ruleset specification is complex and not yet fully covered by unit tests, but progress is being made here. If you notice any unexpected behavior with rulesets, please report it!
- landblock content files are global and cannot be defined on a realm by realm basis yet. This is something I've wanted to address for a very long time now but it hasn't been done yet because of priorities. (UPDATE: This is now scheduled for the v2.3 milestone)

## Developer notes

Property IDs (ACE.Entity.Enum.Properties.PropertyXXX) and PositionTypes from 42000-42999 are reserved by AC Realms Core. 

Realm Property IDs (ACE.Entity.Enum.Properties.RealmPropertyXXX) from 0-9999 are reserved by AC Realms Core in a similar manner.

If using this project in your own server, please do not add new properties with IDs in this range.

## Contact

`russellfannin0@gmail.com`

Discord Link: https://discord.gg/pN65pYqZeS

## License

AGPL v3

## Server Operator Guidelines

```json
{
  "name": "random-test",
  "type": "Ruleset",
  "properties": {
    "Description": "A test ruleset for random properties.",
    "Spellcasting_Max_Angle": {
      "low": "5",
      "high": "180",
      "reroll": "landblock" /*never, always, landblock, admin */,
      "locked": true,
      "probability": 0.5
    },
    "CreatureSpawnHPMultiplier": {
      "low": 0.5,
      "high": 2.0,
      "compose": "add"
    }
  },
  "apply_rulesets": ["creature-attribute-randomizer"],
  "apply_rulesets_random": [
    [{ "creature-hp-boost": 0.5 }, { "creature-hp-boost": 1.0 }],
    { "pvp-ruleset": "auto" },
    { "creature-attribute-randomizer": 0.5 }
  ]
}
```

## Commands

**Admin Commands**
1. `/change-season <realmId>` - This command changes the current active season to a new realmId, newly created players will be created in this season, having no physical interactions with other seasoned characters. Transitioning to a new season clears housing and housing storage from the previous active season.
2. `/exiti` - Exits the current ephemeral realm (dungeon landblock instance).

**Player Commands**
1. `/season-info` - This command gives you information about the current active season that a player belongs to.
2. `/season-list` - This command displays a list of all available seasons to choose from, highlighting the current active season.
3. `/realm-list` - This command displays all realms that exist in the realms.jsonc file.
4. `/ruleset-list` - This command displays all rulesets that exist in rulesets directory.
5. `/realm-info` - This command displays all realm information for the current realm a player exists in (useful for dungeon landblock realms, also known as ephemeral realms).