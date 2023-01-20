using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static PulsarModLoader.Patches.HarmonyHelpers;

namespace Talents
{
    [HarmonyPatch(typeof(PLPlayer), "Start")]
    class StartTalentSizePatch
    {
        public static void Postfix(PLPlayer __instance)
        {
            __instance.Talents = new ObscuredInt[ETalentsPlus.MAX + 1];
            __instance.TalentsLocalEditTime = new float[ETalentsPlus.MAX + 1];
        }
    }
    [HarmonyPatch(typeof(PLPlayer), "SendLateInfoTo")]
    class SendLateInfoTo
    {
        public static bool Prefix(PLPlayer __instance, PhotonPlayer inPlayer)
        {
            if (__instance.MyInventory != null)
            {
                __instance.MyInventory.OnLateJoin(inPlayer);
            }
            __instance.StartCoroutine(SafeSendLateAIInfoTo(__instance, inPlayer));
            return false;
        }
        private static IEnumerator SafeSendLateAIInfoTo(PLPlayer __instance, PhotonPlayer inPlayer)
        {
            int loops = 0;
            while (__instance.GetAIData() == null && loops < 50)
            {
                yield return new WaitForSeconds(0.5f);
                int num = loops;
                loops = num + 1;
            }
            PLPlayer playerForPhotonPlayer = PLServer.GetPlayerForPhotonPlayer(inPlayer);
            if (playerForPhotonPlayer != null && playerForPhotonPlayer.GetClassID() == 0)
            {
                __instance.StartCoroutine(SendTalentsToPhotonTargets(__instance, __instance.GetClassID(), PhotonTargets.MasterClient, true, inPlayer));
            }
            else
            {
                __instance.StartCoroutine(SendPrioritiesToPhotonTargets(__instance, __instance.GetClassID(), PhotonTargets.MasterClient, true, inPlayer));
                __instance.StartCoroutine(SendTalentsToPhotonTargets(__instance, __instance.GetClassID(), PhotonTargets.MasterClient, true, inPlayer));
            }
            yield break;
        }
        protected static FieldInfo TalentsSendTimer_MinTimeInfo = AccessTools.Field(typeof(PLPlayer), "TalentsSendTimer_MinTime");
        protected static FieldInfo EndSyncTalentsTimeInfo = AccessTools.Field(typeof(PLPlayer), "EndSyncTalentsTime");
        protected static FieldInfo CurrentlySendingTalentsToOthersInfo = AccessTools.Field(typeof(PLPlayer), "CurrentlySendingTalentsToOthers");
        private static IEnumerator SendTalentsToPhotonTargets(PLPlayer __instance, int inClassID, PhotonTargets inTargets, bool sendAll = false, PhotonPlayer inPlayer = null)
        {
            int talentID = 0;
            PulsarModLoader.Utilities.Logger.Info($"[TALENTS] - SendTalentsToPhotonTargets - inPlayer = null {inPlayer == null}");
            List<ETalents> TalentList = Mod.TalentsForClass(inClassID);
            if (inPlayer != null)
            {
                PulsarModLoader.Utilities.Logger.Info($"[TALENTS] - SendTalentsToPhotonTargets - PLPlayer = null {PLServer.GetPlayerForPhotonPlayer(inPlayer) == null}");
                PulsarModLoader.Utilities.Logger.Info($"[TALENTS] - SendTalentsToPhotonTargets - ClassID = {inClassID}");
                TalentList.AddRange(Mod.TalentsForSpecies(PLServer.GetPlayerForPhotonPlayer(inPlayer)));
            }
            while (talentID < ETalentsPlus.MAX && talentID < __instance.Talents.Length)
            {
                if (talentID == (int)ETalents.MAX)
                {
                    talentID++;
                    continue;
                }
                if (TalentList.Contains((ETalents)talentID) && (__instance.TalentsLocalEditTime[talentID] + (float)TalentsSendTimer_MinTimeInfo.GetValue(__instance) >= (float)EndSyncTalentsTimeInfo.GetValue(__instance) || UnityEngine.Random.Range(0, 150) == 5 || sendAll))
                {
                    if (inPlayer != null)
                    {
                        __instance.photonView.RPC("GetUpdatedTalent", inPlayer, new object[]
                        {
                        talentID,
                        __instance.Talents[talentID].GetDecrypted()
                        });
                    }
                    else
                    {
                        __instance.photonView.RPC("GetUpdatedTalent", inTargets, new object[]
                        {
                        talentID,
                        __instance.Talents[talentID].GetDecrypted()
                        });
                    }
                    float num = 0.5f;
                    if (__instance.IsBot)
                    {
                        num = 1f;
                    }
                    yield return new WaitForSeconds(num + PLNetworkManager.Instance.Instability);
                }
                talentID++;
            }
            CurrentlySendingTalentsToOthersInfo.SetValue(__instance, false);
            EndSyncTalentsTimeInfo.SetValue(__instance, Time.time);
            yield break;
        }
        protected static FieldInfo CurrentlySendingPrioritiesToOthersInfo = AccessTools.Field(typeof(PLPlayer), "CurrentlySendingPrioritiesToOthers");
        private static IEnumerator SendPrioritiesToPhotonTargets(PLPlayer __instance, int inClassID, PhotonTargets inTargets, bool sendAll = false, PhotonPlayer inPlayer = null)
        {
            if (__instance.GetAIData() != null)
            {
                List<AIPriority> list = new List<AIPriority>(__instance.GetAIData().Priorities);
                foreach (AIPriority priority in list)
                {
                    if (priority.NetDirty || sendAll || UnityEngine.Random.Range(0, 150) == 5)
                    {
                        priority.NetDirty = false;
                        if (inPlayer != null)
                        {
                            __instance.photonView.RPC("GetUpdatedPriority", inPlayer, new object[]
                            {
                            priority.PriID,
                            priority.BasePriority,
                            (int)priority.Type,
                            priority.TypeData
                            });
                        }
                        else
                        {
                            __instance.photonView.RPC("GetUpdatedPriority", inTargets, new object[]
                            {
                            priority.PriID,
                            priority.BasePriority,
                            (int)priority.Type,
                            priority.TypeData
                            });
                        }
                        yield return new WaitForSeconds(0.05f + PLNetworkManager.Instance.Instability * 0.3f);
                    }
                    List<AIPriority> list2 = new List<AIPriority>(priority.Subpriorities);
                    foreach (AIPriority aipriority in list2)
                    {
                        if (aipriority.NetDirty || sendAll || UnityEngine.Random.Range(0, 150) == 5)
                        {
                            aipriority.NetDirty = false;
                            if (inPlayer != null)
                            {
                                __instance.photonView.RPC("GetUpdatedSubPriority", inPlayer, new object[]
                                {
                                priority.PriID,
                                aipriority.PriID,
                                aipriority.BasePriority,
                                (int)aipriority.Type,
                                aipriority.TypeData
                                });
                            }
                            else
                            {
                                __instance.photonView.RPC("GetUpdatedSubPriority", inTargets, new object[]
                                {
                                priority.PriID,
                                aipriority.PriID,
                                aipriority.BasePriority,
                                (int)aipriority.Type,
                                aipriority.TypeData
                                });
                            }
                            yield return new WaitForSeconds(0.05f + PLNetworkManager.Instance.Instability * 0.3f);
                        }
                    }
                }
            }
            CurrentlySendingPrioritiesToOthersInfo.SetValue(__instance, false);
            EndSyncTalentsTimeInfo.SetValue(__instance, Time.time);
            yield break;
        }
    }

