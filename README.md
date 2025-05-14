# Talents

Function: Extends and improves the current Talent system.

Create and override the following abstract class to add your own Talent.
```c#
public abstract class TalentMod
{
    public TalentMod() { }
    /// <summary>
    /// Name of the Talent for ID-ing and appears in Research and Tab display.
    /// </summary>
    public abstract string Name { get; }
    /// <summary>
    /// Description which appears in Research and Tab display.
    /// </summary>
    public virtual string Description { get { return ""; } }
    /// <summary>
    /// Maximum level Talent can be set to (Havent tested beyond 5).
    /// </summary>
    public virtual int MaxRank { get { return 3; } }
    /// <summary>
    /// ClassID of Talent. Basically just puts Talent at the top of the list with the class colour.
    /// </summary>
    public virtual int ClassID { get { return -1; } }
    /// <summary>
    /// Array of how many and which research materials are needed.
    /// </summary>
    public virtual int[] ResearchCost { get { return new int[6]; } }
    /// <summary>
    /// Number of warps to research the talent.
    /// </summary>
    public virtual int WarpsToResearch { get { return 3; } }
    /// <summary>
    /// Boolean that locks the Talent and makes it need researching.
    /// </summary>
    public virtual bool NeedsToBeResearched { get { return false; } }
    /// <summary>
    /// Name of talent that needs to be unlocked before this one is available.
    /// </summary>
    public virtual string ExtendsModdedTalent { get { return ""; } }
    /// <summary>
    /// Name of talent that needs to be unlocked before this one is available.
    /// </summary>
    public virtual ETalents ExtendsDefaultTalent { get { return ETalents.MAX; } }
    /// <summary>
    /// Minimum level required to unlock talent.
    /// </summary>
    public virtual int MinLevel { get { return 0; } }
    /// <summary>
    /// Talents can be hidden from the lists so that they can be hidden/not manually.
    /// Use TalentModManager.HideTalent(int talentID) or TalentModManager.UnHideTalent(int talentID)
    /// </summary>
    public virtual bool HiddenByDefault { get { return false; } }
    /// <summary>
    /// ClassID and Species talent applies to. Both enumerators found in TalentModManager.
    /// All, Captain, Pilot, Scientist, Weapons, Engineer | All, Human, Robot, Sylvassi
    /// </summary>
    public virtual (CharacterClass, CharacterSpecies) TalentAssignment { get { return ((CharacterClass)ClassID, CharacterSpecies.General); } }
}
```

Implementation is done manually, talents can be found with the following:

```c#
int inTalentId = TalentModManager.Instance.GetTalentIDFromName("Health Boost III");
ETalents inTalentId = (ETalents)TalentModManager.Instance.GetTalentIDFromName("Health Boost III");
```

the following are examples:

<details>
<summary>Generic Talent</summary>

Below is an example of how to implement more health boost for Humans.

```c#
internal class Health_Boost_3 : TalentMod
{
    public override string Name => "Health Boost III";
    public override string Description => "+20 to max health per rank";
    public override int MaxRank => 5;
    public override ETalents ExtendsDefaultTalent => ETalents.HEALTH_BOOST_2;
    public override int MinLevel => 11;
    public override (TalentModManager.CharacterClass, TalentModManager.CharacterSpecies) TalentAssignment => (TalentModManager.CharacterClass.General, TalentModManager.CharacterSpecies.Human);
}
internal class Health_Boost_4 : TalentMod
{
    public override string Name => "Health Boost IV";
    public override string Description => "+20 to max health per rank";
    public override int MaxRank => 5;
    public override string ExtendsModdedTalent => "Health Boost III";
    public override int MinLevel => 16;
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
    }
}
```
</details>
<details>
  <summary>Specific Class/Species</summary>

To customise what Class and/or Species the talent is visible for, change the following:
```c#
/// <summary>
/// ClassID and Species talent applies to. Both enumerators found in TalentModManager.
/// All, Captain, Pilot, Scientist, Weapons, Engineer | All, Human, Robot, Sylvassi
/// </summary>
public virtual (CharacterClass, CharacterSpecies) TalentAssignment
{
  get
  {
    return ((CharacterClass)ClassID, CharacterSpecies.General);
  }
}
```

#### CharacterClass
Dictates what class the talent is given to.

```c#
public enum CharacterClass
{
    General = -1,
    Captain,
    Pilot,
    Scientist,
    Weapons,
    Engineer
}
```

#### CharacterSpecies
Dictates what Species the talent is given to.

```c#
public enum CharacterSpecies
{
    General = -1,
    Human,
    Sylvassi,
    Robot
}
```

</details>
<details>
  <summary>Making it Researchable</summary>

Talents can be researched, and to make yours researchable add the following:
```c#
/// <summary>
/// Array of how many and which research materials are needed.
/// </summary>
public override int[] ResearchCost => new int[6] { 1, 2, 3, 4, 5, 6 };

/// <summary>
/// Boolean that locks the Talent and makes it need researching.
/// </summary>
public override bool NeedsToBeResearched => true;
```

And it will make the talent researchable.

![image](https://github.com/user-attachments/assets/4f4cfc39-562a-4790-b742-7df57a9f473f)

</details>
<details>
  <summary>Hidden from List</summary>

Talents can be hidden from the talent and research list to be manually assigned.
For purchaseable talents, mission provided, etc.

To make it so, add the following:
```c#
/// <summary>
/// Talents can be hidden from the lists so that they can be hidden/not manually.
/// Use TalentModManager.HideTalent(int talentID) or TalentModManager.UnHideTalent(int talentID)
/// </summary>
public override bool HiddenByDefault => true;
```

It can then be manually hidden/not with:
```c#
TalentModManager.Instance.HideTalent(int talentID)
TalentModManager.Instance.UnHideTalent(int talentID)
```
</details>
