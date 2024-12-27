using System;
using System.Collections.Generic;
using Cairo;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace XSkillsExperienceHUD;

public enum IconName
{
    Survival,
    Mining,
    Forestry,
    TemporalAdaption,
    Pottery,
    Metalworking,
    Husbandry,
    Farming,
    Digging,
    Cooking,
    Combat,
    Unknown
}

public static class Icons
{
    public static Dictionary<IconName, string> IconNameMapping;

    public static void Initialize()
    {
        IconNameMapping = new Dictionary<IconName, string>
        {
            { IconName.Survival, "heart.png" },
            { IconName.Mining, "pickaxe.png" },
            {
                IconName.Forestry, "axe.png"
            },
            { IconName.TemporalAdaption, "gear.png" },

            { IconName.Pottery, "pottery.png" },

            { IconName.Metalworking, "anvil.png" },

            { IconName.Husbandry, "horseshoe.png" },
            { IconName.Farming, "wateringcan.png" },
            { IconName.Digging, "shovel.png" },
            { IconName.Cooking, "cooking.png" },
            { IconName.Combat, "combat.png" },
            { IconName.Unknown, "question.png" }
        };
    }

    public static ImageSurface GetImageSurfaceForIcon(IconName iconName, int width = 30, int height = 30)
    {
        var capi = XSkillsExperienceHUDModSystem.capi;
        var logger = capi.Logger;

        try
        {
            var icon = GetAssetByFileName(IconNameMapping[iconName]);
            return GuiElement.getImageSurfaceFromAsset(new BitmapExternal(icon.Data, icon.Data.Length, logger),
                width,
                height);
        }
        catch (Exception e)
        {
            logger.Error($"Failed to create surface out of icon {iconName}!");
            return null;
        }
    }

    private static IAsset GetAssetByFileName(string fileName)
    {
        return XSkillsExperienceHUDModSystem.capi.Assets.Get($"xskillsexperiencehud:textures/{fileName}");
    }
}