using System;
using System.Collections.Generic;
using Cairo;
using Vintagestory.API.Client;
using XLib.XLeveling;

namespace XSkillsExperienceHUD;

public class ExperienceDisplay : HudElement
{
    private long id;
    public Dictionary<string, string> xpDisplay = new();

    public ExperienceDisplay(ICoreClientAPI capi) : base(capi)
    {
        capi.Input.RegisterHotKey(
            "toggleexperiencehud",
            "Toggle Experience HUD",
            GlKeys.K,
            HotkeyType.GUIOrOtherControls
        );
        capi.Input.SetHotKeyHandler("toggleexperiencehud", ToggleGui);


        capi.Input.RegisterHotKey(
            "redrawexperiencehud",
            "Redraw Experience HUD",
            GlKeys.N,
            HotkeyType.GUIOrOtherControls
        );
        capi.Input.SetHotKeyHandler("redrawexperiencehud", ResetupDialog);
        SetupDialog();
    }

    public void UpdateDisplay(PlayerSkill playerSkill, float xp)
    {
        SingleComposer?.GetCustomDraw(playerSkill.Skill.Name)?.Redraw();

        // var xpString =
        //     $"Level {playerSkill.Level} - {playerSkill.Experience + xp:F2}/{playerSkill.RequiredExperience:F2}";
        // if (xpDisplay.ContainsKey(skillName))
        // {
        //     xpDisplay[skillName] = xpString;
        // }
        // else
        // {
        //     xpDisplay.TryAdd(skillName, xpString);
        // }

        // UpdateText();
    }

    public bool ResetupDialog(KeyCombination comb)
    {
        capi.World.UnregisterGameTickListener(id);
        SetupDialog();
        return true;
    }

    private void SetupDialog()
    {
        var dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.RightMiddle);

        // Background boundaries. Again, just make it fit it's child elements, then add the text as a child element
        var bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
        bgBounds.BothSizing = ElementSizing.FitToChildren;


        var xLevelingClient = XLeveling.Instance(XSkillsExperienceHUDModSystem.capi).IXLevelingAPI as XLevelingClient;
        if (xLevelingClient == null)
        {
            return;
        }

        var playerSkillset = xLevelingClient
            .LocalPlayerSkillSet;

        SingleComposer = capi.Gui.CreateCompo("experiencedialog", dialogBounds);
        // .AddShadedDialogBG(bgBounds);

        var y = 40;
        var barBoundList = new List<ElementBounds>();
        for (var index = 0; index < playerSkillset.PlayerSkills.Count; index++)
        {
            var playerSkill = playerSkillset.PlayerSkills[index];
            var barBounds = ElementBounds.Fixed(20, y + 25 * index, 200, 30)
                .WithFixedPadding(GuiStyle.HalfPadding);
            // barBoundList.Add(barBounds);


            SingleComposer.AddDynamicCustomDraw(barBounds, (ctx, surface, currentBounds) =>
            {
                var fraction = playerSkill.Experience / playerSkill.RequiredExperience;
                var w = currentBounds.InnerWidth;
                var h = currentBounds.InnerHeight;

                // Draw the progress bar first
                double barX = 0;
                double barY = 20; // Bar offset down to leave space for text above if needed
                var barWidth = w;
                var barHeight = h - 20; // Height of the bar area
                DrawProgressBar(ctx, barWidth, barHeight, fraction, barX, barY);

                // Prepare font and text
                ctx.SetSourceRGBA(1, 1, 1, 1);
                ctx.SelectFontFace("Sans", FontSlant.Normal, FontWeight.Bold);
                ctx.SetFontSize(15);

                var skillName = playerSkill.Skill.DisplayName;
                var currentLevelText = $"{playerSkill.Level}";
                var nextLevelText = $"{playerSkill.Level + 1}";

                var extSkill = ctx.TextExtents(skillName);
                var extCurrentLevel = ctx.TextExtents(currentLevelText);
                var extNextLevel = ctx.TextExtents(nextLevelText);

                // Vertical centering in the bar:
                // We'll center text vertically by using the bar's vertical center line.
                // baselineY = center of bar - half text height - YBearing
                var centerY = barY + barHeight / 2;

                // Draw skill name centered horizontally:
                var skillNameX = barX + (barWidth - extSkill.Width) / 2;
                var skillNameY = centerY - extSkill.Height / 2 - extSkill.YBearing;
                ctx.MoveTo(skillNameX, skillNameY);
                ctx.ShowText(skillName);

                // Draw current level on the left inside the bar
                var currentLevelX = barX + 3;
                var currentLevelY = centerY - extCurrentLevel.Height / 2 - extCurrentLevel.YBearing;
                ctx.MoveTo(currentLevelX, currentLevelY);
                ctx.ShowText(currentLevelText);

                // Draw next level on the right inside the bar
                var nextLevelX = barX + barWidth - extNextLevel.Width - 3;
                var nextLevelY = centerY - extNextLevel.Height / 2 - extNextLevel.YBearing;
                ctx.MoveTo(nextLevelX, nextLevelY);
                ctx.ShowText(nextLevelText);
            }, playerSkill.Skill.Name);
        }

        // bgBounds.WithChildren(barBoundList.ToArray());

        SingleComposer.Bounds.Alignment = EnumDialogArea.RightBottom;
        SingleComposer.Compose();

        // id = capi.World.RegisterGameTickListener(dt => UpdateText(), 1000);
    }

    // Custom draw method for the progress bar
    private void DrawProgressBar(Context ctx, double width, double height, float fraction, double offsetX = 0,
        double offsetY = 0)
    {
        // Background bar
        ctx.SetSourceRGBA(0.1, 0.1, 0.1, 0.8); // Dark background
        RoundRect(ctx, offsetX, offsetY, width, height, 3);
        ctx.Fill();

        // Filled portion
        ctx.SetSourceRGBA(0.2, 0.6, 0.2, 0.9); // Green-ish color
        RoundRect(ctx, offsetX, offsetY, width * fraction, height, 3);
        ctx.Fill();
    }

    // Helper to draw rounded rectangles (optional)
    private void RoundRect(Context ctx, double x, double y, double w, double h, double r)
    {
        ctx.NewPath();
        ctx.Arc(x + w - r, y + r, r, -90 * Math.PI / 180, 0);
        ctx.Arc(x + w - r, y + h - r, r, 0, 90 * Math.PI / 180);
        ctx.Arc(x + r, y + h - r, r, 90 * Math.PI / 180, 180 * Math.PI / 180);
        ctx.Arc(x + r, y + r, r, 180 * Math.PI / 180, 270 * Math.PI / 180);
        ctx.ClosePath();
    }

    private void UpdateText()
    {
        if (SingleComposer == null)
        {
            return;
        }

        // SingleComposer
        //     .GetDynamicText("experiencelist")
        //     .SetNewText(GetXPString());
        SingleComposer.ReCompose();
    }

    public override void OnOwnPlayerDataReceived()
    {
        base.OnOwnPlayerDataReceived();
        SetupDialog();
    }

    public override void Dispose()
    {
        base.Dispose();
        capi.World.UnregisterGameTickListener(id);
    }

    private bool ToggleGui(KeyCombination comb)
    {
        Console.WriteLine("Toggling Experience HUD");
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
}