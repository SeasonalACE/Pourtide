DELETE FROM `weenie` WHERE `class_Id` = 601000;

INSERT INTO `weenie` (`class_Id`, `class_Name`, `type`, `last_Modified`)
VALUES (601000, 'ace601000-forgottenlich', 10, '2024-04-16 13:54:07') /* Creature */;

INSERT INTO `weenie_properties_int` (`object_Id`, `type`, `value`)
VALUES (601000,   1,         16) /* ItemType - Creature */
     , (601000,   2,         14) /* CreatureType - Undead */
     , (601000,   3,         67) /* PaletteTemplate - GreenSlime */
     , (601000,   6,         -1) /* ItemsCapacity */
     , (601000,   7,         -1) /* ContainersCapacity */
     , (601000,  16,          1) /* ItemUseable - No */
     , (601000,  25,         20) /* Level */
     , (601000,  27,          0) /* ArmorType - None */
     , (601000,  40,          1) /* CombatMode - NonCombat */
     , (601000,  68,          3) /* TargetingTactic - Random, Focused */
     , (601000,  93,       1032) /* PhysicsState - ReportCollisions, Gravity */
     , (601000, 101,        183) /* AiAllowedCombatStyle - Unarmed, OneHanded, OneHandedAndShield, Bow, Crossbow, ThrownWeapon */
     , (601000, 133,          2) /* ShowableOnRadar - ShowMovement */
     , (601000, 140,          1) /* AiOptions - CanOpenDoors */
     , (601000, 146,       3500) /* XpOverride */;

INSERT INTO `weenie_properties_bool` (`object_Id`, `type`, `value`)
VALUES (601000,   1, True ) /* Stuck */
     , (601000,   6, True ) /* AiUsesMana */
     , (601000,  11, False) /* IgnoreCollisions */
     , (601000,  12, True ) /* ReportCollisions */
     , (601000,  13, False) /* Ethereal */
     , (601000,  14, True ) /* GravityStatus */
     , (601000,  19, True ) /* Attackable */
     , (601000,  50, True ) /* NeverFailCasting */;

INSERT INTO `weenie_properties_float` (`object_Id`, `type`, `value`)
VALUES (601000,   1,       5) /* HeartbeatInterval */
     , (601000,   2,       0) /* HeartbeatTimestamp */
     , (601000,   3, 0.30000001192092896) /* HealthRate */
     , (601000,   4,     0.5) /* StaminaRate */
     , (601000,   5,       2) /* ManaRate */
     , (601000,  12,     0.5) /* Shade */
     , (601000,  13, 0.800000011920929) /* ArmorModVsSlash */
     , (601000,  14, 0.30000001192092896) /* ArmorModVsPierce */
     , (601000,  15, 0.550000011920929) /* ArmorModVsBludgeon */
     , (601000,  16, 0.18000000715255737) /* ArmorModVsCold */
     , (601000,  17,     0.5) /* ArmorModVsFire */
     , (601000,  18, 0.550000011920929) /* ArmorModVsAcid */
     , (601000,  19, 0.6700000166893005) /* ArmorModVsElectric */
     , (601000,  31,      18) /* VisualAwarenessRange */
     , (601000,  34,       1) /* PowerupTime */
     , (601000,  36,       1) /* ChargeSpeed */
     , (601000,  64,       1) /* ResistSlash */
     , (601000,  65, 0.5199999809265137) /* ResistPierce */
     , (601000,  66,    0.75) /* ResistBludgeon */
     , (601000,  67,       1) /* ResistFire */
     , (601000,  68, 0.20000000298023224) /* ResistCold */
     , (601000,  69,    0.75) /* ResistAcid */
     , (601000,  70, 0.8600000143051147) /* ResistElectric */
     , (601000,  71,       1) /* ResistHealthBoost */
     , (601000,  72,       1) /* ResistStaminaDrain */
     , (601000,  73,       1) /* ResistStaminaBoost */
     , (601000,  74,       1) /* ResistManaDrain */
     , (601000,  75,       1) /* ResistManaBoost */
     , (601000,  80, 2.5999999046325684) /* AiUseMagicDelay */
     , (601000, 104,      10) /* ObviousRadarRange */
     , (601000, 122,       2) /* AiAcquireHealth */
     , (601000, 125,       1) /* ResistHealthDrain */;

