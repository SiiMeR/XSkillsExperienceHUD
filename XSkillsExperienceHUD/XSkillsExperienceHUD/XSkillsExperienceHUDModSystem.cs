using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using XLib.XLeveling;

namespace XSkillsExperienceHUD;

public class XSkillsExperienceHUDModSystem : ModSystem
{
    public static ICoreClientAPI capi;
    public static ExperienceDisplay ExperienceDisplay;

    public override void StartClientSide(ICoreClientAPI api)
    {
        capi = api;

        ExperienceDisplay = new ExperienceDisplay(api);
        ApplyHarmonyPatch(api);
    }

    private void ApplyHarmonyPatch(ICoreClientAPI api)
    {
        var harmony = new Harmony(Mod.Info.ModID);

        var original = AccessTools.Method(typeof(XLevelingClient), nameof(XLevelingClient.AddExperienceToPlayerSkill));
        var patch = AccessTools.Method(typeof(XLevelingClientPatch), nameof(XLevelingClientPatch.Prefix));

        harmony.Patch(original, new HarmonyMethod(patch));

        base.StartClientSide(api);
    }
}