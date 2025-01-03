using System;
using ImGuiNET;

namespace XSkillsExperienceHUD;

public static class ImGuiExtensions
{
    public static bool EnumCombo<T>(string label, ref T selectedItem) where T : Enum
    {
        var currentItem = selectedItem.ToString();
        var values = Enum.GetNames(typeof(T));
        var changed = false;

        if (ImGui.BeginCombo(label, currentItem))
        {
            for (var i = 0; i < values.Length; i++)
            {
                var isSelected = currentItem == values[i];
                if (ImGui.Selectable(values[i], isSelected))
                {
                    selectedItem = (T)Enum.Parse(typeof(T), values[i]);
                    changed = true;
                }

                if (isSelected)
                {
                    ImGui.SetItemDefaultFocus();
                }
            }

            ImGui.EndCombo();
        }

        return changed;
    }
}