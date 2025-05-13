using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using static PulsarModLoader.Patches.HarmonyHelpers;

namespace Talents.Rework
{
    // Change what Talents are available for a player to use.
    [HarmonyPatch(typeof(PLTabMenu), "UpdateTDs")]
    class TalentAssignmentPatch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> target = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldloc_S),       // playerFromPlayerID
                new CodeInstruction(OpCodes.Callvirt),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PLGlobal), "TalentsForClass", new Type[] { typeof(Int32) })),
            };
            int NextInstruction = FindSequence(instructions, target, CheckMode.NONNULL);
            List<CodeInstruction> patch = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldarg_0),       // PLTabMenu Instance
                instructions.ToList()[NextInstruction - 3], // playerFromPlayerID
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TalentAssignmentPatch), "Patch", new Type[] { typeof(PLTabMenu), typeof(PLPlayer) }))
            };
            return PatchBySequence(instructions, target, patch, PatchMode.REPLACE, CheckMode.NONNULL);
        }
        public static List<ETalents> Patch(PLTabMenu instance, PLPlayer pLPlayer)
        {
            return TalentCreation.TalentsForClassSpecies(pLPlayer, pLPlayer.GetClassID());
        }
    }
}
