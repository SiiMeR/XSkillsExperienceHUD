namespace XSkillsExperienceHUD;

public static class Util
{
    public static IconName SkillNameToIconName(string skillName)
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