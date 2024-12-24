using System;
using System.Collections.Generic;
using Cairo;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using XLib.XLeveling;

namespace XSkillsExperienceHUD;

public class FloatingXpDisplay : HudElement
{
    private readonly List<FloatingXP> floatingXPs = new();
    private readonly Queue<FloatingXP> pendingXPs = new();
    private readonly float spawnInterval = 1f; // 1 second interval
    private float cooldownTimer;

    private IAsset Heart;
    private long id;

    public FloatingXpDisplay(ICoreClientAPI capi) : base(capi)
    {
        SetupDialog();
        id = capi.World.RegisterGameTickListener(OnGameTick, 20);
    }

    public bool ResetupDialog(KeyCombination comb)
    {
        capi.World.UnregisterGameTickListener(id);
        SetupDialog();
        // Re-register after setup
        id = capi.World.RegisterGameTickListener(OnGameTick, 20);
        return true;
    }

    public void SetupDialog()
    {
        var dialogBounds = ElementBounds.Fill;

        var xLevelingClient = XLeveling.Instance(XSkillsExperienceHUDModSystem.capi).IXLevelingAPI as XLevelingClient;
        if (xLevelingClient == null)
        {
            return;
        }

        Heart = capi.Assets.Get("xskillsexperiencehud:textures/heart.svg");

        SingleComposer = capi.Gui.CreateCompo("floatingxpdisplay", dialogBounds);

        var floatingXpBounds = ElementBounds.Fixed(EnumDialogArea.RightTop, -50, 30, 400, 300);
        // var floatingXpBounds = ElementBounds.Fill;
        SingleComposer.AddDynamicCustomDraw(floatingXpBounds, DrawFloatingXp, "floatingXP");


        SingleComposer.Bounds.Alignment = EnumDialogArea.RightTop;
        SingleComposer.Compose();
    }

    private void DrawFloatingXp(Context ctx, ImageSurface surface, ElementBounds currentBounds)
    {
        ctx.SelectFontFace("Sans", FontSlant.Normal, FontWeight.Bold);
        ctx.SetFontSize(25); // adjust as needed

        foreach (var fxp in floatingXPs)
        {
            ctx.SetSourceRGBA(1, 1, 0, fxp.Alpha);
            ctx.MoveTo(fxp.X, fxp.Y);
            ctx.ShowText(fxp.Text);
        }
    }

    public void UpdateDisplay(PlayerSkill playerSkill, float xp)
    {
        if (playerSkill.Skill.Id == 0)
        {
            return;
        }

        var skillName = playerSkill.Skill.DisplayName;
        double startX = 50;
        double startY = 100;

        // Check pending XPs only for aggregation
        foreach (var queuedXP in pendingXPs)
        {
            if (queuedXP.SkillName == skillName)
            {
                queuedXP.AddXP(xp);
                SingleComposer?.GetCustomDraw("floatingXP")?.Redraw();
                return;
            }
        }

        // No matching pending entry, create a new one
        pendingXPs.Enqueue(new FloatingXP(skillName, xp, startX, startY, 2.0f));
        SingleComposer?.GetCustomDraw("floatingXP")?.Redraw();
    }

    private void OnGameTick(float dt)
    {
        // Update cooldown timer
        cooldownTimer += dt;


        // Update all visible XPs
        for (var i = floatingXPs.Count - 1; i >= 0; i--)
        {
            floatingXPs[i].Update(dt);
            if (floatingXPs[i].IsDead)
            {
                floatingXPs.RemoveAt(i);
            }
        }

        // If cooldown passed 1 second and we have pending XPs, spawn one
        if (cooldownTimer >= spawnInterval && pendingXPs.Count > 0)
        {
            floatingXPs.Add(pendingXPs.Dequeue());
            cooldownTimer = 0f; // reset the timer
        }

        SingleComposer?.GetCustomDraw("floatingXP")?.Redraw();
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