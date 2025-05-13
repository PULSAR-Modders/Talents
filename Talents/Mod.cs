using HarmonyLib;
using PulsarModLoader;
using System.Collections.Generic;
using System.Reflection.Emit;
using System;
using Talents.Framework;
using static PulsarModLoader.Patches.HarmonyHelpers;
using static HarmonyLib.AccessTools;

namespace Talents
{
    public class Mod : PulsarMod
    {
        public override string HarmonyIdentifier() => "Mest.Talents";
        public override string Author => "Mest";
        public override string Name => "Talents";
        public override string Version => "0.0.0";
    }

    public class HelperMethods
    {
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
            PatchMode.REPLACE);
        }
        public static int Override63TranspilerPatch() => Enum.GetValues(typeof(ETalents)).Length + TalentModManager.Instance.TalentTypes.Count;
    }

    [HarmonyPatch(typeof(PLGlobal), "Start")]
    class InjectTalents
    {
        static void Postfix()
        {
            _ = TalentModManager.Instance;
        }
    }
            /*
            [Serializable]
            public class TalentInfoAdditionalData
            {
                public ETalents[] ConflictTalents; // This is called with: TalentInfo __instance.GetAdditionalData().ConflictTalents
                public TalentInfoAdditionalData()
                {
                    ConflictTalents = null;
                }
            }
            /// <summary>
            /// The below class adds the custom class to the existing class TalentInfo
            /// </summary>
            public static class TalentInfoExtension
            {
                private static readonly ConditionalWeakTable<TalentInfo, TalentInfoAdditionalData> data = new ConditionalWeakTable<TalentInfo, TalentInfoAdditionalData>();
                public static TalentInfoAdditionalData GetAdditionalData(this TalentInfo Talent)
                {
                    return data.GetOrCreateValue(Talent);
                }
                public static void AddData(this TalentInfo Talent, TalentInfoAdditionalData value)
                {
                    try
                    {
                        data.Add(Talent, value);
                    }
                    catch (Exception) { }
                }
            }
            */
}
