using System;
using System.Drawing;
using System.Numerics;
using ConfigLib;
using ImGuiNET;
using MobsRadar.Configuration;
using Vintagestory.API.Client;

namespace XSkillsExperienceHUD;

public class ConfigLibCompatibility
{
    public ConfigLibCompatibility(ICoreClientAPI api)
    {
        Init(api);
    }

    private void Init(ICoreClientAPI api)
    {
        api.ModLoader.GetModSystem<ConfigLibModSystem>()
            .RegisterCustomConfig("xskillsexperiencehud", (id, buttons) => EditConfig(id, buttons, api));
    }

    private void EditConfig(string id, ControlButtons buttons, ICoreClientAPI api)
    {
        if (buttons.Save)
        {
            ConfigHelper.WriteConfig(api, XSkillsExperienceHUDModSystem.Config);
        }

        if (buttons.Defaults)
        {
            XSkillsExperienceHUDModSystem.Config = new ModConfig.ModConfig();
        }

        Edit(api, XSkillsExperienceHUDModSystem.Config, id);
    }

    private void Edit(ICoreClientAPI api, ModConfig.ModConfig config, string id)
    {
        var floatingTextFontSize = config.FloatingTextFontSize;
        ImGui.SliderInt("Floating text font size", ref floatingTextFontSize, 20, 35);
        config.FloatingTextFontSize = floatingTextFontSize;

        ImGui.NewLine();


        var isVisible = true;
        if (ImGui.CollapsingHeader("Enable / Disable floating XP for skills", ref isVisible))
        {
            foreach (IconName skill in Enum.GetValues(typeof(IconName)))
            {
                var currentValue = config.SkillConfiguration[skill];
                if (ImGui.Checkbox(skill.ToString(), ref currentValue))
                {
                    config.SkillConfiguration[skill] = currentValue;
                }
            }
        }

        ImGui.NewLine();

        var floatingTextColor = config.FloatingTextColor;
        var color = ColorTranslator.FromHtml(floatingTextColor);
        var colorVector = new Vector3(
            color.R / 255.0f,
            color.G / 255.0f,
            color.B / 255.0f
        );
        ImGui.ColorPicker3("Floating text color", ref colorVector);

        var r = (int)(colorVector.X * 255.0f);
        var g = (int)(colorVector.Y * 255.0f);
        var b = (int)(colorVector.Z * 255.0f);

        var htmlColor =
            ColorTranslator.ToHtml(Color.FromArgb(r, g, b));
        config.FloatingTextColor =
            htmlColor;
    }
}