    [HarmonyPatch(typeof(PLTabMenu), "UpdateTDs")]
    class UpdateTDs
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
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(UpdateTDs), "Replacement", new Type[] { typeof(PLTabMenu), typeof(PLPlayer) }))
            };
            return PatchBySequence(instructions, target, patch, PatchMode.REPLACE, CheckMode.NONNULL);
        }
        public static List<ETalents> Replacement(PLTabMenu instance, PLPlayer pLPlayer)
        {
            List<ETalents> Talents = Mod.TalentsForClass(pLPlayer.GetClassID());
            Talents.AddRange(Mod.TalentsForSpecies(pLPlayer));
            return Talents;
        }
    }

    [HarmonyPatch(typeof(PLGlobal), "GetTalentInfoForTalentType")]
    class GetTalentInfoForTalentType
    {
        protected static FieldInfo CachedTalentInfosInfo = AccessTools.Field(typeof(PLGlobal), "CachedTalentInfos");
        public static void Prefix(PLGlobal __instance, ETalents inTalent)
        {
            Dictionary<int, TalentInfo> CachedTalentInfos = (Dictionary<int, TalentInfo>)CachedTalentInfosInfo.GetValue(__instance);
            if (CachedTalentInfos.ContainsKey((int)inTalent))
            {
                return;
            }
            int VanillaTalentMaxType = Enum.GetValues(typeof(ETalents)).Length;
            int eTalentsPlus = (int)inTalent - VanillaTalentMaxType;
            if (eTalentsPlus < 0) return;
            TalentInfo talentInfo = new TalentInfo();
            talentInfo.ClassID = -1;
            talentInfo.TalentID = (int)inTalent;
            talentInfo.MaxRank = 3;
            talentInfo.ResearchCost = new int[6];
            talentInfo.WarpsToResearch = 3;
            talentInfo.ExtendsTalent = ETalents.MAX;
            talentInfo.MinLevel = 0;
            switch ((int)inTalent)
            {
                default:
                    return;
                case ETalentsPlus.HEALTH_BOOST_3:
                    talentInfo.Name = "Health Boost III";
                    talentInfo.Desc = "+20 to max health per rank";
                    talentInfo.MaxRank = 5;
                    talentInfo.ExtendsTalent = ETalents.HEALTH_BOOST_2;
                    talentInfo.MinLevel = 11;
                    break;
                case ETalentsPlus.HEALTH_BOOST_4:
                    talentInfo.Name = "Health Boost IV";
                    talentInfo.Desc = "+20 to max health per rank";
                    talentInfo.MaxRank = 5;
                    talentInfo.ExtendsTalent = (ETalents) ETalentsPlus.HEALTH_BOOST_3;
                    talentInfo.MinLevel = 16;
                    break;
                case ETalentsPlus.HEALTH_BOOST_5:
                    talentInfo.Name = "Health Boost V";
                    talentInfo.Desc = "+20 to max health per rank";
                    talentInfo.MaxRank = 5;
                    talentInfo.ExtendsTalent = (ETalents) ETalentsPlus.HEALTH_BOOST_4;
                    talentInfo.MinLevel = 21;
                    break;
            }
            CachedTalentInfos.Add((int)inTalent, talentInfo);
        }
    }
    public class ETalentsPlus
    {
        public const int HEALTH_BOOST_3 = (int)ETalents.MAX + 1;    // 64
        public const int HEALTH_BOOST_4 = (int)ETalents.MAX + 2;    // 64
        public const int HEALTH_BOOST_5 = (int)ETalents.MAX + 3;    // 64
        public const int MAX = (int)ETalents.MAX + 4;
    }
}
