using PulsarModLoader;
using System;
using System.Collections.Generic;
using System.Reflection;
using PulsarModLoader.Utilities;
using HarmonyLib;
using CodeStage.AntiCheat.ObscuredTypes;
using static PulsarModLoader.Patches.HarmonyHelpers;
using static HarmonyLib.AccessTools;
using System.Reflection.Emit;

namespace Talents.Framework
{
    public class TalentModManager
    {
        public enum CharacterClass
        {
            General = -1,
            Captain,
            Pilot,
            Scientist,
            Weapons,
            Engineer
        }
        public enum CharacterSpecies
        {
            General = -1,
            Human,
            Sylvassi,
            Robot
        }

        public readonly int vanillaTalentMaxType = 0;
        private static TalentModManager m_instance = null;
        public readonly List<TalentMod> TalentTypes = new List<TalentMod>();
        public static TalentModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new TalentModManager();
                }
                return m_instance;
            }
        }

        TalentModManager()
        {
            vanillaTalentMaxType = Enum.GetValues(typeof(ETalents)).Length;
            Logger.Info($"MaxTypeint = {vanillaTalentMaxType - 1}");
            foreach (PulsarMod mod in ModManager.Instance.GetAllMods())
            {
                Assembly asm = mod.GetType().Assembly;
                Type talentMod = typeof(TalentMod);
                foreach (Type t in asm.GetTypes())
                {
                    if (talentMod.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    {
                        Logger.Info("Loading Talent Mod from assembly");
                        TalentMod talentModHandler = (TalentMod)Activator.CreateInstance(t);
                        if (GetTalentIDFromName(talentModHandler.Name) == -1)
                        {
                            TalentTypes.Add(talentModHandler);
                            TalentCreation.cachedTalents[talentModHandler.TalentAssignment].Add((ETalents)GetTalentIDFromName(talentModHandler.Name));
                            Logger.Info($"Added Talent: '{talentModHandler.Name}' with ID '{GetTalentIDFromName(talentModHandler.Name)}'");
                        }
                        else
                        {
                            Logger.Info($"Could not add Talent from {mod.Name} with the duplicate name of '{talentModHandler.Name}'");
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Finds Talent ID equivilent to given name. Returns -1 if couldn't find Talent.
        /// </summary>
        /// <param name="TalentName">Name of Talent</param>
        /// <returns>ID of Talent</returns>
        public int GetTalentIDFromName(string TalentName)
        {
            for (int i = 0; i < TalentTypes.Count; i++)
            {
                if (TalentTypes[i].Name == TalentName)
                {
                    return i + vanillaTalentMaxType;
                }
            }
            return -1;
        }
    }

    // Makes new Talent Infos retrievable
    [HarmonyPatch(typeof(PLGlobal), "GetTalentInfoForTalentType")]
    class TalentInfoFix
    {
        static bool Prefix(ETalents inTalent, ref TalentInfo __result)
        {
            int subtypeformodded = (int)inTalent - TalentModManager.Instance.vanillaTalentMaxType;
            if (subtypeformodded <= TalentModManager.Instance.TalentTypes.Count && subtypeformodded > -1)
            {
                __result = TalentModManager.Instance.TalentTypes[subtypeformodded].TalentInfo;
                return false;
            }
            return true;
        }
    }

    // Increases the array size of Talents
    [HarmonyPatch(typeof(PLPlayer), "Start")]
    class StartTalentSizePatch
    {
        static void Postfix(PLPlayer __instance)
        {
            int TalentMaxSize = Enum.GetValues(typeof(ETalents)).Length + TalentModManager.Instance.TalentTypes.Count;
            __instance.Talents = new ObscuredInt[TalentMaxSize];
            __instance.TalentsLocalEditTime = new float[TalentMaxSize];
        }
    }

    // Allows new Talents to be researchable
    [HarmonyPatch(typeof(PLShipInfo), "UpdateResearchTalentChoices")]
    class ResearchTalentSizePatch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return PatchBySequence(instructions,
            new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldc_I4_S, 0x3F),
            },
            new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Call, Method(typeof(ResearchTalentSizePatch), "Patch"))
            },
            PatchMode.REPLACE);
        }
        static int Patch() => Enum.GetValues(typeof(ETalents)).Length + TalentModManager.Instance.TalentTypes.Count;
    }

    // Allows new Talents to be synced with joining players
    [HarmonyPatch(typeof(PLPlayer), "SendTalentsToPhotonTargets")]
    class TalentSerializePatch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return PatchBySequence(instructions,
            new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldc_I4_S, 0x3F),
            },
            new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Call, Method(typeof(TalentSerializePatch), "Patch"))
            },
            PatchMode.REPLACE);
        }
        static int Patch() => Enum.GetValues(typeof(ETalents)).Length + TalentModManager.Instance.TalentTypes.Count;
    }
}
