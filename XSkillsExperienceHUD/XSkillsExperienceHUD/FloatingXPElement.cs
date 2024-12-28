using System;
using System.Drawing;
using Cairo;
using Vintagestory.API.Client;

namespace XSkillsExperienceHUD;

public class FloatingXPElement : GuiElement
{
    private readonly float fadeSpeed = 0.5f;
    private readonly float totalDuration;
    private float accruedXp;

    public FloatingXPElement(ICoreClientAPI capi, ElementBounds bounds, string skillName, float xp, double x, double y,
        float duration)
        : base(capi, bounds)
    {
        SkillName = skillName;
        accruedXp = xp;
        X = x;
        Y = y;
        Duration = duration;
        totalDuration = duration;
        Alpha = 1f;
    }

    public string SkillName { get; }
    public float Alpha { get; private set; }
    public float Duration { get; private set; }

    public string Text
    {
        get
        {
            string formattedXP;

            if (Math.Abs(accruedXp) < 0.01 && accruedXp != 0.0)
            {
                formattedXP = accruedXp.ToString("0.###");
            }
            else
            {
                formattedXP = accruedXp.ToString("0.##");
            }

            return $"+{formattedXP}";
        }
    }

    public double X { get; }
    public double Y { get; set; }
    public bool IsDead => Alpha <= 0;

    public override void ComposeElements(Context ctx, ImageSurface surface)
    {
        base.ComposeElements(ctx, surface);

        ctx.SelectFontFace("Sans", FontSlant.Normal, FontWeight.Bold);
        ctx.SetFontSize(25);

        // main txt shadow
        ctx.SetSourceRGBA(0, 0, 0, Alpha * 0.7);
        ctx.MoveTo(X + 2, Y + 2);
        ctx.ShowText(Text);

        // main txt
        var color = ColorTranslator.FromHtml(XSkillsExperienceHUDModSystem.Config.FloatingTextColor);
        ctx.SetSourceRGBA(color.R / 255f, color.G / 255f, color.B / 255f, Alpha);
        ctx.MoveTo(X, Y);
        ctx.ShowText(Text);

        var extents = ctx.TextExtents(Text);
        ctx.MoveTo(X, Y);
        ctx.ShowText(Text);

        var iconSurface = Icons.GetImageSurfaceForIcon(SkillNameToIconName(SkillName));


        var iconPosX = extents.Width + X + 5;
        var iconPosY = Y - 22;


        // icon shadow
        ctx.Save();
        ctx.SetSourceRGBA(0, 0, 0, Alpha * 0.7);
        ctx.MaskSurface(iconSurface, (int)(iconPosX + 2), (int)(iconPosY + 2));
        ctx.Restore();

        // icon
        ctx.Save();
        ctx.SetSourceSurface(iconSurface, (int)iconPosX, (int)iconPosY);
        ctx.PaintWithAlpha(Alpha);
        ctx.Restore();
    }

    public void Update(float dt)
    {
        Duration -= dt;
        Alpha -= fadeSpeed * dt;
        Y -= dt * 45;

        var progress = 1f - Duration / totalDuration;
        Alpha = 1f - Easings.EaseOutQuad(progress);

        if (Alpha < 0)
        {
            Alpha = 0;
        }
    }

    public void AddXP(float xp)
    {
        accruedXp += xp;
    }

    private IconName SkillNameToIconName(string skillName)
    {
        return skillName switch
        {
            "combat" => IconName.Combat,
            "cooking" => IconName.Cooking,
            "digging" => IconName.Digging,
            "farming" => IconName.Farming,
            "forestry" => IconName.Forestry,
            "husbandry" => IconName.Husbandry,
            "metalworking" => IconName.Metalworking,
            "mining" => IconName.Mining,
            "pottery" => IconName.Pottery,
            "survival" => IconName.Survival,
            "temporaladaptation" => IconName.TemporalAdaption,
            _ => IconName.Unknown
        };
    }
}

public static class Easings
{
    public static float EaseOutQuad(float t)
    {
        return 1f - (1f - t) * (1f - t);
    }
}