INSERT INTO `weenie_properties_string` (`object_Id`, `type`, `value`)
VALUES (601000,   1, 'Forgotten Lich') /* Name */;

INSERT INTO `weenie_properties_d_i_d` (`object_Id`, `type`, `value`)
VALUES (601000,   1,   33554839) /* Setup */
     , (601000,   2,  150994967) /* MotionTable */
     , (601000,   3,  536870934) /* SoundTable */
     , (601000,   4,  805306368) /* CombatTable */
     , (601000,   6,   67110722) /* PaletteBase */
     , (601000,   7,  268435558) /* ClothingBase */
     , (601000,   8,  100667942) /* Icon */
     , (601000,  22,  872415272) /* PhysicsEffectTable */
     , (601000,  32,        248) /* WieldedTreasureType - 
                                   Wield 6x Throwing Axe (304) | Probability: 10%
                                   Wield Nayin (334) | Probability: 10%
                                   Wield 20x Arrow (300) | Probability: 100%
                                   Wield Longbow (306) | Probability: 10%
                                   Wield 20x Arrow (300) | Probability: 100%
                                   Wield Yumi (363) | Probability: 10%
                                   Wield 14x Arrow (300) | Probability: 100%
                                   Wield Heavy Crossbow (311) | Probability: 60.000004%
                                   Wield 15x Quarrel (305) | Probability: 100%
                                   Wield Battle Axe (301) | Probability: 14%
                                   Wield Broad Sword (350) | Probability: 7%
                                   Wield Kaskara (324) | Probability: 6%
                                   Wield Ken (327) | Probability: 6%
                                   Wield Long Sword (351) | Probability: 6%
                                   Wield Morning Star (332) | Probability: 10%
                                   Wield Scimitar (339) | Probability: 6%
                                   Wield Shamshir (340) | Probability: 6%
                                   Wield Ono (336) | Probability: 13%
                                   Wield Silifi (344) | Probability: 13%
                                   Wield Tachi (353) | Probability: 6%
                                   Wield Takuba (354) | Probability: 6%
                                   Wield Large Kite Shield (92) | Probability: 30.000002%
                                   Wield Kite Shield (91) | Probability: 20%
                                   Wield Large Round Shield (94) | Probability: 20% */
     , (601000,  35,        453) /* DeathTreasureType - Loot Tier: 1 */;

INSERT INTO `weenie_properties_attribute` (`object_Id`, `type`, `init_Level`, `level_From_C_P`, `c_P_Spent`)
VALUES (601000,   1,  50, 0, 0) /* Strength */
     , (601000,   2,  60, 0, 0) /* Endurance */
     , (601000,   3,  30, 0, 0) /* Quickness */
     , (601000,   4,  80, 0, 0) /* Coordination */
     , (601000,   5, 125, 0, 0) /* Focus */
     , (601000,   6, 115, 0, 0) /* Self */;

INSERT INTO `weenie_properties_attribute_2nd` (`object_Id`, `type`, `init_Level`, `level_From_C_P`, `c_P_Spent`, `current_Level`)
VALUES (601000,   1,    60, 0, 0, 90) /* MaxHealth */
     , (601000,   3,    70, 0, 0, 130) /* MaxStamina */
     , (601000,   5,    40, 0, 0, 155) /* MaxMana */;

