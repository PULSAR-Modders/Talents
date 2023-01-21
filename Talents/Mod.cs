using HarmonyLib;
using PulsarModLoader;
using PulsarModLoader.Chat.Commands.CommandRouter;
using PulsarModLoader.Content.Components.Reactor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static PulsarModLoader.Patches.HarmonyHelpers;

namespace Talents
{
    public class Mod : PulsarMod
    {
        public override string HarmonyIdentifier() => "Mest.Talents";
        public override string Author => "Mest";
        public override string Name => "TalentsPlus";
        public override string Version => "0.0.0";
        public static bool ModEnabled = false;
    }

    [Serializable]
    public class TalentInfoAdditionalData
    {
        public ETalents ConflictTalent; // This is called with: TalentInfo __instance.GetAdditionalData().PlayerID
        public TalentInfoAdditionalData()
        {
            ConflictTalent = ETalents.MAX;
        }
    }
    [HarmonyPatch(typeof(PLServer), "Start")]
    class StartPatch
    {
        private static void Postfix()
        {
            Mod.ModEnabled = PhotonNetwork.isMasterClient || PulsarModLoader.MPModChecks.MPModCheckManager.Instance.NetworkedPeerHasMod(PhotonNetwork.masterClient, "Mest.Talents");
        }
    }
    [HarmonyPatch(typeof(PLUIClassSelectionMenu), "ClickReady")]
    class BackupStartPatch
    {
        public static void Postfix()
        {
            if (!Mod.ModEnabled)
            {
                Mod.ModEnabled = PhotonNetwork.isMasterClient || PulsarModLoader.MPModChecks.MPModCheckManager.Instance.NetworkedPeerHasMod(PhotonNetwork.masterClient, "Mest.Talents");
            }
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

    public class Command : ChatCommand
    {
        public override string[] CommandAliases() => new string[] { "gettalents" };

        public override string Description() => "Display All Talents";

        public override void Execute(string arguments)
        {
            for (int i = 0; i < ETalentsPlus.MAX + 1; i++)
            {
                TalentInfo talent = PLGlobal.GetTalentInfoForTalentType((ETalents)i);
                if (talent != null)
                {
                    PulsarModLoader.Utilities.Logger.Info($"[TALENTS] {i} - {talent.Name} - ID: {talent.TalentID}");
                }
            }
        }
    }
    public class Command2 : ChatCommand
    {
        public override string[] CommandAliases() => new string[] { "getlocaltalents" };

        public override string Description() => "Display LOCAL Talents";

        public override void Execute(string arguments)
        {
            PLPlayer Player = PLNetworkManager.Instance.LocalPlayer;
            int Count = 0;
            foreach (int i in Player.Talents)
            {
                TalentInfo talent = PLGlobal.GetTalentInfoForTalentType((ETalents)Count);
                if (talent != null)
                {
                    PulsarModLoader.Utilities.Logger.Info($"[TALENTS] {talent.Name} - ID: {talent.TalentID} - LEVEL: {i}");
                }
                Count++;
            }
        }
    }
}
