using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talents
{
    internal class TalentCreation
    {
        public static Dictionary<int, Dictionary<int, List<ETalents>>> cachedTalentsForClassSpecies = new Dictionary<int, Dictionary<int, List<ETalents>>>(5);
        public static List<ETalents> TalentsForClassSpecies(PLPlayer pLPlayer, int ClassID = -1)
        {
            if (!Mod.ModEnabled) return PLGlobal.TalentsForClass(ClassID);
            int RaceID = 0;
            if (ClassID != -1 && pLPlayer != null)
            {
                RaceID = pLPlayer.RaceID; // 0 = Human, 1 = Sylvassi, 2 = Robot
                if (RaceID == 0 && !pLPlayer.Gender_IsMale) RaceID = 3; // Makes Female as RaceID 3 (Local)
            }
            if (cachedTalentsForClassSpecies.ContainsKey(ClassID) && cachedTalentsForClassSpecies[ClassID].ContainsKey(RaceID))
            {
                return cachedTalentsForClassSpecies[ClassID][RaceID];
            }
            List<ETalents> list = new List<ETalents>();
            list.Add(ETalents.HEALTH_BOOST);
            list.Add(ETalents.ARMOR_BOOST);
            list.Add(ETalents.HEALTH_BOOST_2);
            list.Add((ETalents)ETalentsPlus.HEALTH_BOOST_3);
            list.Add((ETalents)ETalentsPlus.HEALTH_BOOST_4);
            list.Add((ETalents)ETalentsPlus.HEALTH_BOOST_5);
            list.Add(ETalents.ARMOR_BOOST_2);
            list.Add(ETalents.PISTOL_DMG_BOOST);
            list.Add(ETalents.QUICK_RESPAWN);
            list.Add(ETalents.ADVANCED_OPERATOR);
            list.Add(ETalents.SENSOR_DISH_CERT);
            list.Add(ETalents.WPNS_RELOAD_SPEED);
            list.Add(ETalents.WPNS_RELOAD_SPEED_2);
            list.Add(ETalents.INC_STAMINA);
            list.Add(ETalents.INC_MAX_WEIGHT);
            list.Add(ETalents.INC_JETPACK);
            list.Add(ETalents.INC_TURRET_ZOOM);
            list.Add(ETalents.INC_HEALING_RATE);
            list.Add(ETalents.INC_ENEMY_ATRIUM_HEAL);
            list.Add(ETalents.INC_ALLOW_ENCUMBERED_SPRINT);
            list.Add(ETalents.ANTI_RAD_INJECTION);
            list.Add(ETalents.SCI_SCANNER_PICKUPS);
            list.Add(ETalents.SCI_SCANNER_RESEARCH_MAT);
            list.Add(ETalents.ITEM_UPGRADER_OPERATOR);
            list.Add(ETalents.COMPONENT_UPGRADER_OPERATOR);
            switch (RaceID)
            {
                default:
                    break;
                case 0:     // Human Male
                    list.Add(ETalents.OXYGEN_TRAINING);
                    list.Add((ETalents)ETalentsPlus.HUMAN_M);
                    break;
                case 3:     // Human Female
                    list.Add(ETalents.OXYGEN_TRAINING);
                    list.Add((ETalents)ETalentsPlus.HUMAN_F);
                    break;
                case 1:     // Sylvassi
                    list.Add((ETalents)ETalentsPlus.SYLVASSI);
                    break;
                case 2:     // Robot
                    list.Add((ETalents)ETalentsPlus.ROBOT);
                    break;
            }
            switch (ClassID)
            {
                case 0:
                    list.Add(ETalents.CAP_CREW_SPEED_BOOST);
                    list.Add(ETalents.CAP_SCAVENGER);
                    list.Add(ETalents.CAP_ARMOR_BOOST);
                    list.Add(ETalents.CAP_DIPLOMACY);
                    list.Add(ETalents.CAP_INTIMIDATION);
                    list.Add(ETalents.CAP_SCREEN_DEFENSE);
                    list.Add(ETalents.CAP_SCREEN_SAFETY);
                    switch (RaceID)
                    {
                        default:
                            break;
                        case 0:     // Human Male
                            break;
                        case 3:     // Human Female
                            break;
                        case 1:     // Sylvassi
                            break;
                        case 2:     // Robot
                            break;
                    }
                    break;
                case 1:
                    list.Add(ETalents.PIL_SHIP_TURNING);
                    list.Add(ETalents.PIL_SHIP_SPEED);
                    list.Add(ETalents.PIL_REDUCE_SYS_DAMAGE);
                    list.Add(ETalents.PIL_REDUCE_HULL_DAMAGE);
                    list.Add(ETalents.PIL_KEEN_EYES);
                    switch (RaceID)
                    {
                        default:
                            break;
                        case 0:     // Human Male
                            break;
                        case 3:     // Human Female
                            break;
                        case 1:     // Sylvassi
                            break;
                        case 2:     // Robot
                            break;
                    }
                    break;
                case 2:
                    list.Add(ETalents.SCI_HEAL_NEARBY);
                    list.Add(ETalents.SCI_SENSOR_BOOST);
                    list.Add(ETalents.SCI_SENSOR_HIDE);
                    list.Add(ETalents.SCI_FREQ_AMPLIFIER);
                    list.Add(ETalents.SCI_RESEARCH_SPECIALTY);
                    list.Add(ETalents.SCI_PROBE_COOLDOWN);
                    list.Add(ETalents.SCI_PROBE_XP);
                    switch (RaceID)
                    {
                        default:
                            break;
                        case 0:     // Human Male
                            break;
                        case 3:     // Human Female
                            break;
                        case 1:     // Sylvassi
                            break;
                        case 2:     // Robot
                            break;
                    }
                    break;
                case 3:
                    list.Add(ETalents.WPNS_TURRET_BOOST);
                    list.Add(ETalents.WPNS_MISSILE_EXPERT);
                    list.Add(ETalents.WPNS_COOLING);
                    list.Add(ETalents.WPNS_REDUCE_PAWN_DMG);
                    list.Add(ETalents.WPNS_BOOST_CREW_TURRET_CHARGE);
                    list.Add(ETalents.WPNS_BOOST_CREW_TURRET_DAMAGE);
                    list.Add(ETalents.WPN_SCREEN_HACKER);
                    list.Add(ETalents.WPN_AMMO_BOOST);
                    list.Add(ETalents.E_TURRET_COOLING_CREW_WEAPONS);
                    switch (RaceID)
                    {
                        default:
                            break;
                        case 0:     // Human Male
                            break;
                        case 3:     // Human Female
                            break;
                        case 1:     // Sylvassi
                            break;
                        case 2:     // Robot
                            break;
                    }
                    break;
                case 4:
                    list.Add(ETalents.ENG_COOLANT_MIX_CUSTOM);
                    list.Add(ETalents.ENG_FIRE_REDUCTION);
                    list.Add(ETalents.ENG_REPAIR_DRONES);
                    list.Add(ETalents.ENG_WARP_CHARGE_BOOST);
                    list.Add(ETalents.ENG_AUX_POWER_BOOST);
                    list.Add(ETalents.ENG_SALVAGE);
                    list.Add(ETalents.ENG_COREPOWERBOOST);
                    list.Add(ETalents.ENG_CORECOOLINGBOOST);
                    list.Add(ETalents.E_TURRET_COOLING_CREW_ENGINEER);
                    switch (RaceID)
                    {
                        default:
                            break;
                        case 0:     // Human Male
                            break;
                        case 3:     // Human Female
                            break;
                        case 1:     // Sylvassi
                            break;
                        case 2:     // Robot
                            break;
                    }
                    break;
            }
            if (cachedTalentsForClassSpecies.ContainsKey(ClassID))
            {
                if (cachedTalentsForClassSpecies[ClassID].ContainsKey(RaceID))
                {
                    cachedTalentsForClassSpecies[ClassID][RaceID] = list;
                }
                else
                {
                    cachedTalentsForClassSpecies[ClassID].Add(RaceID, list);
                }
            }
            else
            {
                cachedTalentsForClassSpecies.Add(ClassID, new Dictionary<int, List<ETalents>> { { RaceID, list } });
            }
            return list;
        }
    }
}
