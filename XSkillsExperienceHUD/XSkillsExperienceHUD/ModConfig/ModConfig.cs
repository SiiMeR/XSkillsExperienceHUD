using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;

namespace XSkillsExperienceHUD.ModConfig;

public class ModConfig
{
    public EnumDialogArea FloatingXPLocation = EnumDialogArea.RightTop;
    public int FloatingXPX = 0;
    public int FloatingXPY = 0;
    public Dictionary<IconName, bool> SkillConfiguration = new();

    public ModConfig()
    {
        SkillConfiguration = Enum.GetValues<IconName>().ToList().ToDictionary(icon => icon, _ => true);
        SkillConfiguration[IconName.Survival] = false;
    }

    public ModConfig(ModConfig previousModConfig)
    {
        FloatingTextColor = previousModConfig.FloatingTextColor;
        SkillConfiguration = previousModConfig.SkillConfiguration;
    }

    public string FloatingTextColor { get; set; } = "#FFFF00";

    public int FloatingTextMoveSpeed { get; set; } = 40;

    public float FloaterScale { get; set; } = 0.5f;

    public bool ShowExperienceTable { get; set; } = true;
}