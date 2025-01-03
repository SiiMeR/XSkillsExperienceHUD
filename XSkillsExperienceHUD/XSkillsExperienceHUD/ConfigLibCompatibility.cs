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
        var changed = false;

        var displayStaticXP = config.ShowExperienceTable;
        if (ImGui.Checkbox("Display experience table", ref displayStaticXP))
        {
            config.ShowExperienceTable = displayStaticXP;
            ExperienceDisplay.Instance?.Toggle();
        }


        var floatingTextMoveSpeed = config.FloatingTextMoveSpeed;
        if (ImGui.SliderInt("Floating text move speed", ref floatingTextMoveSpeed, 20, 90))
        {
            config.FloatingTextMoveSpeed = floatingTextMoveSpeed;
            changed = true;
        }


        var floaterScale = config.FloaterScale;
        if (ImGui.SliderFloat("Floater scale", ref floaterScale, 0.5f, 1.0f))
        {
            config.FloaterScale = floaterScale;
            changed = true;
        }

        var location = config.FloatingXPLocation;
        if (ImGuiExtensions.EnumCombo("XP General Placement", ref location))
        {
            config.FloatingXPLocation = location;
            changed = true;
        }

        var fineTuneX = config.FloatingXPX;
        if (ImGui.SliderInt("Fine tune XP location X", ref fineTuneX, 0, 300))
        {
            config.FloatingXPX = fineTuneX;
            changed = true;
        }


        var fineTuneY = config.FloatingXPY;
        if (ImGui.SliderInt("Fine tune XP location Y", ref fineTuneY, 0, 300))
        {
            config.FloatingXPY = fineTuneY;
            changed = true;
        }


        var isVisible = true;
        if (ImGui.CollapsingHeader("Enable floating XP per skill", ref isVisible))
        {
            foreach (IconName skill in Enum.GetValues(typeof(IconName)))
            {
                var currentValue = config.SkillConfiguration[skill];
                if (ImGui.Checkbox(skill.ToString(), ref currentValue))
                {
                    config.SkillConfiguration[skill] = currentValue;
                    changed = true;
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

        if (ImGui.ColorPicker3("Floating text color", ref colorVector))
        {
            var r = (int)(colorVector.X * 255.0f);
            var g = (int)(colorVector.Y * 255.0f);
            var b = (int)(colorVector.Z * 255.0f);

            var htmlColor =
                ColorTranslator.ToHtml(Color.FromArgb(r, g, b));
            config.FloatingTextColor =
                htmlColor;
            changed = true;
        }

        if (changed)
        {
            FloatingXpDisplay.Instance?.SetupDialog();
        }
    }
}