using HarmonyLib;
using PulsarModLoader;
using PulsarModLoader.Chat.Commands.CommandRouter;
using PulsarModLoader.Content.Components.Reactor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using static PulsarModLoader.Patches.HarmonyHelpers;

namespace Talents
{
    public class Mod : PulsarMod
    {
        public override string HarmonyIdentifier() => "Mest.Talents";
        public override string Author => "Mest";
        public override string Name => "TalentsPlus";
        public override string Version => "0.0.0";

        public static Dictionary<int, List<ETalents>> cachedTalentsForClass = new Dictionary<int, List<ETalents>>();
        public static List<ETalents> TalentsForClass(int inClassID)
        {
            if (cachedTalentsForClass.ContainsKey(inClassID))
            {
                return cachedTalentsForClass[inClassID];
            }
            List<ETalents> list = new List<ETalents>();
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
            switch (inClassID)
            {
                case 0:
                    list.Add(ETalents.CAP_CREW_SPEED_BOOST);
                    list.Add(ETalents.CAP_SCAVENGER);
                    list.Add(ETalents.CAP_ARMOR_BOOST);
                    list.Add(ETalents.CAP_DIPLOMACY);
                    list.Add(ETalents.CAP_INTIMIDATION);
                    list.Add(ETalents.CAP_SCREEN_DEFENSE);
                    list.Add(ETalents.CAP_SCREEN_SAFETY);
                    break;
                case 1:
                    list.Add(ETalents.PIL_SHIP_TURNING);
                    list.Add(ETalents.PIL_SHIP_SPEED);
                    list.Add(ETalents.PIL_REDUCE_SYS_DAMAGE);
                    list.Add(ETalents.PIL_REDUCE_HULL_DAMAGE);
                    list.Add(ETalents.PIL_KEEN_EYES);
                    break;
                case 2:
                    list.Add(ETalents.SCI_HEAL_NEARBY);
                    list.Add(ETalents.SCI_SENSOR_BOOST);
                    list.Add(ETalents.SCI_SENSOR_HIDE);
                    list.Add(ETalents.SCI_FREQ_AMPLIFIER);
                    list.Add(ETalents.SCI_RESEARCH_SPECIALTY);
                    list.Add(ETalents.SCI_PROBE_COOLDOWN);
                    list.Add(ETalents.SCI_PROBE_XP);
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
                    break;
            }
            cachedTalentsForClass[inClassID] = list;
            return list;
        }

        public static Dictionary<int, List<ETalents>> cachedTalentsForSpecies = new Dictionary<int, List<ETalents>>();
        public static List<ETalents> TalentsForSpecies(PLPlayer Player)
        {
            bool Gender_IsMale = Player.Gender_IsMale; // True if not Human
            int RaceID = Player.RaceID; // 0 = Human, 1 = Sylvassi, 2 = Robot
            if (RaceID == 0 && Gender_IsMale) RaceID = 3; // Makes Female as RaceID 3 (Local)
            if (cachedTalentsForSpecies.ContainsKey(RaceID))
            {
                return cachedTalentsForSpecies[RaceID];
            }
            List<ETalents> list = new List<ETalents>();
            // Default Species Talents
            list.Add(ETalents.HEALTH_BOOST);
            list.Add(ETalents.ARMOR_BOOST);
            list.Add(ETalents.HEALTH_BOOST_2);
            list.Add((ETalents)ETalentsPlus.HEALTH_BOOST_3);
            list.Add((ETalents)ETalentsPlus.HEALTH_BOOST_4);
            list.Add((ETalents)ETalentsPlus.HEALTH_BOOST_5);
            list.Add(ETalents.ARMOR_BOOST_2);
            list.Add(ETalents.OXYGEN_TRAINING);
            switch (RaceID)
            {
                case 0:     // Human Male
                    //list.Add(ETalents.PIL_SHIP_TURNING);
                    break;
                case 3:     // Human Female
                    //list.Add(ETalents.PIL_SHIP_TURNING);
                    break;
                case 1:     // Sylvassi
                    //list.Add(ETalents.PIL_SHIP_TURNING);
                    break;
                case 2:     // Robot
                    //list.Add(ETalents.PIL_SHIP_TURNING);
                    break;
            }
            cachedTalentsForSpecies[RaceID] = list;
            return list;
        }
    }
    public class Command : ChatCommand
    {
        public override string[] CommandAliases() => new string[] { "gettalents" };

        public override string Description() => "Display All Talents";

        public override void Execute(string arguments)
        {
            for (int i = 0; i < ETalentsPlus.MAX + 1; i++)
            {
                TalentInfo talent = PLGlobal.GetTalentInfoForTalentType((ETalents)i);
                if (talent != null)
                {
                    PulsarModLoader.Utilities.Logger.Info($"[TALENTS] {i} - {talent.Name} - ID: {talent.TalentID}");
                }
            }
        }
    }
    public class Command2 : ChatCommand
    {
        public override string[] CommandAliases() => new string[] { "getlocaltalents" };

        public override string Description() => "Display LOCAL Talents";

        public override void Execute(string arguments)
        {
            PLPlayer Player = PLNetworkManager.Instance.LocalPlayer;
            int Count = 0;
            foreach (int i in Player.Talents)
            {
                TalentInfo talent = PLGlobal.GetTalentInfoForTalentType((ETalents)Count);
                if (talent != null)
                {
                    PulsarModLoader.Utilities.Logger.Info($"[TALENTS] {talent.Name} - ID: {talent.TalentID} - LEVEL: {i}");
                }
                Count++;
            }
        }
    }
}
