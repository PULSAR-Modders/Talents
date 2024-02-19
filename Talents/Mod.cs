using HarmonyLib;
using PulsarModLoader;
using Talents.Framework;

namespace Talents
{
    public class Mod : PulsarMod
    {
        public override string HarmonyIdentifier() => "Mest.Talents";
        public override string Author => "Mest";
        public override string Name => "Talents";
        public override string Version => "0.0.0";
    }
    [HarmonyPatch(typeof(PLGlobal), "Start")]
    class InjectTalents
    {
        static void Postfix()
        {
            PulsarModLoader.Utilities.Logger.Info("TALENTS WAS HERE");
            _ = TalentModManager.Instance;
            PulsarModLoader.Utilities.Logger.Info("TALENTS WAS STARTED HERE HERE");
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
