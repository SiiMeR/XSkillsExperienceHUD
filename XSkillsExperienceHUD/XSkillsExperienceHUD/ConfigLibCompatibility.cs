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
        var showSurvivalExperience = config.ShowSurvivalExperience;
        ImGui.Checkbox("Show survival experience drops", ref showSurvivalExperience);
        config.ShowSurvivalExperience = showSurvivalExperience;

        var showTemporalAdaptionExperience = config.ShowTemporalAdaptionExperience;
        ImGui.Checkbox("Show temporal adaption experience drops", ref showTemporalAdaptionExperience);
        config.ShowTemporalAdaptionExperience = showTemporalAdaptionExperience;

        var floatingTextColor = config.FloatingTextColor;
        var color = ColorTranslator.FromHtml(floatingTextColor);
        var colorVector = new Vector3(color.R, color.G, color.B);
        ImGui.ColorPicker3("Floating text color", ref colorVector, ImGuiColorEditFlags.Float);
        var htmlColor =
            ColorTranslator.ToHtml(Color.FromArgb((int)Math.Clamp(colorVector.X * 255f, 0, 255),
                (int)Math.Clamp(colorVector.Y * 255f, 0, 255), (int)Math.Clamp(colorVector.Z * 255f, 0, 255)));
        config.FloatingTextColor =
            htmlColor;
        //
        //
        // int horizontalRadius = config.HorizontalRadius;
        // ImGui.InputInt(Lang.Get(settingHorizontalRadius) + $"##horizontalRadius-{id}", ref horizontalRadius, 1,
        //     10);
        // config.HorizontalRadius = horizontalRadius <= 0 ? 1 : horizontalRadius;
        //
        // int verticalRadius = config.VerticalRadius;
        // ImGui.InputInt(Lang.Get(settingVerticalRadius) + $"##verticalRadius-{id}", ref verticalRadius, 1,
        //     10);
        // config.VerticalRadius = verticalRadius <= 0 ? 1 : verticalRadius;
        //
        // var canRemove = config.Markers.Count > 1;
        // if (!canRemove)
        // {
        //     ImGui.BeginDisabled();
        // }
        //
        // ImGui.SameLine();
        //
        // var canAddMarker = markerToAdd != "" && !config.Markers.ContainsKey(markerToAdd);
        // if (!canAddMarker)
        // {
        //     ImGui.BeginDisabled();
        // }
        //
        // if (ImGui.Button(Lang.Get(settingAdd) + $"##add-{id}"))
        // {
        //     config.Markers.Add(markerToAdd, new());
        //     selectedMarker = config.Markers.Keys.ToArray().IndexOf(markerToAdd);
        // }
        //
        // if (!canAddMarker)
        // {
        //     ImGui.EndDisabled();
        // }
        //
        // ImGui.SameLine();
        //
        // ImGui.InputTextWithHint($"##{id}", Lang.Get(textRegexSupport), ref markerToAdd, 512);
        //
        // string[] keys = config.Markers.Keys.ToArray();
        // ImGui.ListBox(Lang.Get(settingMarkers) + $"##markers-{id}", ref selectedMarker, keys, keys.Length);
        //
        // ImGui.SeparatorText(Lang.Get(settingMarkerProperties));
    }
}