INSERT INTO `weenie_properties_skill` (`object_Id`, `type`, `level_From_P_P`, `s_a_c`, `p_p`, `init_Level`, `resistance_At_Last_Check`, `last_Used_Time`)
VALUES (601000,  1, 0, 3, 0,  90, 0, 271.0123596191406) /* Axe                 Specialized */
     , (601000,  2, 0, 3, 0, 100, 0, 271.0123596191406) /* Bow                 Specialized */
     , (601000,  3, 0, 3, 0, 100, 0, 271.0123596191406) /* Crossbow            Specialized */
     , (601000,  4, 0, 3, 0,  90, 0, 271.0123596191406) /* Dagger              Specialized */
     , (601000,  5, 0, 3, 0,  90, 0, 271.0123596191406) /* Mace                Specialized */
     , (601000,  6, 0, 3, 0,  86, 0, 271.0123596191406) /* MeleeDefense        Specialized */
     , (601000,  7, 0, 3, 0, 126, 0, 271.0123596191406) /* MissileDefense      Specialized */
     , (601000,  9, 0, 3, 0,  90, 0, 271.0123596191406) /* Spear               Specialized */
     , (601000, 10, 0, 3, 0,  90, 0, 271.0123596191406) /* Staff               Specialized */
     , (601000, 11, 0, 3, 0,  90, 0, 271.0123596191406) /* Sword               Specialized */
     , (601000, 13, 0, 3, 0,  90, 0, 271.0123596191406) /* UnarmedCombat       Specialized */
     , (601000, 14, 0, 3, 0,  50, 0, 271.0123596191406) /* ArcaneLore          Specialized */
     , (601000, 15, 0, 3, 0,  76, 0, 271.0123596191406) /* MagicDefense        Specialized */
     , (601000, 20, 0, 3, 0,  50, 0, 271.0123596191406) /* Deception           Specialized */
     , (601000, 31, 0, 3, 0,  24, 0, 271.0123596191406) /* CreatureEnchantment Specialized */
     , (601000, 33, 0, 3, 0,  24, 0, 271.0123596191406) /* LifeMagic           Specialized */
     , (601000, 34, 0, 3, 0,  24, 0, 271.0123596191406) /* WarMagic            Specialized */;

INSERT INTO `weenie_properties_body_part` (`object_Id`, `key`, `d_Type`, `d_Val`, `d_Var`, `base_Armor`, `armor_Vs_Slash`, `armor_Vs_Pierce`, `armor_Vs_Bludgeon`, `armor_Vs_Cold`, `armor_Vs_Fire`, `armor_Vs_Acid`, `armor_Vs_Electric`, `armor_Vs_Nether`, `b_h`, `h_l_f`, `m_l_f`, `l_l_f`, `h_r_f`, `m_r_f`, `l_r_f`, `h_l_b`, `m_l_b`, `l_l_b`, `h_r_b`, `m_r_b`, `l_r_b`)
VALUES (601000,  0,  4,  0,    0,   70,   56,   21,   39,   13,   35,   39,   47,    0, 1, 0.33,    0,    0, 0.33,    0,    0, 0.33,    0,    0, 0.33,    0,    0) /* Head */
     , (601000,  1,  4,  0,    0,   80,   64,   24,   44,   14,   40,   44,   54,    0, 2, 0.44, 0.17,    0, 0.44, 0.17,    0, 0.44, 0.17,    0, 0.44, 0.17,    0) /* Chest */
     , (601000,  2,  4,  0,    0,   80,   64,   24,   44,   14,   40,   44,   54,    0, 3,    0, 0.17,    0,    0, 0.17,    0,    0, 0.17,    0,    0, 0.17,    0) /* Abdomen */
     , (601000,  3,  4,  0,    0,   70,   56,   21,   39,   13,   35,   39,   47,    0, 1, 0.23, 0.03,    0, 0.23, 0.03,    0, 0.23, 0.03,    0, 0.23, 0.03,    0) /* UpperArm */
     , (601000,  4,  4,  0,    0,   80,   64,   24,   44,   14,   40,   44,   54,    0, 2,    0,  0.3,    0,    0,  0.3,    0,    0,  0.3,    0,    0,  0.3,    0) /* LowerArm */
     , (601000,  5,  4,  2, 0.75,   80,   64,   24,   44,   14,   40,   44,   54,    0, 2,    0,  0.2,    0,    0,  0.2,    0,    0,  0.2,    0,    0,  0.2,    0) /* Hand */
     , (601000,  6,  4,  0,    0,   90,   72,   27,   50,   16,   45,   50,   60,    0, 3,    0, 0.13, 0.18,    0, 0.13, 0.18,    0, 0.13, 0.18,    0, 0.13, 0.18) /* UpperLeg */
     , (601000,  7,  4,  0,    0,   90,   72,   27,   50,   16,   45,   50,   60,    0, 3,    0,    0,  0.6,    0,    0,  0.6,    0,    0,  0.6,    0,    0,  0.6) /* LowerLeg */
     , (601000,  8,  4,  3, 0.75,   90,   72,   27,   50,   16,   45,   50,   60,    0, 3,    0,    0, 0.22,    0,    0, 0.22,    0,    0, 0.22,    0,    0, 0.22) /* Foot */;

