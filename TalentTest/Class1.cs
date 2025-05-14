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
    internal class Researchable_Talent : TalentMod
    {
        public override string Name => "Research Me";
        public override string Description => "I need to be researched";
        public override int[] ResearchCost => new int[6] { 1, 0, 0, 0, 0, 0 };
        public override bool NeedsToBeResearched => true;
        public override (TalentModManager.CharacterClass, TalentModManager.CharacterSpecies) TalentAssignment => (TalentModManager.CharacterClass.General, TalentModManager.CharacterSpecies.General);
    }
    internal class Revealer : TalentMod
    {
        public override string Name => "Revealer";
        public override string Description => "I reveal a hidden talent!";
        public override int MaxRank => 1;
        public override (TalentModManager.CharacterClass, TalentModManager.CharacterSpecies) TalentAssignment => (TalentModManager.CharacterClass.General, TalentModManager.CharacterSpecies.General);
    }
    internal class Hidden : TalentMod
    {
        public override string Name => "Hidden";
        public override string Description => "I was hiding!";
        public override int MaxRank => 0;
        public override bool HiddenByDefault => true;
        public override (TalentModManager.CharacterClass, TalentModManager.CharacterSpecies) TalentAssignment => (TalentModManager.CharacterClass.General, TalentModManager.CharacterSpecies.General);
    }
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
            int boost3 = TalentModManager.Instance.GetTalentIDFromName("Health Boost III");
            int boost4 = TalentModManager.Instance.GetTalentIDFromName("Health Boost IV");
            if (pLPlayer.Talents.Count() > boost3) maxHealth += (float)pLPlayer.Talents[TalentModManager.Instance.GetTalentIDFromName("Health Boost III")] * 20f;
            if (pLPlayer.Talents.Count() > boost4) maxHealth += (float)pLPlayer.Talents[TalentModManager.Instance.GetTalentIDFromName("Health Boost IV")] * 20f;
            int boost5 = TalentModManager.Instance.GetTalentIDFromName("Revealer");
            if (pLPlayer.Talents.Count() > boost5 && pLPlayer.Talents[boost5] > 0) TalentModManager.Instance.UnHideTalent(TalentModManager.Instance.GetTalentIDFromName("Hidden"));
            return maxHealth;
        }
    }
}
