using HarmonyLib;
using XLib.XLeveling;

namespace XSkillsExperienceHUD;

public class XLevelingClientPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(XLevelingClient), "MessageHandler")]
    public static void Patch(
        XLevelingClient __instance,
        ExperiencePackage package)
    {
        var skillId = package.skillId;

        var genericSkill =
            skillId == 0 ? __instance.XLeveling.SkillSetTemplate[0] : __instance.XLeveling.GetSkill(skillId);

        if (genericSkill == null)
        {
            return;
        }

        var playerSkill = __instance.LocalPlayerSkillSet.PlayerSkills[skillId];
        if (playerSkill == null)
        {
            return;
        }

        XSkillsExperienceHUDModSystem.ExperienceDisplay.UpdateDisplay(playerSkill, package.experience);
    }
}