INSERT INTO `weenie_properties_spell_book` (`object_Id`, `spell`, `probability`)
VALUES (601000,    59,  2.029)  /* Acid Stream II */
     , (601000,    65,  2.029)  /* Shock Wave II */
     , (601000,    70,  2.029)  /* Frost Bolt II */
     , (601000,    76,  2.029)  /* Lightning Bolt II */
     , (601000,    81,  2.029)  /* Flame Bolt II */
     , (601000,    87,  2.029)  /* Force Bolt II */
     , (601000,    93,  2.029)  /* Whirling Blade II */
     , (601000,   172,  2.009)  /* Fester Other II */
     , (601000,  1238,   2.02)  /* Drain Health Other II */
     , (601000,  1250,   2.02)  /* Drain Stamina Other II */
     , (601000,  1261,   2.02)  /* Drain Mana Other II */
     , (601000,  1339,  2.009)  /* Weakness Other II */
     , (601000,  1368,  2.009)  /* Frailty Other II */
     , (601000,  1392,  2.009)  /* Clumsiness Other II */
     , (601000,  1416,  2.009)  /* Slowness Other II */
     , (601000,  1440,  2.009)  /* Bafflement Other II */
     , (601000,  1464,  2.009)  /* Feeblemind Other II */;

INSERT INTO `weenie_properties_emote` (`object_Id`, `category`, `probability`, `weenie_Class_Id`, `style`, `substyle`, `quest`, `vendor_Type`, `min_Health`, `max_Health`)
VALUES (601000,  3 /* Death */,   0.02, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

SET @parent_id = LAST_INSERT_ID();

INSERT INTO `weenie_properties_emote_action` (`emote_Id`, `order`, `type`, `delay`, `extent`, `motion`, `message`, `test_String`, `min`, `max`, `min_64`, `max_64`, `min_Dbl`, `max_Dbl`, `stat`, `display`, `amount`, `amount_64`, `hero_X_P_64`, `percent`, `spell_Id`, `wealth_Rating`, `treasure_Class`, `treasure_Type`, `p_Script`, `sound`, `destination_Type`, `weenie_Class_Id`, `stack_Size`, `palette`, `shade`, `try_To_Bond`, `obj_Cell_Id`, `origin_X`, `origin_Y`, `origin_Z`, `angles_W`, `angles_X`, `angles_Y`, `angles_Z`)
VALUES (@parent_id,  0,  17 /* LocalBroadcast */, 0, 0, NULL, 'As the ancient creature collapses into viscera and rot, it groans the name of Avoren.', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

INSERT INTO `weenie_properties_create_list` (`object_Id`, `destination_Type`, `weenie_Class_Id`, `stack_Size`, `palette`, `shade`, `try_To_Bond`)
VALUES (601000, 9,  7041,  0, 0, 0.02, False) /* Create Undead Thighbone (7041) for ContainTreasure */
     , (601000, 9,     0,  0, 0, 0.98, False) /* Create nothing for ContainTreasure */
     , (601000, 9,  9312,  0, 0, 0.03, False) /* Create A Small Mnemosyne (9312) for ContainTreasure */
     , (601000, 9,     0,  0, 0, 0.97, False) /* Create nothing for ContainTreasure */;

/* Lifestoned Changelog:
{
  "LastModified": "2024-04-16T06:53:34.6651088-07:00",
  "ModifiedBy": "pourman",
  "Changelog": [],
  "UserChangeSummary": "Removing Seasonal Drops",
  "IsDone": true
}
*/
