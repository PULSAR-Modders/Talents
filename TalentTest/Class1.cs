using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Talents.Framework;
using static PulsarModLoader.Patches.HarmonyHelpers;

namespace TalentTest
{
    internal class Health_Boost_3 : TalentMod
    {
        public override string Name => "Health Boost III";
        public override string Description => "+20 to max health per rank";
        public override int MaxRank => 5;
        public override ETalents ExtendsDefaultTalent => ETalents.HEALTH_BOOST_2;
        public override int MinLevel => 11;
        public override int ClassID => 0;
        public override (TalentModManager.CharacterClass, TalentModManager.CharacterSpecies) TalentAssignment => (TalentModManager.CharacterClass.General, TalentModManager.CharacterSpecies.Human);
    }
    internal class Health_Boost_4 : TalentMod
    {
        public override string Name => "Health Boost IV";
        public override string Description => "+20 to max health per rank";
        public override int MaxRank => 5;
        public override string ExtendsModdedTalent => "Health Boost III";
        public override int MinLevel => 16;
        public override int ClassID => 0;
        public override (TalentModManager.CharacterClass, TalentModManager.CharacterSpecies) TalentAssignment => (TalentModManager.CharacterClass.General, TalentModManager.CharacterSpecies.Human);
    }

    [HarmonyPatch(typeof(PLPawn), "Update")]
    class Health_Boost_Patch
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
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Health_Boost_Patch), "Patch")),
                ListInstructions[NextInstruction - 1]   // Store Value
            };
            return PatchBySequence(instructions, target, patch, PatchMode.AFTER, CheckMode.NEVER);
        }
        public static float Patch(PLPawn Instance, float MaxHealth, PLPlayer pLPlayer)
        {
            if (pLPlayer.PreviewPlayer) return MaxHealth;
            float maxHealth = MaxHealth;
            maxHealth += (float)pLPlayer.Talents[TalentModManager.Instance.GetTalentIDFromName("Health Boost III")] * 20f;
            maxHealth += (float)pLPlayer.Talents[TalentModManager.Instance.GetTalentIDFromName("Health Boost IV")] * 20f;
            return maxHealth;
        }
    }
}
