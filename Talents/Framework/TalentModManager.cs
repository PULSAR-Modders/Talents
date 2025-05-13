﻿using PulsarModLoader;
using System;
using System.Collections.Generic;
using System.Reflection;
using PulsarModLoader.Utilities;
using CodeStage.AntiCheat.ObscuredTypes;

namespace Talents.Framework
{
    public class TalentModManager
    {
        public enum CharacterClass
        {
            General = -1,
            Captain,
            Pilot,
            Scientist,
            Weapons,
            Engineer
        }
        public enum CharacterSpecies
        {
            General = -1,
            Human,
            Sylvassi,
            Robot
        }

        public readonly int vanillaTalentMaxType = 0;
        private static TalentModManager m_instance = null;
        public readonly List<TalentMod> TalentTypes = new List<TalentMod>();
        public static TalentModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new TalentModManager();
                }
                return m_instance;
            }
        }

        TalentModManager()
        {
            vanillaTalentMaxType = Enum.GetValues(typeof(ETalents)).Length;
            Logger.Info($"Talents Vanilla MaxTypeint = {vanillaTalentMaxType - 1}");
            extraTalentStatuses.Add(0, 0L);
            foreach (PulsarMod mod in ModManager.Instance.GetAllMods())
            {
                Assembly asm = mod.GetType().Assembly;
                Type talentMod = typeof(TalentMod);
                foreach (Type t in asm.GetTypes())
                {
                    if (talentMod.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    {
                        TalentMod talentModHandler = (TalentMod)Activator.CreateInstance(t);
                        if (GetTalentIDFromName(talentModHandler.Name) == -1)
                        {
                            TalentTypes.Add(talentModHandler);
                            int newTalentID = GetTalentIDFromName(talentModHandler.Name);
                            TalentCreation.cachedTalents[talentModHandler.TalentAssignment].Add((ETalents)newTalentID);
                            Logger.Info($"Added Talent: '{talentModHandler.Name}' with ID '{newTalentID}'");
                            if (newTalentID / 64 >= extraTalentStatuses.Keys.Count)
                            {   // Extend the ObscureLong TalentLockedStatus for > 64 talents
                                extraTalentStatuses.Add(extraTalentStatuses.Keys.Count, 0L);
                            }
                            if (talentModHandler.NeedsToBeResearched)
                            {
                                LockTalent(newTalentID);
                                Logger.Info($"Talent '{talentModHandler.Name}' is set to NEED TO BE RESEARCHED.");
                            }
                        }
                        else
                        {
                            Logger.Info($"Could not add Talent from {mod.Name} with the duplicate name of '{talentModHandler.Name}'");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Finds Talent ID equivilent to given name. Returns -1 if couldn't find Talent.
        /// </summary>
        /// <param name="TalentName">Name of Talent</param>
        /// <returns>ID of Talent</returns>
        public int GetTalentIDFromName(string TalentName)
        {
            for (int i = 0; i < TalentTypes.Count; i++)
            {
                if (TalentTypes[i].Name == TalentName)
                {
                    return i + vanillaTalentMaxType;
                }
            }
            return -1;
        }

        /// <summary>
        /// ObscuredLong PLServer.TalentLockedStatus is length limited to 64. This allows that to be extended.
        /// </summary>
        public Dictionary<int, ObscuredLong> extraTalentStatuses = new Dictionary<int, ObscuredLong>();

        /// <summary>
        /// Sets talentID bit location in the TalentLockedStatus ObscuredLong to be locked.
        /// </summary>
        /// <param name="talentID">ID of Talent</param>
        public void LockTalent(int talentID)
        {
            int index = talentID / 64;
            int bitPosition = talentID % 64;

            if (index != 0) // Skip index == 0 as that is the default 64 range
            {
                long mask = 1L << bitPosition;
                extraTalentStatuses[index] |= mask; // Set bit to 1 (locked)
            }
        }

        /// <summary>
        /// Sets talentID bit location in the TalentLockedStatus ObscuredLong to be Unlocked.
        /// </summary>
        /// <param name="talentID">ID of Talent</param>
        public void UnlockTalent(int talentID)
        {
            int index = talentID / 64;
            int bitPosition = talentID % 64;

            if (index != 0) // Skip index == 0 as that is the default 64 range
            {
                long mask = 1L << bitPosition;
                extraTalentStatuses[index] &= ~mask; // Set bit to 0 (unlocked)
            }
        }

    }
}
