using CodeStage.AntiCheat.ObscuredTypes;
using ExitGames.Demos.DemoAnimator;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static PulsarModLoader.Patches.HarmonyHelpers;

namespace Talents
{
    /*
     * Serialize Talents (Might have already patched in TalentModManager)
     * 
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
            //PulsarModLoader.Utilities.Logger.Info($"[TALENTS] - SendTalentsToPhotonTargets: inPlayer = null {inPlayer == null} | {inClassID}");
            List<ETalents> TalentList = TalentCreation.TalentsForClassSpecies(PLServer.GetPlayerForPhotonPlayer(inPlayer), inClassID);
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
    */
    /*
     * Conflicting Talents Patch
     * 
    [HarmonyPatch(typeof(PLTabMenu), "UpdateTDs")]
    class LockConflictingTalents
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> target = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(PLServer), "Instance")),
                new CodeInstruction(OpCodes.Ldloc_S),       // etalents
                new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(PLServer), "IsTalentUnlocked", new Type[] { typeof(ETalents) }))
            };
            int NextInstruction = FindSequence(instructions, target, CheckMode.NONNULL);
            List<CodeInstruction> patch = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldarg_0),       // PLTabMenu Instance
                instructions.ToList()[NextInstruction - 2], // etalents
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(LockConflictingTalents), "Replacement", new Type[] { typeof(PLTabMenu), typeof(ETalents) }))
            };
            return PatchBySequence(instructions, target, patch, PatchMode.REPLACE, CheckMode.NONNULL);
        }
        public static bool Replacement(PLTabMenu instance, ETalents etalents)
        {
            PLPlayer player = PLServer.Instance.GetPlayerFromPlayerID(instance.TalentsListSelectedPlayerID);
            return PLServer.Instance.IsTalentUnlocked(etalents) && !HasConflictingTalents(player, etalents);
        }
        private static bool HasConflictingTalents(PLPlayer player, ETalents etalents)
        {
            if (PLGlobal.GetTalentInfoForTalentType(etalents).GetAdditionalData().ConflictTalents == null) return false;
            foreach (ETalents Talent in PLGlobal.GetTalentInfoForTalentType(etalents).GetAdditionalData().ConflictTalents)
            {
                if (player.Talents[(int)Talent] != 0) return true;
            }
            return false;
        }
    }
    */
}
