namespace XSkillsExperienceHUD.ModConfig;

public class ModConfig
{
    public ModConfig()
    {
    }

    public ModConfig(ModConfig previousModConfig)
    {
        ShowSurvivalExperience = previousModConfig.ShowSurvivalExperience;
        ShowTemporalAdaptionExperience = previousModConfig.ShowTemporalAdaptionExperience;
        FloatingTextColor = previousModConfig.FloatingTextColor;
    }

    public bool ShowSurvivalExperience { get; set; }
    public bool ShowTemporalAdaptionExperience { get; set; }
    public string FloatingTextColor { get; set; } = "#FFFF00";
}