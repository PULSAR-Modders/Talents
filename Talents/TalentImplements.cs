using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using static PulsarModLoader.Patches.HarmonyHelpers;
using static UIWidget;
using UnityEngine;
using Talents;
using System.Linq;

namespace TalentsImplements
{
    [HarmonyPatch(typeof(PLPawn), "Update")]
    class HEALTH_BOOST
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> target = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Stloc_S),
                new CodeInstruction(OpCodes.Ldloc_S),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Callvirt),
                new CodeInstruction(OpCodes.Ldfld),
                new CodeInstruction(OpCodes.Ldc_I4_0),
                new CodeInstruction(OpCodes.Ldelem),
                new CodeInstruction(OpCodes.Call),
                new CodeInstruction(OpCodes.Conv_R4),
                new CodeInstruction(OpCodes.Ldc_R4),
                new CodeInstruction(OpCodes.Mul),
                new CodeInstruction(OpCodes.Add),
                new CodeInstruction(OpCodes.Stloc_S),
            };
            int NextInstruction = FindSequence(instructions, target, CheckMode.NEVER);
            List<CodeInstruction> ListInstructions = instructions.ToList();
            List<CodeInstruction> patch = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldarg_0),   // Instance
                ListInstructions[NextInstruction],      // num11
                ListInstructions[NextInstruction + 1],  // Instance
                ListInstructions[NextInstruction + 2],  // GetPlayer
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HEALTH_BOOST), "Replacement")),
                ListInstructions[NextInstruction - 1]   // Store Value
            };
            return PatchBySequence(instructions, target, patch, PatchMode.AFTER, CheckMode.NEVER);
        }
        public static float Replacement(PLPawn Instance, float MaxHealth, PLPlayer pLPlayer)
        {
            if (pLPlayer.Talents.Length != ETalentsPlus.MAX + 1)
            {
                return MaxHealth;
            }
            float maxHealth = MaxHealth;
            maxHealth += (float)pLPlayer.Talents[ETalentsPlus.HEALTH_BOOST_3] * 20f;
            maxHealth += (float)pLPlayer.Talents[ETalentsPlus.HEALTH_BOOST_4] * 20f;
            maxHealth += (float)pLPlayer.Talents[ETalentsPlus.HEALTH_BOOST_5] * 20f;
            return maxHealth;
        }
    }
}
