using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cairo;
using Vintagestory.API.Client;
using XLib.XLeveling;

namespace XSkillsExperienceHUD;

public class ExperienceDisplay : HudElement
{
    private readonly Dictionary<string, float> skillFractions = new();
    private readonly Dictionary<string, float> skillRecentGains = new();
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

    public void SetXP(string skillName, PlayerSkill playerSkill, float xp)
    {
        var xpString =
            $"Level {playerSkill.Level} - {playerSkill.Experience + xp:F2}/{playerSkill.RequiredExperience:F2}";
        if (xpDisplay.ContainsKey(skillName))
        {
            xpDisplay[skillName] = xpString;
        }
        else
        {
            xpDisplay.TryAdd(skillName, xpString);
        }

        UpdateText();
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


        // Just a simple 300x300 pixel box
        // var textBounds = ElementBounds.Fixed(0, 40, 300, 350);

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

        // Lastly, create the dialog
        SingleComposer = capi.Gui.CreateCompo("experiencedialog", dialogBounds)
            .AddShadedDialogBG(bgBounds);

        var y = 40;
        var barBoundList = new List<ElementBounds>();
        for (var index = 0; index < playerSkillset.PlayerSkills.Count; index++)
        {
            var playerSkill = playerSkillset.PlayerSkills[index];
            // Increase the height to 40 so we have space above the bar for text
            var barBounds = ElementBounds.Fixed(20, y + 25 * index, 200, 30).WithFixedPadding(0, 20);
            barBoundList.Add(barBounds);

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
                var currentLevelX = barX + 5;
                var currentLevelY = centerY - extCurrentLevel.Height / 2 - extCurrentLevel.YBearing;
                ctx.MoveTo(currentLevelX, currentLevelY);
                ctx.ShowText(currentLevelText);

                // Draw next level on the right inside the bar
                var nextLevelX = barX + barWidth - extNextLevel.Width - 5;
                var nextLevelY = centerY - extNextLevel.Height / 2 - extNextLevel.YBearing;
                ctx.MoveTo(nextLevelX, nextLevelY);
                ctx.ShowText(nextLevelText);
            }, "Progressbar" + index);
        }

        bgBounds.WithChildren(barBoundList.ToArray());

        SingleComposer.Bounds.Alignment = EnumDialogArea.RightBottom;
        SingleComposer.Compose();

        id = capi.World.RegisterGameTickListener(dt => UpdateText(), 1000);
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

    // Dummy functions for illustration
    private float GetCurrentSkillXP(string skillName)
    {
        // Return the current XP for the skillName
        return 350f; // Example
    }

    private float GetNextSkillLevelXP(string skillName)
    {
        // Return the XP needed for the next level for skillName
        return 500f; // Example
    }


    // Called every second or on XP events
    private void UpdateUI()
    {
        // Example logic: Suppose you update skill XP and fraction here.
        // Update fractions and text, then redraw the bars.
        foreach (var name in skillFractions.Keys.ToList())
        {
            // Let's say we have a function to get the skill's current and next XP
            var currentXP = GetCurrentSkillXP(name);
            var nextLevelXP = GetNextSkillLevelXP(name);
            var fraction = currentXP / nextLevelXP;
            skillFractions[name] = fraction;

            // Update the label
            SingleComposer.GetDynamicText(name + "Label").SetNewText($"{name}: {currentXP}/{nextLevelXP}");

            // Update XP gain text if there is recent gain
            var gained = skillRecentGains[name];
            if (gained > 0)
            {
                SingleComposer.GetDynamicText(name + "XPChange").SetNewText($"(+{gained})");
                // After some time, you might reduce this or reset to 0
                // skillRecentGains[name] = 0 after a delay
            }
            else
            {
                SingleComposer.GetDynamicText(name + "XPChange").SetNewText("");
            }

            // Redraw the bar
            SingleComposer.GetCustomDraw(name + "Bar").Redraw();
        }
    }

    private string GetXPString()
    {
        var stringBuilder = new StringBuilder();

        foreach (var (key, value) in xpDisplay)
        {
            stringBuilder.AppendLine($"{key} | {value}");
        }

        return stringBuilder.ToString();
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