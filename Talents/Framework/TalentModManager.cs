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
using ServerSerializeExtension;
using System.IO;

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
            Logger.Info($"Talents Vanilla MaxTypeint = {vanillaTalentMaxType - 1}");
            extraTalentStatuses.Add(0, 0L);
            foreach (PulsarMod mod in ModManager.Instance.GetAllMods())
            {
                Assembly asm = mod.GetType().Assembly;
                Type talentMod = typeof(TalentMod);
                foreach (Type t in asm.GetTypes())
                {
                    if (talentMod.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    {
                        TalentMod talentModHandler = (TalentMod)Activator.CreateInstance(t);
                        if (GetTalentIDFromName(talentModHandler.Name) == -1)
                        {
                            TalentTypes.Add(talentModHandler);
                            int newTalentID = GetTalentIDFromName(talentModHandler.Name);
                            TalentCreation.cachedTalents[talentModHandler.TalentAssignment].Add((ETalents)newTalentID);
                            Logger.Info($"Added Talent: '{talentModHandler.Name}' with ID '{newTalentID}'");
                            if (newTalentID / 64 > extraTalentStatuses.Keys.Count)
                            {   // Extend the ObscureLong TalentLockedStatus for > 64 talents
                                extraTalentStatuses.Add(extraTalentStatuses.Keys.Count, 0L);
                            }
                            if (talentModHandler.NeedsToBeResearched)
                            {
                                LockTalent(newTalentID);
                                Logger.Info($"Talent '{talentModHandler.Name}' is set to NEED TO BE RESEARCHED.");
                            }
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

        /// <summary>
        /// ObscuredLong PLServer.TalentLockedStatus is length limited to 64. This allows that to be extended.
        /// </summary>
        public Dictionary<int, ObscuredLong> extraTalentStatuses = new Dictionary<int, ObscuredLong>();

        /// <summary>
        /// Sets talentID bit location in the TalentLockedStatus ObscuredLong to be locked.
        /// </summary>
        /// <param name="talentID">ID of Talent</param>
        public void LockTalent(int talentID)
        {
            int index = talentID / 64;
            int bitPosition = talentID % 64;

            if (index != 0) // Skip index == 0 as that is the default 64 range
            {
                long mask = 1L << bitPosition;
                extraTalentStatuses[index] |= mask; // Set bit to 1 (locked)
            }
        }

        /// <summary>
        /// Sets talentID bit location in the TalentLockedStatus ObscuredLong to be Unlocked.
        /// </summary>
        /// <param name="talentID">ID of Talent</param>
        public void UnlockTalent(int talentID)
        {
            int index = talentID / 64;
            int bitPosition = talentID % 64;

            if (index != 0) // Skip index == 0 as that is the default 64 range
            {
                long mask = 1L << bitPosition;
                extraTalentStatuses[index] &= ~mask; // Set bit to 0 (unlocked)
            }
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

    [HarmonyPatch(typeof(PLServer), "IsTalentUnlocked")]
    class ExtendIsTalentUnlocked
    {
        static bool Prefix(ETalents inTalent, ref bool __result)
        {
            int index = (int)inTalent / 64;
            int bitPosition = (int)inTalent % 64;

            if (index == 0) return true;
            else
            {
                if (!TalentModManager.Instance.extraTalentStatuses.TryGetValue(index, out ObscuredLong status))
                {
                    __result = true;
                    return false;
                }
                long mask = 1L << bitPosition;
                __result = (status & mask) == 0L;
                return false;
            }
        }
    }

    [HarmonyPatch(typeof(PLServer), "SetTalentAsUnlocked")]
    class ExtendSetTalentAsUnlocked
    {
        static bool Prefix(int inTalentID)
        {
            int index = inTalentID / 64;
            int bitPosition = inTalentID % 64;
            if (index == 0)
            {
                return true;
            }
            else
            {
                if (!TalentModManager.Instance.extraTalentStatuses.TryGetValue(index, out ObscuredLong status))
                {
                    return false;
                }
                long mask = 1L << bitPosition;
                TalentModManager.Instance.extraTalentStatuses[index] &= ~mask; // Set bit to 0 (unlocked)
                return false;
            }
        }
    }

    [HarmonyPatch(typeof(PLServer), "ResetTalentLockedStatus")]
    class ExtendResetTalentLockedStatus
    {
        static void Postfix()
        {
            for (int i = 0; i < TalentModManager.Instance.extraTalentStatuses.Count; i++)
            {
                TalentModManager.Instance.extraTalentStatuses[i] = 0L;
            }
            foreach (TalentMod talentMod in TalentModManager.Instance.TalentTypes)
            {
                if (talentMod != null && talentMod.NeedsToBeResearched)
                {
                    TalentModManager.Instance.LockTalent(TalentModManager.Instance.GetTalentIDFromName(talentMod.Name));
                }
            }
        }
    }
    class SyncTalentResearchStatus : PLServerSerialize
    {
        private int count = 0;
        private int statusCount;
        public override int Receive(object[] receiving)
        {
            switch (count)
            {
                case 0:
                    statusCount = (int)receiving[0];
                    count = 1;
                    return statusCount * 2;
                case 1:
                    for (int i = 0; i < statusCount * 2; i += 2)
                    {
                        int key = (int)receiving[i];
                        long value = (long)receiving[i + 1];
                        TalentModManager.Instance.extraTalentStatuses[key] = value;
                    }
                    return 0;
                default:
                    return 0;
            }
        }

        public override int ReceiveFirst()
        {
            count = 0;
            return 1;
        }

        public override object[] Send()
        {
            List<object> result = new List<object>();
            result.Add(TalentModManager.Instance.extraTalentStatuses.Count);
            foreach (var status in TalentModManager.Instance.extraTalentStatuses)
            {
                result.Add(status.Key);
                result.Add(status.Value);
            }
            return result.ToArray();
        }
    }
}
