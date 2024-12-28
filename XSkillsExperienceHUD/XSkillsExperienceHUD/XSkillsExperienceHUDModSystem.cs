﻿using HarmonyLib;
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

        Icons.Initialize();
        ExperienceDisplay = new ExperienceDisplay(api);
        ApplyHarmonyPatch(api);
    }

    private void ApplyHarmonyPatch(ICoreClientAPI api)
    {
        var harmony = new Harmony(Mod.Info.ModID);

        var original =
            AccessTools.Method(typeof(XLevelingClient), "MessageHandler", new[] { typeof(ExperiencePackage) });
        var patch = AccessTools.Method(typeof(XLevelingClientPatch), nameof(XLevelingClientPatch.Patch));

        harmony.Patch(original, postfix: new HarmonyMethod(patch));

        base.StartClientSide(api);
    }
}