using Vintagestory.API.Common;
using XSkillsExperienceHUD.ModConfig;

namespace MobsRadar.Configuration;

public static class ConfigHelper
{
    private const string ConfigName = "xskillsexperiencehud.config.json";

    public static ModConfig ReadConfig(ICoreAPI api)
    {
        ModConfig modConfig;

        try
        {
            modConfig = LoadConfig(api);

            if (modConfig == null)
            {
                GenerateConfig(api);
                modConfig = LoadConfig(api);
            }
            else
            {
                GenerateConfig(api, modConfig);
            }
        }
        catch
        {
            GenerateConfig(api);
            modConfig = LoadConfig(api);
        }

        return modConfig;
    }

    public static void WriteConfig(ICoreAPI api, ModConfig modConfig)
    {
        GenerateConfig(api, modConfig);
    }

    private static ModConfig LoadConfig(ICoreAPI api)
    {
        return api.LoadModConfig<ModConfig>(ConfigName);
    }

    private static void GenerateConfig(ICoreAPI api)
    {
        api.StoreModConfig(new ModConfig(), ConfigName);
    }

    private static void GenerateConfig(ICoreAPI api, ModConfig previousModConfig)
    {
        api.StoreModConfig(new ModConfig(previousModConfig), ConfigName);
    }
}