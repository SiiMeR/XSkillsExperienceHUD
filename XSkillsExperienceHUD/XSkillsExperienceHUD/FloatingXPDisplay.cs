using System;
using System.Collections.Generic;
using Cairo;
using Vintagestory.API.Client;
using XLib.XLeveling;

namespace XSkillsExperienceHUD;

public class FloatingXpDisplay : HudElement
{
    private readonly List<FloatingXP> floatingXPs = new();
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
        ctx.SetFontSize(20); // adjust as needed

        foreach (var fxp in floatingXPs)
        {
            // Yellowish text fading out
            ctx.SetSourceRGBA(1, 1, 0, fxp.Alpha);
            // Just draw at the coordinates stored in fxp
            ctx.MoveTo(fxp.X, fxp.Y);
            ctx.ShowText(fxp.Text);
        }
    }

    public void UpdateDisplay(PlayerSkill playerSkill, float xp)
    {
        double startX = 60;
        double startY = 100;

        var xpText = $"+{xp:0.###} {playerSkill.Skill.DisplayName} XP";
        floatingXPs.Add(new FloatingXP(xpText, startX, startY));

        SingleComposer?.GetCustomDraw("floatingXP")?.Redraw();
    }

    private void OnGameTick(float dt)
    {
        // Update all floating XP texts
        for (var i = floatingXPs.Count - 1; i >= 0; i--)
        {
            floatingXPs[i].Update(dt);
            if (floatingXPs[i].IsDead)
            {
                floatingXPs.RemoveAt(i);
            }
        }

        // Force redraw if needed
        // The custom draw can be handled in a dedicated custom draw handler
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