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
    private const int IconWidth = 30;
    private const int IconHeight = 30;
    private static Dictionary<IconName, string> _iconNameMapping;
    private static readonly Dictionary<IconName, ImageSurface> IconCache = new();

    private static ICoreClientAPI capi => XSkillsExperienceHUDModSystem.capi;


    public static void Initialize()
    {
        _iconNameMapping = new Dictionary<IconName, string>
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

    public static ImageSurface GetImageSurfaceForIcon(IconName iconName)
    {
        var logger = capi.Logger;

        // 1) Check if we already have a surface for this icon.
        if (IconCache.TryGetValue(iconName, out var surface))
        {
            // Already cached
            return surface;
        }

        // 2) If not cached, load it now:
        try
        {
            var fileName = _iconNameMapping[iconName];
            var iconAsset = GetAssetByFileName(fileName);

            surface = GuiElement.getImageSurfaceFromAsset(
                new BitmapExternal(iconAsset.Data, iconAsset.Data.Length, logger),
                IconWidth,
                IconHeight
            );

            IconCache[iconName] = surface;
            return surface;
        }
        catch (Exception e)
        {
            logger.Error($"Failed to create surface for icon {iconName}!\n{e}");
            return null;
        }
    }

    public static void DisposeAll()
    {
        foreach (var kvp in IconCache)
        {
            kvp.Value?.Dispose();
        }

        IconCache.Clear();
    }

    private static IAsset GetAssetByFileName(string fileName)
    {
        return capi.Assets.Get($"xskillsexperiencehud:textures/{fileName}");
    }
}