using HarmonyLib;
using PulsarModLoader;
using PulsarModLoader.CustomGUI;
using System;
using Talents.Framework;
using UnityEngine;

namespace Talents
{
    public class Mod : PulsarMod
    {
        public override string HarmonyIdentifier() => "Mest.Talents";
        public override string Author => "Mest";
        public override string Name => "Talents";
        public override string Version => "0.0.1";
    }

    [HarmonyPatch(typeof(PLGlobal), "Start")]
    class InjectTalents
    {
        static void Postfix()
        {
            _ = TalentModManager.Instance;
        }
    }

    internal class Config : ModSettingsMenu
    {
        public override string Name() => "Talents Config";
        public override void Draw()
        {
            HideResearchableTalentsFromTab.Value = GUILayout.Toggle(HideResearchableTalentsFromTab.Value, "Hide Non-Researched Talents from Tab Menu");
        }
        public static SaveValue<bool> HideResearchableTalentsFromTab = new SaveValue<bool>("HideResearchableTalentsFromTab", false);
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
