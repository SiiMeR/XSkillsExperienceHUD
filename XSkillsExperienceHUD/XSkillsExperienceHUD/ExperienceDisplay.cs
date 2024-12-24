using System;
using Cairo;
using Vintagestory.API.Client;
using XLib.XLeveling;

namespace XSkillsExperienceHUD;

public class ExperienceDisplay : HudElement
{
    public FloatingXpDisplay FloatingXpDisplay;

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

        FloatingXpDisplay = new FloatingXpDisplay(capi);
        FloatingXpDisplay.SetupDialog();
    }


    public void UpdateDisplay(PlayerSkill playerSkill, float xp)
    {
        SetupDialog();
        SingleComposer?.GetCustomDraw(playerSkill.Skill.Name)?.Redraw();
        FloatingXpDisplay?.UpdateDisplay(playerSkill, xp);
    }

    public bool ResetupDialog(KeyCombination comb)
    {
        FloatingXpDisplay?.ResetupDialog(comb);
        SetupDialog();
        return true;
    }

    private void SetupDialog()
    {
        var dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.RightMiddle);

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

        var y = 40;
        for (var index = 0; index < playerSkillset.PlayerSkills.Count; index++)
        {
            var playerSkill = playerSkillset.PlayerSkills[index];
            var barBounds = ElementBounds.Fixed(20, 25 * index, 175, 30)
                .WithFixedPadding(GuiStyle.HalfPadding);

            var hoverTextBounds = ElementBounds.Fixed(20, 25 * index + 10, 175, 20);

            var hoverText = $"{playerSkill.Experience:0.##}/{playerSkill.RequiredExperience:0.#} XP";
            SingleComposer.AddAutoSizeHoverText(hoverText, CairoFont.WhiteDetailText(), 200, hoverTextBounds);
            SingleComposer.AddDynamicCustomDraw(barBounds, (ctx, surface, currentBounds) =>
            {
                var fraction = playerSkill.Experience / playerSkill.RequiredExperience;
                var w = currentBounds.InnerWidth;
                var h = currentBounds.InnerHeight;

                double barX = 0;
                double barY = 20;
                var barWidth = w;
                var barHeight = h - 20;
                DrawProgressBar(ctx, barWidth, barHeight, fraction, barX, barY);

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


    public override void OnOwnPlayerDataReceived()
    {
        base.OnOwnPlayerDataReceived();
        SetupDialog();
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

        FloatingXpDisplay.ToggleGui(comb);

        return true;
    }
}