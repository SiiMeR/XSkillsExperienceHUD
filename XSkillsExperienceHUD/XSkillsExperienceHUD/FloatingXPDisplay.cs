using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using XLib.XLeveling;

namespace XSkillsExperienceHUD;

public class FloatingXpDisplay : HudElement
{
    public static FloatingXpDisplay Instance;
    private readonly List<FloatingXPElement> floatingXPElements = new();
    private readonly Queue<FloatingXPElement> pendingXPs = new();
    private readonly float spawnInterval = 0.3f;
    private float cooldownTimer;
    private long id;

    public FloatingXpDisplay(ICoreClientAPI capi) : base(capi)
    {
        Instance = this;
        SetupDialog();
        id = capi.World.RegisterGameTickListener(OnGameTick, 30);
    }

    public void SetupDialog()
    {
        var dialogBounds = ElementBounds.Fill;
        var xLevelingClient = XLeveling.Instance(XSkillsExperienceHUDModSystem.capi).IXLevelingAPI as XLevelingClient;
        if (xLevelingClient == null)
        {
            return;
        }

        SingleComposer = capi.Gui.CreateCompo("floatingxpdisplay", dialogBounds);

        var modConfig = XSkillsExperienceHUDModSystem.Config;
        var floatingXpBounds =
            ElementBounds.Fixed(modConfig.FloatingXPLocation, modConfig.FloatingXPX, modConfig.FloatingXPY, 400, 400);
        SingleComposer.AddDynamicCustomDraw(floatingXpBounds, (ctx, surface, bounds) =>
        {
            foreach (var fxpElem in floatingXPElements)
            {
                fxpElem.ComposeElements(ctx, surface);
            }
        }, "floatingXP");

        SingleComposer.Compose();
    }

    public void UpdateDisplay(PlayerSkill playerSkill, float xp)
    {
        if (XSkillsExperienceHUDModSystem.Config.SkillConfiguration[Util.SkillNameToIconName(playerSkill.Skill.Name)] ==
            false)
        {
            return;
        }

        var skillName = playerSkill.Skill.Name;
        double startX = 0;
        double startY = 150;

        foreach (var queuedXP in pendingXPs)
        {
            if (queuedXP.SkillName == skillName)
            {
                queuedXP.AddXP(xp);
                SingleComposer?.GetCustomDraw("floatingXP")?.Redraw();
                return;
            }
        }

        pendingXPs.Enqueue(new FloatingXPElement(capi, ElementBounds.Fixed(0, 0, 50, 20), skillName, xp, startX, startY,
            5.0f));
        SingleComposer?.GetCustomDraw("floatingXP")?.Redraw();
    }

    private void OnGameTick(float dt)
    {
        cooldownTimer += dt;
        for (var i = floatingXPElements.Count - 1; i >= 0; i--)
        {
            floatingXPElements[i].Update(dt);
            if (floatingXPElements[i].IsDead)
            {
                floatingXPElements.RemoveAt(i);
            }
        }

        if (cooldownTimer >= spawnInterval && pendingXPs.Count > 0)
        {
            floatingXPElements.Add(pendingXPs.Dequeue());
            cooldownTimer = 0f;
        }

        SingleComposer?.GetCustomDraw("floatingXP")?.Redraw();
    }

    public bool ResetupDialog(KeyCombination comb)
    {
        capi.World.UnregisterGameTickListener(id);
        SetupDialog();
        id = capi.World.RegisterGameTickListener(OnGameTick, 20);
        return true;
    }

    public override void Dispose()
    {
        capi.World.UnregisterGameTickListener(id);
    }

    public bool ToggleGui(KeyCombination comb)
    {
        Console.WriteLine("Toggling Floating XP HUD");
        if (IsOpened())
        {
            TryClose();
        }
        else
        {
            TryOpen();
        }

        return true;
    }

    public override bool ShouldReceiveMouseEvents()
    {
        return false;
    }
}