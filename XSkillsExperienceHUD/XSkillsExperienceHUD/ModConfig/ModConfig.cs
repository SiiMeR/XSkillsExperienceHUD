using System;
using System.Collections.Generic;
using System.Linq;

namespace XSkillsExperienceHUD.ModConfig;

public class ModConfig
{
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
        FloatingTextFontSize = previousModConfig.FloatingTextFontSize;
    }

    public string FloatingTextColor { get; set; } = "#FFFF00";

    public int FloatingTextFontSize { get; set; } = 25;
}