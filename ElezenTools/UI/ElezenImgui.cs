// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Eauldane
//
// This file is part of ElezenTools.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.


using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin;
using ElezenTools.Services;

namespace ElezenTools.UI;

public static class ElezenImgui
{
    public static void WrappedText(string text, float wrapPosition = 0)
    {
        ImGui.PushTextWrapPos(wrapPosition);
        ImGui.Text(text);
        ImGui.PopTextWrapPos();
    }
    
    public static void ColouredText(string text, Vector4 colour)
    {
        using var imraiiColour = ImRaii.PushColor(ImGuiCol.Text, colour);
        ImGui.Text(text);
    }

    public static void ColouredWrappedText(string text, Vector4 colour, float wrapPosition = 0)
    {
        using var imraiiColour = ImRaii.PushColor(ImGuiCol.Text, colour);
        ImGui.PushTextWrapPos(wrapPosition);
        ImGui.Text(text);
        ImGui.PopTextWrapPos();
    }

    public static Vector4 GetBooleanColour(bool inputValue)
    {
        return inputValue ? ElezenColours.BooleanGreen : ElezenColours.BooleanRed;
    }

    public static void GetBooleanIcon(bool inputValue, bool inline = true)
    {
        if (inline)
        {
            ImGui.SameLine();
        }

        if (inputValue)
        {
            using var colour = ImRaii.PushColor(ImGuiCol.Text, ElezenColours.BooleanGreen);
            ShowIcon(FontAwesomeIcon.Check);
        }
        else
        {
            using var colour = ImRaii.PushColor(ImGuiCol.Text, ElezenColours.BooleanRed);
            ShowIcon(FontAwesomeIcon.Times);
        }
    }
    
    public static void FontText(string text, IFontHandle font, Vector4? color = null)
    {
        FontText(text, font, color == null ? ImGui.GetColorU32(ImGuiCol.Text) : ImGui.GetColorU32(color.Value));
    }

    public static void FontText(string text, IFontHandle font, uint color)
    {
        using var pushedFont = font.Push();
        using var pushedColor = ImRaii.PushColor(ImGuiCol.Text, color);
        ImGui.TextUnformatted(text);
    }
    
    public static void ShowIcon(FontAwesomeIcon icon, uint color)
    {
        FontText(icon.ToIconString(), Service.PluginInterface.UiBuilder.IconFontFixedWidthHandle, color);
        
    }

    public static void ShowIcon(FontAwesomeIcon icon, Vector4? color = null)
    {
        ShowIcon(icon, color == null ? ImGui.GetColorU32(ImGuiCol.Text) : ImGui.GetColorU32(color.Value));
    }
    
    private static bool ShowIconButtonInternal(FontAwesomeIcon icon, string text, Vector4? defaultColor = null, float? width = null)
    {
        var iconFont = Service.PluginInterface.UiBuilder.IconFontFixedWidthHandle;
        int num = 0;
        if (defaultColor.HasValue)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, defaultColor.Value);
            num++;
        }

        ImGui.PushID(text);
        Vector2 vector;
        using (iconFont.Push())
            vector = ImGui.CalcTextSize(icon.ToIconString());
        Vector2 vector2 = ImGui.CalcTextSize(text);
        ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
        Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
        float num2 = 3f * ImGuiHelpers.GlobalScale;
        float x = width ?? vector.X + vector2.X + ImGui.GetStyle().FramePadding.X * 2f + num2;
        float frameHeight = ImGui.GetFrameHeight();
        bool result = ImGui.Button(string.Empty, new Vector2(x, frameHeight));
        Vector2 pos = new Vector2(cursorScreenPos.X + ImGui.GetStyle().FramePadding.X, cursorScreenPos.Y + ImGui.GetStyle().FramePadding.Y);
        using (iconFont.Push())
            windowDrawList.AddText(pos, ImGui.GetColorU32(ImGuiCol.Text), icon.ToIconString());
        Vector2 pos2 = new Vector2(pos.X + vector.X + num2, cursorScreenPos.Y + ImGui.GetStyle().FramePadding.Y);
        windowDrawList.AddText(pos2, ImGui.GetColorU32(ImGuiCol.Text), text);
        ImGui.PopID();
        if (num > 0)
        {
            ImGui.PopStyleColor(num);
        }

        return result;
    }

    public static bool ShowIconButton(FontAwesomeIcon icon, string text, float? width = null, bool isInPopup = false)
    {
        return ShowIconButtonInternal(icon, text,
            isInPopup ? ColorHelpers.RgbaUintToVector4(ImGui.GetColorU32(ImGuiCol.PopupBg)) : null,
            width <= 0 ? null : width);
    }
    
    public static Vector2 GetIconButtonSize(FontAwesomeIcon icon)
    {
        var iconFont = Service.PluginInterface.UiBuilder.IconFontFixedWidthHandle;
        using var font = iconFont.Push();
        return ImGuiHelpers.GetButtonSize(icon.ToIconString());
    }

    public static Vector2 GetIconData(FontAwesomeIcon icon)
    {
        var iconFont = Service.PluginInterface.UiBuilder.IconFontFixedWidthHandle;

        using var font = iconFont.Push();
        return ImGui.CalcTextSize(icon.ToIconString());
    }
    
    public static float GetIconButtonTextSize(FontAwesomeIcon icon, string text)
    {
        var iconFont = Service.PluginInterface.UiBuilder.IconFontFixedWidthHandle;

        Vector2 vector;
        using (iconFont.Push())
            vector = ImGui.CalcTextSize(icon.ToIconString());

        Vector2 vector2 = ImGui.CalcTextSize(text);
        float num = 3f * ImGuiHelpers.GlobalScale;
        return vector.X + vector2.X + ImGui.GetStyle().FramePadding.X * 2f + num;
    }

    public static Vector2 GetIconSize(FontAwesomeIcon icon)
    {
        using var font = ImRaii.PushFont(UiBuilder.IconFont);
        var iconSize = ImGui.CalcTextSize(icon.ToIconString());
        return iconSize;
    }
}