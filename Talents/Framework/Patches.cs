using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using static PulsarModLoader.Patches.HarmonyHelpers;
using static HarmonyLib.AccessTools;
using System.Linq;
using static UnityEngine.EventSystems.EventTrigger;
using PulsarModLoader.SaveData;
using System.IO;

namespace Talents.Framework
{
    public class HelperMethods
    {
        // Finds and overrides int 63 values, these are usually used to iterate to maximum talent count
        public static IEnumerable<CodeInstruction> Override63Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return PatchBySequence(instructions,
            new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)63),
            },
            new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Call, Method(typeof(HelperMethods), "Override63TranspilerPatch"))
            },
            PatchMode.REPLACE, showDebugOutput:true);
        }
        public static int Override63TranspilerPatch() => Enum.GetValues(typeof(ETalents)).Length + TalentModManager.Instance.TalentTypes.Count;
    }

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

    /*[HarmonyPatch(typeof(PLTabMenu), "PressTD")]
    class rankTalentMessage
    {
        static void Prefix(PLTabMenu.TalentDisplay inTD)
        {
            TalentInfo talentInfoForTalentType = PLGlobal.GetTalentInfoForTalentType(inTD.MyType);
            PulsarModLoader.Utilities.Messaging.Echo(PhotonTargets.All, $"RankTalent {inTD.MyType} - {talentInfoForTalentType.Name} - {talentInfoForTalentType.TalentID}");
        }
    }*/

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
    
    // Patches PLServer Serialze to add syncing of locked talents
    [HarmonyPatch(typeof(PLServer), "OnPhotonSerializeView")]
    public class SyncLockedTalents
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler_SendSync(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionsList = instructions.ToList();
            List<CodeInstruction> targetSequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, Field(typeof(PLServer), "JumpsNeededToResearchTalent")),
                new CodeInstruction(OpCodes.Call),
                new CodeInstruction(OpCodes.Box),
                new CodeInstruction(OpCodes.Callvirt, Method(typeof(PhotonStream), "SendNext")),
                new CodeInstruction(OpCodes.Br),
            };
            int location = FindSequence(instructions, targetSequence, CheckMode.NONNULL);
            return PatchBySequence(instructions,
            targetSequence,
            new List<CodeInstruction>()
            {
                instructionsList[location-7], // Ldarg_1
                instructionsList[location-6], // Ldarg_0
                instructionsList[location-5], // Ldfld - PLServer JumpsNeededToResearchTalent
                instructionsList[location-4], // Call
                instructionsList[location-3], // Box
                instructionsList[location-2], // Callvirt - PhotonStream SendNext
                instructionsList[location-7], // Ldarg_1
                new CodeInstruction(OpCodes.Call, Method(typeof(SyncLockedTalents), "PatchSend")),
                instructionsList[location-1], // Br
            },
            PatchMode.REPLACE, CheckMode.NONNULL);
        }
        public static void PatchSend(PhotonStream stream)
        {
            Dictionary<int, ObscuredLong> newDict = TalentModManager.Instance.extraTalentStatuses;
            stream.SendNext(newDict.Count);
            foreach (var kvp in newDict)
            {
                stream.SendNext(kvp.Key);
                stream.SendNext((long)kvp.Value);
            }
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler_RecieveSync(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionsList = instructions.ToList();
            List<CodeInstruction> targetSequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Callvirt, Method(typeof(PhotonStream), "ReceiveNext")),
                new CodeInstruction(OpCodes.Unbox_Any),
                new CodeInstruction(OpCodes.Call),
                new CodeInstruction(OpCodes.Stfld, Field(typeof(PLServer), "JumpsNeededToResearchTalent")),
                new CodeInstruction(OpCodes.Br),
            };
            int location = FindSequence(instructions, targetSequence, CheckMode.NONNULL);
            return PatchBySequence(instructions,
            targetSequence,
            new List<CodeInstruction>()
            {
                instructionsList[location-7], // Ldarg_0
                instructionsList[location-6], // Ldarg_1
                instructionsList[location-5], // Callvirt - PhotonStream ReceiveNext
                instructionsList[location-4], // Unbox_Any
                instructionsList[location-3], // Call
                instructionsList[location-2], // Stfld - PLServer JumpsNeededToResearchTalent
                instructionsList[location-6], // Ldarg_1
                new CodeInstruction(OpCodes.Call, Method(typeof(SyncLockedTalents), "PatchRecieve")),
                instructionsList[location-1], // Br
            },
            PatchMode.REPLACE, CheckMode.NONNULL);
        }
        public static void PatchRecieve(PhotonStream stream)
        {
            int count = (int)stream.ReceiveNext();
            var newDict = new Dictionary<int, ObscuredLong>();

            for (int i = 0; i < count; i++)
            {
                int key = (int)stream.ReceiveNext();
                long value = (long)stream.ReceiveNext();
                newDict[key] = value;
            }

            TalentModManager.Instance.extraTalentStatuses = newDict;
        }
    }

    // Implements saving of locked talents
    class LockedTalentsSaveData : PMLSaveData
    {
        public override string Identifier()
        {
            return "LockedTalentsSaveData";
        }

        public override void LoadData(byte[] Data, uint VersionID)
        {
            var dict = new Dictionary<int, ObscuredLong>();

            using (MemoryStream ms = new MemoryStream(Data))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                int count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    int key = reader.ReadInt32();
                    long value = reader.ReadInt64();
                    dict[key] = value;
                }
            }

            TalentModManager.Instance.extraTalentStatuses = dict;
        }

        public override byte[] SaveData()
        {
            Dictionary<int, ObscuredLong> dict = TalentModManager.Instance.extraTalentStatuses;

            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                writer.Write(dict.Count);
                foreach (var kvp in dict)
                {
                    writer.Write(kvp.Key);
                    writer.Write((long)kvp.Value);
                }
                return ms.ToArray();
            }
        }
    }
    #endregion
}
