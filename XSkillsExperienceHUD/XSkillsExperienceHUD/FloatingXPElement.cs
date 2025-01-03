using System;
using System.Drawing;
using Cairo;
using Vintagestory.API.Client;

namespace XSkillsExperienceHUD;

public class FloatingXPElement : GuiElement
{
    private readonly float fadeSpeed = 0.5f;
    private readonly ImageSurface iconSurface;
    private readonly float totalDuration;
    private float accruedXp;

    public FloatingXPElement(ICoreClientAPI capi, ElementBounds bounds, string skillName, float xp, double x, double y,
        float duration)
        : base(capi, bounds)
    {
        SkillName = skillName;
        iconSurface = Icons.GetImageSurfaceForIcon(Util.SkillNameToIconName(SkillName));

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
        // Apply scale to everything
        ctx.Save();
        var scale = XSkillsExperienceHUDModSystem.Config.FloaterScale * 2f;
        ctx.Scale(scale, scale);

        // Text setup
        ctx.SelectFontFace("Sans", FontSlant.Normal, FontWeight.Bold);
        ctx.SetFontSize(25);

        // Text shadow
        ctx.SetSourceRGBA(0, 0, 0, Alpha * 0.7);
        ctx.MoveTo(X / scale + 2, Y / scale + 2);
        ctx.ShowText(Text);

        // Main text
        var color = ColorTranslator.FromHtml(XSkillsExperienceHUDModSystem.Config.FloatingTextColor);
        ctx.SetSourceRGBA(color.R / 255f, color.G / 255f, color.B / 255f, Alpha);
        ctx.MoveTo(X / scale, Y / scale);
        ctx.ShowText(Text);

        var extents = ctx.TextExtents(Text);
        var iconPosX = extents.Width + X / scale + 5;
        var iconPosY = Y / scale - 22;


        // Icon shadow
        ctx.SetSourceRGBA(0, 0, 0, Alpha * 0.7);
        ctx.MaskSurface(iconSurface, (int)(iconPosX + 2), (int)(iconPosY + 2));

        // Icon
        ctx.SetSourceSurface(iconSurface, (int)iconPosX, (int)iconPosY);
        ctx.PaintWithAlpha(Alpha);

        ctx.Restore();
    }

    public void Update(float dt)
    {
        Duration -= dt;
        Alpha -= fadeSpeed * dt;
        Y -= dt * XSkillsExperienceHUDModSystem.Config.FloatingTextMoveSpeed;

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
}

public static class Easings
{
    public static float EaseOutQuad(float t)
    {
        return 1f - (1f - t) * (1f - t);
    }
}