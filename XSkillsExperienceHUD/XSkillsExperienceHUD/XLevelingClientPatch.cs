using HarmonyLib;
using Vintagestory.API.Common;
using XLib.XLeveling;

namespace XSkillsExperienceHUD;

public class XLevelingClientPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(XLevelingClient))]
    [HarmonyPatch(typeof(XLevelingClient), nameof(XLevelingClient.AddExperienceToPlayerSkill))]
    public static bool Prefix(
        XLevelingClient __instance,
        IPlayer player,
        int skillId,
        float experience,
        bool informClient)
    {
        var genericSkill =
            skillId == 0 ? __instance.XLeveling.SkillSetTemplate[0] : __instance.XLeveling.GetSkill(skillId);

        if (genericSkill == null)
        {
            return true;
        }

        var behavior = player.Entity.GetBehavior<PlayerSkillSet>();
        if (behavior == null)
        {
            return true;
        }

        var playerSkill = __instance.LocalPlayerSkillSet.PlayerSkills[skillId];

        if (playerSkill == null)
        {
            return true;
        }

        XSkillsExperienceHUDModSystem.ExperienceDisplay.UpdateDisplay(playerSkill, experience);
        return true;
    }
}