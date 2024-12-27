using Cairo;
using Vintagestory.API.Client;

namespace XSkillsExperienceHUD;

public class FloatingXPElement : GuiElement
{
    private readonly float fadeSpeed = 0.5f; // adjust as needed
    private float AccruedXP;

    private ImageSurface iconSurface;


    public FloatingXPElement(ICoreClientAPI capi, ElementBounds bounds, string skillName, float xp, double x, double y,
        float duration)
        : base(capi, bounds)
    {
        SkillName = skillName;
        AccruedXP = xp;
        X = x;
        Y = y;
        Duration = duration;
        Alpha = 1f;
    }

    public string SkillName { get; }
    public float Alpha { get; private set; }
    public float Duration { get; private set; }
    public string Text => $"+{AccruedXP:0.##}";
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
        ctx.SetSourceRGBA(1, 1, 0, Alpha); // Yellow text
        ctx.MoveTo(X, Y);
        ctx.ShowText(Text);

        var extents = ctx.TextExtents(Text);
        ctx.MoveTo(X, Y);
        ctx.ShowText(Text);

        iconSurface = Icons.GetImageSurfaceForIcon(SkillNameToIconName(SkillName));


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

        if (Alpha < 0)
        {
            Alpha = 0;
        }
    }

    public void AddXP(float xp)
    {
        AccruedXP += xp;
    }

    public override void Dispose()
    {
        iconSurface?.Dispose();
        base.Dispose();
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