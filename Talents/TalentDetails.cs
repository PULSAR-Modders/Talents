using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Talents
{
    [HarmonyPatch(typeof(PLGlobal), "GetTalentInfoForTalentType")]
    class GetTalentInfoForTalentType
    {
        protected static FieldInfo CachedTalentInfosInfo = AccessTools.Field(typeof(PLGlobal), "CachedTalentInfos");
        public static bool Prefix(PLGlobal __instance, ETalents inTalent, ref TalentInfo __result)
        {
            Dictionary<int, TalentInfo> CachedTalentInfos = (Dictionary<int, TalentInfo>)CachedTalentInfosInfo.GetValue(__instance);
            if (CachedTalentInfos.ContainsKey((int)inTalent))
            {
                __result = CachedTalentInfos[(int)inTalent];
                return false;
            }
            TalentInfo talentInfo = new TalentInfo();
            talentInfo.ClassID = -1;
            talentInfo.TalentID = (int)inTalent;
            talentInfo.MaxRank = 3;
            talentInfo.ResearchCost = new int[6];
            talentInfo.WarpsToResearch = 3;
            talentInfo.ExtendsTalent = ETalents.MAX;
            talentInfo.MinLevel = 0;
            switch ((int)inTalent)
            {
                case (int)ETalents.HEALTH_BOOST:
                    talentInfo.Name = "Health Boost";
                    talentInfo.Desc = "+20 to max health per rank";
                    talentInfo.MaxRank = 5;
                    //talentInfo.GetAdditionalData().ConflictTalent = ETalents.ARMOR_BOOST;
                    break;
                case (int)ETalents.OXYGEN_TRAINING:
                    talentInfo.Name = "Oxygen Survival";
                    talentInfo.Desc = "Last 20% longer without oxygen per rank";
                    talentInfo.MaxRank = 5;
                    break;
                case (int)ETalents.PISTOL_DMG_BOOST:
                    talentInfo.Name = "Pistoleer";
                    talentInfo.Desc = "Boosts pistol damage by 10% per rank";
                    talentInfo.MaxRank = 5;
                    break;
                case (int)ETalents.QUICK_RESPAWN:
                    talentInfo.Name = "Custom DNA";
                    talentInfo.Desc = "Decreases respawn time by 12% per rank";
                    talentInfo.MaxRank = 4;
                    break;
                case (int)ETalents.ADVANCED_OPERATOR:
                    talentInfo.Name = "Advanced Operator";
                    talentInfo.Desc = "Allows use of the main turret systems";
                    talentInfo.MaxRank = 1;
                    break;
                case (int)ETalents.CAP_CREW_SPEED_BOOST:
                    talentInfo.Name = "Crew Marathon Training";
                    talentInfo.Desc = "Increases crew run speed by 6% per rank";
                    talentInfo.MaxRank = 5;
                    talentInfo.ClassID = 0;
                    break;
                case (int)ETalents.CAP_SCAVENGER:
                    talentInfo.Name = "Scavenger";
                    talentInfo.Desc = "Increases captured credits by 10% per rank";
                    talentInfo.MaxRank = 4;
                    talentInfo.ClassID = 0;
                    break;
                case (int)ETalents.CAP_SHOP_DISCOUNTS:
                    talentInfo.Name = "Shopping Spree";
                    talentInfo.Desc = "Reduces prices at shops by 5% per rank";
                    talentInfo.MaxRank = 5;
                    talentInfo.ClassID = 0;
                    talentInfo.ResearchCost[0] = 2;
                    talentInfo.ResearchCost[1] = 1;
                    break;
                case (int)ETalents.PIL_SHIP_SPEED:
                    talentInfo.Name = "Racing School";
                    talentInfo.Desc = "Boosts ship thrust 5% per rank";
                    talentInfo.MaxRank = 5;
                    talentInfo.ClassID = 1;
                    break;
                case (int)ETalents.PIL_SHIP_TURNING:
                    talentInfo.Name = "Special Inertia Training";
                    talentInfo.Desc = "Boosts ship rotation speed 5% per rank";
                    talentInfo.MaxRank = 5;
                    talentInfo.ClassID = 1;
                    break;
                case (int)ETalents.PIL_KEEN_EYES:
                    talentInfo.Name = "Custom Eye Implants";
                    talentInfo.Desc = "Piloting UI provides additional information";
                    talentInfo.MaxRank = 1;
                    talentInfo.ClassID = 1;
                    talentInfo.ResearchCost[3] = 2;
                    talentInfo.ResearchCost[4] = 1;
                    talentInfo.WarpsToResearch = 3;
                    break;
                case (int)ETalents.SCI_SENSOR_BOOST:
                    talentInfo.Name = "Scanner Alignment";
                    talentInfo.Desc = "Boosts ship detection level 10% per rank";
                    talentInfo.MaxRank = 5;
                    talentInfo.ClassID = 2;
                    break;
                case (int)ETalents.SCI_SENSOR_HIDE:
                    talentInfo.Name = "Custom Shield Algorithms";
                    talentInfo.Desc = "Reduces ship signature level 10% per rank";
                    talentInfo.MaxRank = 4;
                    talentInfo.ClassID = 2;
                    break;
                case (int)ETalents.SCI_HEAL_NEARBY:
                    talentInfo.Name = "Bridge Medic";
                    talentInfo.Desc = "Heals nearby allies for a small amount over time";
                    talentInfo.MaxRank = 5;
                    talentInfo.ClassID = 2;
                    break;
                case (int)ETalents.SCI_FREQ_AMPLIFIER:
                    talentInfo.Name = "Frequency Amplification";
                    talentInfo.Desc = "Increases positive effects of the shield frequency setting.";
                    talentInfo.MaxRank = 5;
                    talentInfo.ClassID = 2;
                    break;
                case (int)ETalents.WPNS_TURRET_BOOST:
                    talentInfo.Name = "Turret Charge Booster";
                    talentInfo.Desc = "Increases charge speed of turret you are operating";
                    talentInfo.MaxRank = 5;
                    talentInfo.ClassID = 3;
                    break;
                case (int)ETalents.WPNS_MISSILE_EXPERT:
                    talentInfo.Name = "Missile Expert";
                    talentInfo.Desc = "Increases missile lock on rate by 25% per rank";
                    talentInfo.MaxRank = 4;
                    talentInfo.ClassID = 3;
                    break;
                case (int)ETalents.WPNS_RANGE_BOOST:
                    talentInfo.Name = "Turret Range";
                    talentInfo.Desc = "Increases your turret's range by 10% per rank";
                    talentInfo.MaxRank = 4;
                    talentInfo.ClassID = 3;
                    break;
                case (int)ETalents.ENG_FIRE_REDUCTION:
                    talentInfo.Name = "Nano Anti-Fire System";
                    talentInfo.Desc = "Slows the spread and growth of fires";
                    talentInfo.MaxRank = 5;
                    talentInfo.ClassID = 4;
                    break;
                case (int)ETalents.ENG_COOLANT_MIX_CUSTOM:
                    talentInfo.Name = "Custom Coolant Mix";
                    talentInfo.Desc = "Reduces the amount of coolant used by 8% per rank";
                    talentInfo.MaxRank = 4;
                    talentInfo.ClassID = 4;
                    break;
                case (int)ETalents.ENG_REPAIR_DRONES:
                    talentInfo.Name = "Auto-Repair Module";
                    talentInfo.Desc = "Repairs major systems over time";
                    talentInfo.MaxRank = 4;
                    talentInfo.ClassID = 4;
                    break;
                case (int)ETalents.INC_STAMINA:
                    talentInfo.Name = "Stamina Boost";
                    talentInfo.Desc = "Increases your stamina by 20% per rank";
                    talentInfo.MaxRank = 4;
                    talentInfo.ClassID = -1;
                    talentInfo.ResearchCost[1] = 2;
                    talentInfo.ResearchCost[2] = 1;
                    talentInfo.WarpsToResearch = 3;
                    break;
                case (int)ETalents.WPNS_COOLING:
                    talentInfo.Name = "Gun Cooling";
                    talentInfo.Desc = "All crew guns will cool down 5% faster per rank";
                    talentInfo.MaxRank = 4;
                    talentInfo.ClassID = 3;
                    talentInfo.ResearchCost[0] = 2;
                    talentInfo.ResearchCost[2] = 1;
                    talentInfo.ResearchCost[4] = 2;
                    talentInfo.WarpsToResearch = 4;
                    break;
                case (int)ETalents.INC_JETPACK:
                    talentInfo.Name = "Custom Jetpack Fuel";
                    talentInfo.Desc = "Increases your jetpack's recharge rate by 25% per rank";
                    talentInfo.MaxRank = 4;
                    talentInfo.ResearchCost[0] = 1;
                    talentInfo.WarpsToResearch = 2;
                    break;
                case (int)ETalents.WPNS_REDUCE_PAWN_DMG:
                    talentInfo.Name = "Armored Skin Cells";
                    talentInfo.Desc = "Decreases incoming damage by 10% per rank";
                    talentInfo.MaxRank = 4;
                    talentInfo.ClassID = 3;
                    talentInfo.ResearchCost[0] = 1;
                    talentInfo.WarpsToResearch = 2;
                    break;
                case (int)ETalents.INC_MAX_WEIGHT:
                    talentInfo.Name = "Strength Boost";
                    talentInfo.Desc = "Increases carrying capacity by 10 per rank";
                    talentInfo.MaxRank = 4;
                    break;
                case (int)ETalents.CAP_ARMOR_BOOST:
                    talentInfo.Name = "Crew Heavy Armor Training";
                    talentInfo.Desc = "All crew members gain additional Armor (+1 per rank)";
                    talentInfo.MaxRank = 5;
                    talentInfo.ClassID = 0;
                    talentInfo.ResearchCost[3] = 1;
                    talentInfo.ResearchCost[4] = 1;
                    talentInfo.WarpsToResearch = 3;
                    break;
                case (int)ETalents.SCI_RESEARCH_SPECIALTY:
                    talentInfo.Name = "Research Specialty";
                    talentInfo.Desc = "Reduces the warps required to research talents by 1";
                    talentInfo.MaxRank = 1;
                    talentInfo.ClassID = 2;
                    talentInfo.ResearchCost[3] = 3;
                    talentInfo.ResearchCost[5] = 1;
                    talentInfo.WarpsToResearch = 3;
                    break;
                case (int)ETalents.ENG_WARP_CHARGE_BOOST:
                    talentInfo.Name = "Warp Charge Booster";
                    talentInfo.Desc = "Increases the warp drive charge rate by 20% per rank";
                    talentInfo.MaxRank = 3;
                    talentInfo.ClassID = 4;
                    talentInfo.ResearchCost[2] = 1;
                    talentInfo.ResearchCost[4] = 1;
                    talentInfo.WarpsToResearch = 3;
                    break;
                case (int)ETalents.INC_TURRET_ZOOM:
                    talentInfo.Name = "Turret Zoom";
                    talentInfo.Desc = "Increases the zoom in turrets by 15% per rank";
                    talentInfo.MaxRank = 3;
                    talentInfo.ResearchCost[3] = 1;
                    talentInfo.WarpsToResearch = 2;
                    break;
                case (int)ETalents.INC_HEALING_RATE:
                    talentInfo.Name = "Atrium Healing Enhancement";
                    talentInfo.Desc = "Increases the rate you heal in Atriums by 40% per rank";
                    talentInfo.MaxRank = 3;
                    talentInfo.ResearchCost[0] = 3;
                    talentInfo.WarpsToResearch = 3;
                    break;
                case (int)ETalents.INC_ENEMY_ATRIUM_HEAL:
                    talentInfo.Name = "Cloaked DNA";
                    talentInfo.Desc = "Causes enemy atriums to heal you";
                    talentInfo.MaxRank = 1;
                    talentInfo.ResearchCost[0] = 1;
                    talentInfo.ResearchCost[4] = 2;
                    talentInfo.ResearchCost[5] = 2;
                    talentInfo.WarpsToResearch = 5;
                    break;
                case (int)ETalents.SCI_SCANNER_RESEARCH_MAT:
                    talentInfo.Name = "Scanner Mode: Research Materials";
                    talentInfo.Desc = "Allows the scanner to detect research materials";
                    talentInfo.MaxRank = 1;
                    talentInfo.ResearchCost[3] = 3;
                    talentInfo.ResearchCost[4] = 1;
                    talentInfo.WarpsToResearch = 4;
                    break;
                case (int)ETalents.SCI_SCANNER_PICKUPS:
                    talentInfo.Name = "Scanner Mode: Item Pickups";
                    talentInfo.Desc = "Allows the scanner to detect item pickups";
                    talentInfo.MaxRank = 1;
                    talentInfo.ResearchCost[3] = 1;
                    talentInfo.WarpsToResearch = 2;
                    break;
                case (int)ETalents.PIL_REDUCE_SYS_DAMAGE:
                    talentInfo.Name = "Damage Mitigation";
                    talentInfo.Desc = "Reduces incoming system damage by 5% per rank";
                    talentInfo.MaxRank = 5;
                    talentInfo.ClassID = 1;
                    talentInfo.ResearchCost[1] = 2;
                    talentInfo.ResearchCost[5] = 2;
                    talentInfo.WarpsToResearch = 5;
                    break;
                case (int)ETalents.PIL_REDUCE_HULL_DAMAGE:
                    talentInfo.Name = "Reaction Time";
                    talentInfo.Desc = "Reduces incoming hull damage by 5% per rank";
                    talentInfo.MaxRank = 5;
                    talentInfo.ClassID = 1;
                    talentInfo.ResearchCost[1] = 2;
                    talentInfo.ResearchCost[3] = 2;
                    talentInfo.WarpsToResearch = 5;
                    break;
                case (int)ETalents.INC_ALLOW_ENCUMBERED_SPRINT:
                    talentInfo.Name = "Encumbered Sprint";
                    talentInfo.Desc = "Allows sprinting while encumbered";
                    talentInfo.MaxRank = 1;
                    talentInfo.ResearchCost[1] = 1;
                    talentInfo.WarpsToResearch = 2;
                    break;
                case (int)ETalents.WPNS_BOOST_CREW_TURRET_CHARGE:
                    talentInfo.Name = "Upgraded Turret Power Conduits";
                    talentInfo.Desc = "All friendly ship turrets will charge 4% faster per rank";
                    talentInfo.MaxRank = 5;
                    talentInfo.ClassID = 3;
                    talentInfo.ResearchCost[2] = 3;
                    talentInfo.ResearchCost[4] = 2;
                    talentInfo.WarpsToResearch = 5;
                    break;
                case (int)ETalents.WPNS_BOOST_CREW_TURRET_DAMAGE:
                    talentInfo.Name = "Turret Damage Booster";
                    talentInfo.Desc = "All friendly ship turrets will deal 4% more damage per rank";
                    talentInfo.MaxRank = 5;
                    talentInfo.ClassID = 3;
                    talentInfo.ResearchCost[1] = 3;
                    talentInfo.ResearchCost[5] = 2;
                    talentInfo.WarpsToResearch = 5;
                    break;
                case (int)ETalents.ANTI_RAD_INJECTION:
                    talentInfo.Name = "Anti-Radiation Injection";
                    talentInfo.Desc = "Radiation damage is 8% less per rank";
                    talentInfo.MaxRank = 5;
                    talentInfo.ResearchCost[1] = 3;
                    talentInfo.ResearchCost[3] = 1;
                    talentInfo.WarpsToResearch = 2;
                    break;
                case (int)ETalents.ENG_AUX_POWER_BOOST:
                    talentInfo.Name = "Auxiliary Power";
                    talentInfo.Desc = "Aux systems generate 10% more power per rank";
                    talentInfo.MaxRank = 5;
                    talentInfo.ClassID = 4;
                    talentInfo.ResearchCost[0] = 3;
                    talentInfo.ResearchCost[4] = 2;
                    talentInfo.WarpsToResearch = 4;
                    break;
                case (int)ETalents.CAP_DIPLOMACY:
                    talentInfo.Name = "Diplomacy";
                    talentInfo.Desc = "Diplomatic actions are 5% more likely to succeed per rank";
                    talentInfo.MaxRank = 5;
                    talentInfo.ClassID = 0;
                    break;
                case (int)ETalents.CAP_INTIMIDATION:
                    talentInfo.Name = "Intimidation";
                    talentInfo.Desc = "Intimidation actions are 5% more likely to succeed per rank";
                    talentInfo.MaxRank = 5;
                    talentInfo.ClassID = 0;
                    break;
                case (int)ETalents.ENG_SALVAGE:
                    talentInfo.Name = "Extraction Training";
                    talentInfo.Desc = "Increases chance of succesful extraction by 2% per rank";
                    talentInfo.MaxRank = 5;
                    talentInfo.ClassID = 4;
                    break;
                case (int)ETalents.ENG_COREPOWERBOOST:
                    talentInfo.Name = "Reactor Calibration";
                    talentInfo.Desc = "Increases core reactor output by 2% per rank";
                    talentInfo.MaxRank = 5;
                    talentInfo.ClassID = 4;
                    talentInfo.ResearchCost[0] = 2;
                    talentInfo.ResearchCost[2] = 2;
                    talentInfo.WarpsToResearch = 2;
                    break;
                case (int)ETalents.ENG_CORECOOLINGBOOST:
                    talentInfo.Name = "Reactor Cooling Specialist";
                    talentInfo.Desc = "Decreases core reactor heat output by 2% per rank";
                    talentInfo.MaxRank = 5;
                    talentInfo.ClassID = 4;
                    talentInfo.ResearchCost[1] = 2;
                    talentInfo.ResearchCost[3] = 2;
                    talentInfo.WarpsToResearch = 2;
                    break;
                case (int)ETalents.CAP_SCREEN_DEFENSE:
                    talentInfo.Name = "Screen Defensive Measures";
                    talentInfo.Desc = "Increases friendly screen capture time by 5% per rank";
                    talentInfo.MaxRank = 5;
                    talentInfo.ClassID = 0;
                    talentInfo.ResearchCost[0] = 3;
                    talentInfo.WarpsToResearch = 3;
                    break;
                case (int)ETalents.WPN_SCREEN_HACKER:
                    talentInfo.Name = "Screen Hacker";
                    talentInfo.Desc = "Decreases enemy screen capture time by 5% per rank";
                    talentInfo.MaxRank = 5;
                    talentInfo.ClassID = 3;
                    talentInfo.ResearchCost[4] = 2;
                    talentInfo.WarpsToResearch = 3;
                    break;
                case (int)ETalents.WPN_AMMO_BOOST:
                    talentInfo.Name = "Reloader";
                    talentInfo.Desc = "Small chance of gaining some ammo every shot";
                    talentInfo.MaxRank = 5;
                    talentInfo.ClassID = 3;
                    talentInfo.ResearchCost[5] = 1;
                    talentInfo.WarpsToResearch = 2;
                    break;
                case (int)ETalents.CAP_SCREEN_SAFETY:
                    talentInfo.Name = "Screen Safety Systems";
                    talentInfo.Desc = "Decreases chance of ship screens exploding in battle";
                    talentInfo.MaxRank = 5;
                    talentInfo.ClassID = 0;
                    talentInfo.ResearchCost[5] = 2;
                    talentInfo.WarpsToResearch = 2;
                    break;
                case (int)ETalents.SENSOR_DISH_CERT:
                    talentInfo.Name = "Sensor Dish Certification";
                    talentInfo.Desc = "Allows use of the sensor dish";
                    talentInfo.MaxRank = 1;
                    break;
                case (int)ETalents.SCI_PROBE_XP:
                    talentInfo.Name = "Probe Specialty: XP Gain";
                    talentInfo.Desc = "Increased chance to gain extra XP when performing probe research (10% per rank)";
                    talentInfo.MaxRank = 5;
                    talentInfo.ClassID = 2;
                    talentInfo.ResearchCost[3] = 2;
                    talentInfo.ResearchCost[4] = 2;
                    break;
                case (int)ETalents.SCI_PROBE_COOLDOWN:
                    talentInfo.Name = "Probe Specialty: Cooldown";
                    talentInfo.Desc = "Decreased cooldown and capacitor usage when firing probes (-10% per rank)";
                    talentInfo.MaxRank = 5;
                    talentInfo.ClassID = 2;
                    talentInfo.ResearchCost[3] = 3;
                    talentInfo.ResearchCost[4] = 1;
                    break;
                case (int)ETalents.ITEM_UPGRADER_OPERATOR:
                    talentInfo.Name = "Item Upgrader Operator";
                    talentInfo.Desc = "Allows use of the Item Upgrader";
                    talentInfo.MaxRank = 1;
                    break;
                case (int)ETalents.COMPONENT_UPGRADER_OPERATOR:
                    talentInfo.Name = "Component Upgrader Operator";
                    talentInfo.Desc = "Allows use of the Component Upgrader";
                    talentInfo.MaxRank = 1;
                    break;
                case (int)ETalents.ARMOR_BOOST:
                    talentInfo.Name = "Armor Boost";
                    talentInfo.Desc = "+1 Armor per rank";
                    talentInfo.MaxRank = 5;
                    //talentInfo.GetAdditionalData().ConflictTalent = ETalents.HEALTH_BOOST;
                    break;
                case (int)ETalents.HEALTH_BOOST_2:
                    talentInfo.Name = "Health Boost II";
                    talentInfo.Desc = "+20 to max health per rank";
                    talentInfo.MaxRank = 5;
                    talentInfo.ExtendsTalent = ETalents.HEALTH_BOOST;
                    talentInfo.MinLevel = 6;
                    break;
                case (int)ETalents.ARMOR_BOOST_2:
                    talentInfo.Name = "Armor Boost II";
                    talentInfo.Desc = "+1 Armor per rank";
                    talentInfo.MaxRank = 5;
                    talentInfo.ExtendsTalent = ETalents.ARMOR_BOOST;
                    talentInfo.MinLevel = 6;
                    break;
                case (int)ETalents.WPNS_RELOAD_SPEED:
                    talentInfo.Name = "Quick Reload I";
                    talentInfo.Desc = "+8% to reload speed per rank";
                    talentInfo.MaxRank = 5;
                    break;
                case (int)ETalents.WPNS_RELOAD_SPEED_2:
                    talentInfo.Name = "Quick Reload II";
                    talentInfo.Desc = "+8% to reload speed per rank";
                    talentInfo.MaxRank = 5;
                    talentInfo.ExtendsTalent = ETalents.WPNS_RELOAD_SPEED;
                    talentInfo.MinLevel = 6;
                    break;
                case (int)ETalents.E_TURRET_COOLING_CREW_ENGINEER:
                    talentInfo.Name = "Turret Cooling Optimizations";
                    talentInfo.Desc = "+5% to crew Turret Cooling rate per rank";
                    talentInfo.MaxRank = 5;
                    talentInfo.ClassID = 4;
                    break;
                case (int)ETalents.E_TURRET_COOLING_CREW_WEAPONS:
                    talentInfo.Name = "Turret Cooling Optimizations";
                    talentInfo.Desc = "+5% to crew Turret Cooling rate per rank";
                    talentInfo.MaxRank = 5;
                    talentInfo.ClassID = 3;
                    break;

                default:
                    __result = null;
                    return false;

                case ETalentsPlus.HEALTH_BOOST_3:
                    talentInfo.Name = "Health Boost III";
                    talentInfo.Desc = "+20 to max health per rank";
                    talentInfo.MaxRank = 5;
                    talentInfo.ExtendsTalent = ETalents.HEALTH_BOOST_2;
                    talentInfo.MinLevel = 11;
                    break;
                case ETalentsPlus.HEALTH_BOOST_4:
                    talentInfo.Name = "Health Boost IV";
                    talentInfo.Desc = "+20 to max health per rank";
                    talentInfo.MaxRank = 5;
                    talentInfo.ExtendsTalent = (ETalents)ETalentsPlus.HEALTH_BOOST_3;
                    talentInfo.MinLevel = 16;
                    break;
                case ETalentsPlus.HEALTH_BOOST_5:
                    talentInfo.Name = "Health Boost V";
                    talentInfo.Desc = "+20 to max health per rank";
                    talentInfo.MaxRank = 5;
                    talentInfo.ExtendsTalent = (ETalents)ETalentsPlus.HEALTH_BOOST_4;
                    talentInfo.MinLevel = 21;
                    break;
                case ETalentsPlus.HUMAN_M:
                    talentInfo.Name = "(DEBUG) HUMAN MALE";
                    talentInfo.Desc = "DEBUG";
                    talentInfo.MaxRank = 1;
                    break;
                case ETalentsPlus.HUMAN_F:
                    talentInfo.Name = "(DEBUG) HUMAN FEMALE";
                    talentInfo.Desc = "DEBUG";
                    talentInfo.MaxRank = 1;
                    break;
                case ETalentsPlus.SYLVASSI:
                    talentInfo.Name = "(DEBUG) SYLVASSI";
                    talentInfo.Desc = "DEBUG";
                    talentInfo.MaxRank = 1;
                    break;
                case ETalentsPlus.ROBOT:
                    talentInfo.Name = "(DEBUG) ROBOT";
                    talentInfo.Desc = "DEBUG";
                    talentInfo.MaxRank = 1;
                    break;
            }
            CachedTalentInfos.Add((int)inTalent, talentInfo);
            __result = talentInfo;
            return false;
        }
    }
    public class ETalentsPlus
    {
        public const int HEALTH_BOOST_3 = (int)ETalents.MAX + 1;    // 64
        public const int HEALTH_BOOST_4 = (int)ETalents.MAX + 2;
        public const int HEALTH_BOOST_5 = (int)ETalents.MAX + 3;
        public const int HUMAN_M = (int)ETalents.MAX + 4;
        public const int HUMAN_F = (int)ETalents.MAX + 5;
        public const int SYLVASSI = (int)ETalents.MAX + 6;
        public const int ROBOT = (int)ETalents.MAX + 7;
        public const int MAX = (int)ETalents.MAX + 8;
    }
}
