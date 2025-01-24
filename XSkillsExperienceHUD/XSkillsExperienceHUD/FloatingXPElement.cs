using System;
using System.Drawing;
using Cairo;
using Vintagestory.API.Client;

namespace XSkillsExperienceHUD;

public class FloatingXPElement : GuiElement
{
    private readonly float fadeSpeed = 0.4f;
    private readonly ImageSurface iconSurface;
    private readonly float totalDuration;
    private float accruedXp;
    private Context cachedCtx;
    private ImageSurface cachedSurface;
    private bool isDirty;

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

        isDirty = true;
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

        if (isDirty)
        {
            RebuildCachedSurface();
            isDirty = false;
        }

        ctx.Save();

        var scale = XSkillsExperienceHUDModSystem.Config.FloaterScale * 2f;
        ctx.Scale(scale, scale);

        ctx.SetSourceSurface(cachedSurface, (int)(X / scale), (int)(Y / scale));

        ctx.PaintWithAlpha(Alpha);

        ctx.Restore();
    }

    private void RebuildCachedSurface()
    {
        cachedSurface?.Dispose();
        cachedSurface = new ImageSurface(Format.Argb32, 200, 40);
        cachedCtx?.Dispose();
        cachedCtx = new Context(cachedSurface);

        cachedCtx.Save();

        cachedCtx.SelectFontFace("Sans", FontSlant.Normal, FontWeight.Bold);
        cachedCtx.SetFontSize(25);

        cachedCtx.SetSourceRGBA(0, 0, 0, 1.0);
        cachedCtx.MoveTo(2, 25); // baseline around y=25
        cachedCtx.ShowText(Text);

        // Main text
        var color = ColorTranslator.FromHtml(XSkillsExperienceHUDModSystem.Config.FloatingTextColor);
        cachedCtx.SetSourceRGBA(color.R / 255f, color.G / 255f, color.B / 255f, 1.0);
        cachedCtx.MoveTo(0, 23); // baseline around y=23
        cachedCtx.ShowText(Text);

        var te = cachedCtx.TextExtents(Text);
        var textWidth = (float)te.Width;

        // Icon shadow
        var iconPosX = textWidth + 5;
        float iconPosY = 0;
        cachedCtx.SetSourceRGBA(0, 0, 0, 1.0);
        cachedCtx.MaskSurface(iconSurface, (int)(iconPosX + 2), (int)(iconPosY + 2));

        // Icon
        cachedCtx.SetSourceSurface(iconSurface, (int)iconPosX, (int)iconPosY);
        cachedCtx.Paint();

        cachedCtx.Restore();
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
        isDirty = true;
    }
}

public static class Easings
{
    public static float EaseOutQuad(float t)
    {
        return 1f - (1f - t) * (1f - t);
    }
}