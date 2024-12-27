using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using XLib.XLeveling;

namespace XSkillsExperienceHUD;

public class FloatingXpDisplay : HudElement
{
    private readonly List<FloatingXPElement> floatingXPElements = new();
    private readonly Queue<FloatingXPElement> pendingXPs = new();
    private readonly float spawnInterval = 0.3f;
    private float cooldownTimer;
    private long id;

    public FloatingXpDisplay(ICoreClientAPI capi) : base(capi)
    {
        SetupDialog();
        id = capi.World.RegisterGameTickListener(OnGameTick, 20);
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

        var floatingXpBounds = ElementBounds.Fixed(EnumDialogArea.CenterMiddle, 165, 0, 400, 300);
        SingleComposer.AddDynamicCustomDraw(floatingXpBounds, (ctx, surface, bounds) =>
        {
            foreach (var fxpElem in floatingXPElements)
            {
                fxpElem.ComposeElements(ctx, surface);
            }
        }, "floatingXP");

        SingleComposer.Bounds.Alignment = EnumDialogArea.CenterMiddle;
        SingleComposer.Compose();
    }

    public void UpdateDisplay(PlayerSkill playerSkill, float xp)
    {
        if (playerSkill.Skill.Id == 0)
        {
            return;
        }

        var skillName = playerSkill.Skill.Name;
        double startX = 0;
        double startY = 100;

        foreach (var queuedXP in pendingXPs)
        {
            if (queuedXP.SkillName == skillName)
            {
                queuedXP.AddXP(xp);
                SingleComposer?.GetCustomDraw("floatingXP")?.Redraw();
                return;
            }
        }

        pendingXPs.Enqueue(new FloatingXPElement(capi, ElementBounds.Fixed(0, 0, 0, 0), skillName, xp, startX, startY,
            3.0f));
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
        base.Dispose();
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