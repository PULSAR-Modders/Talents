using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using PulsarModLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talents.Framework
{
    // Makes new Talent Infos retrievable
    [HarmonyPatch(typeof(PLGlobal), "GetTalentInfoForTalentType")]
    class TalentInfoFix
    {
        static bool Prefix(ETalents inTalent, ref TalentInfo __result)
        {
            //PulsarModLoader.Utilities.Logger.Info($"[GetTalentInfoForTalentType] {inTalent}");
            int subtypeformodded = (int)inTalent - TalentModManager.Instance.vanillaTalentMaxType;
            if (subtypeformodded <= TalentModManager.Instance.TalentTypes.Count && subtypeformodded > -1)
            {
                __result = TalentModManager.Instance.TalentTypes[subtypeformodded].TalentInfo;
                return false;
            }
            //PulsarModLoader.Utilities.Logger.Info($"[GetTalentInfoForTalentType] is vanilla");
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
    public class ResearchTalentSizePatch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => HelperMethods.Override63Transpiler(instructions);
    }

    [HarmonyPatch(typeof(PLShipInfo), "Update")]
    public class ResearchTalentSizePatch2
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => HelperMethods.Override63Transpiler(instructions);
    }

    // Allows new Talents to be synced with joining players
    [HarmonyPatch(typeof(PLPlayer), "SendTalentsToPhotonTargets")]
    public class TalentSerializePatch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => HelperMethods.Override63Transpiler(instructions);
    }

    // Talents are unlocked based on the synced ObscuredLong this.TalentLockedStatus - Each bit represents one of the 64 talents, 1 = locked 0 = unlocked.
    // Since we are adding talents >64 we need to add to check and create our own ObscuredLong.
    #region Lock/Unlock Talents
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
                //PulsarModLoader.Utilities.Logger.Info($"[IsTalentUnlocked] {inTalent} = {__result}");
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
    /*
     * Need to find a way to sync our list of ObscuredLongs akin to the Serialization on PLServer!!
    */

    /*class SyncTalentResearchStatus : PLServerSerialize
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
    }*/
    #endregion
}
