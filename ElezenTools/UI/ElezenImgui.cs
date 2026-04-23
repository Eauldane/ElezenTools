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
    private const string TooltipSeparator = "--SEP--";

    /// <summary>
    /// Print out wrapped text.
    /// </summary>
    /// <param name="text">Text to print.</param>
    /// <param name="wrapPosition">Position to wrap text at. This is in pixels. "0" means "end of current window space".
    /// Positive integer values wrap at that "local" coordinate within the window. For example, if your cursor is at 40px,
    /// setting this to 200 gives an effective 160px of width.</param>
    public static void WrappedText(string text, float wrapPosition = 0)
    {
        ImGui.PushTextWrapPos(wrapPosition);
        ImGui.Text(text);
        ImGui.PopTextWrapPos();
    }
    
    /// <summary>
    /// Print out coloured text.
    /// </summary>
    /// <param name="text">Text to print.</param>
    /// <param name="colour">Colour to print the text in, as a Vector4. Use ElezenTools.Colour for conversions if needed.</param>
    public static void ColouredText(string text, Vector4 colour)
    {
        using var imraiiColour = ImRaii.PushColor(ImGuiCol.Text, colour);
        ImGui.Text(text);
    }

    /// <summary>
    /// Print out coloured and wrapped text.
    /// </summary>
    /// <param name="text">Text to print.</param>
    /// <param name="colour">Colour to print the text in, as a Vector4. Use ElezenTools.Colour for conversions if needed.</param>
    /// <param name="wrapPosition">Position to wrap text at. This is in pixels. "0" means "end of current window space".
    /// Positive integer values wrap at that "local" coordinate within the window. For example, if your cursor is at 40px,
    /// setting this to 200 gives an effective 160px of width.</param>
    public static void ColouredWrappedText(string text, Vector4 colour, float wrapPosition = 0)
    {
        using var imraiiColour = ImRaii.PushColor(ImGuiCol.Text, colour);
        ImGui.PushTextWrapPos(wrapPosition);
        ImGui.Text(text);
        ImGui.PopTextWrapPos();
    }

    /// <summary>
    /// Draw a labelled progress bar using the available width of the current region.
    /// </summary>
    /// <param name="label">Text shown above the progress bar.</param>
    /// <param name="progress">Progress value. Values outside the 0-1 range are clamped before drawing.</param>
    /// <param name="barText">Optional text drawn inside the progress bar. If null, the clamped progress is shown as a percentage.</param>
    /// <param name="statusText">Optional text shown below the progress bar.</param>
    /// <param name="statusColour">Optional colour for <paramref name="statusText"/>. If null, the status text is drawn using the disabled text colour.</param>
    /// <param name="height">Height of the progress bar in pixels.</param>
    /// <param name="helpText">Optional tooltip text shown next to the label using the standard help icon.</param>
    public static void DrawProgressBarOption(string label, float progress, string? barText = null,
        string? statusText = null, Vector4? statusColour = null, float height = 18f, string? helpText = null)
    {
        DrawProgressBarOption(label, progress, new Vector2(-1f, height), barText, statusText, statusColour, helpText);
    }

    /// <summary>
    /// Draw a labelled progress bar with an explicit size.
    /// </summary>
    /// <param name="label">Text shown above the progress bar.</param>
    /// <param name="progress">Progress value. Values outside the 0-1 range are clamped before drawing.</param>
    /// <param name="size">Size passed to ImGui for the progress bar. This controls both width and height.</param>
    /// <param name="barText">Optional text drawn inside the progress bar. If null, the clamped progress is shown as a percentage.</param>
    /// <param name="statusText">Optional text shown below the progress bar.</param>
    /// <param name="statusColour">Optional colour for <paramref name="statusText"/>. If null, the status text is drawn using the disabled text colour.</param>
    /// <param name="helpText">Optional tooltip text shown next to the label using the standard help icon.</param>
    public static void DrawProgressBarOption(string label, float progress, Vector2 size, string? barText = null,
        string? statusText = null, Vector4? statusColour = null, string? helpText = null)
    {
        var clampedProgress = Math.Clamp(progress, 0f, 1f);

        ImGui.TextUnformatted(label);
        if (!string.IsNullOrWhiteSpace(helpText))
        {
            DrawHelpText(helpText);
        }

        ImGui.ProgressBar(clampedProgress, size, barText ?? $"{clampedProgress:P1}");
        DrawProgressBarStatus(statusText, statusColour);
    }

    /// <summary>
    /// Draw a labelled progress bar from provided current and total values using the available width of the current region.
    /// </summary>
    /// <param name="label">Text shown above the progress bar.</param>
    /// <param name="current">Current completed amount. Negative values are treated as 0.</param>
    /// <param name="total">Total amount. Negative values are treated as 0. If this is 0, the bar is rendered at 0%.</param>
    /// <param name="barText">Optional text drawn inside the progress bar. If null, the bar shows the sanitised current and total values.</param>
    /// <param name="statusText">Optional text shown below the progress bar.</param>
    /// <param name="statusColour">Optional colour for <paramref name="statusText"/>. If null, the status text is drawn using the disabled text colour.</param>
    /// <param name="height">Height of the progress bar in pixels.</param>
    /// <param name="helpText">Optional tooltip text shown next to the label using the standard help icon.</param>
    public static void DrawProgressBarOption(string label, int current, int total, string? barText = null,
        string? statusText = null, Vector4? statusColour = null, float height = 18f, string? helpText = null)
    {
        var safeCurrent = Math.Max(current, 0);
        var safeTotal = Math.Max(total, 0);
        var progress = safeTotal == 0 ? 0f : (float)safeCurrent / safeTotal;
        DrawProgressBarOption(label, progress, new Vector2(-1f, height),
            barText ?? $"{safeCurrent:N0} / {safeTotal:N0}", statusText, statusColour, helpText);
    }

    /// <summary>
    /// Checks whether a velue is true or false, and provides either green or red Vector4 colour depending on the value.
    /// </summary>
    /// <param name="inputValue">A boolean input value.</param>
    public static Vector4 GetBooleanColour(bool inputValue)
    {
        return inputValue ? ElezenColours.BooleanGreen : ElezenColours.BooleanRed;
    }

    /// <summary>
    /// Checks whether a velue is true or false, and provides either a tick or cross icon depending on the value.
    /// </summary>
    /// <param name="inputValue">A boolean input value.</param>
    /// <param name="inline">Whether the icon should appear on the same line as the previous content before it was called. Defaults true.</param>
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
    
    public static void FontText(string text, IFontHandle font, Vector4? colour = null)
    {
        FontText(text, font, colour == null ? ImGui.GetColorU32(ImGuiCol.Text) : ImGui.GetColorU32(colour.Value));
    }

    public static void FontText(string text, IFontHandle font, uint colour)
    {
        using var pushedFont = font.Push();
        using var pushedColor = ImRaii.PushColor(ImGuiCol.Text, colour);
        ImGui.Text(text);
    }
    
    public static void ShowIcon(FontAwesomeIcon icon, uint colour)
    {
        FontText(icon.ToIconString(), Service.PluginInterface.UiBuilder.IconFontFixedWidthHandle, colour);
        
    }

    public static void ShowIcon(FontAwesomeIcon icon, Vector4? colour = null)
    {
        ShowIcon(icon, colour == null ? ImGui.GetColorU32(ImGuiCol.Text) : ImGui.GetColorU32(colour.Value));
    }
    
    private static bool ShowIconButtonInternal(FontAwesomeIcon icon, string text, Vector4? defaultColour = null, float? width = null)
    {
        var iconFont = Service.PluginInterface.UiBuilder.IconFontFixedWidthHandle;
        int num = 0;
        if (defaultColour.HasValue)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, defaultColour.Value);
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
    
    public static void DrawHelpText(string helpText)
    {
        ImGui.SameLine();
        ShowIcon(FontAwesomeIcon.QuestionCircle, ImGui.GetColorU32(ImGuiCol.TextDisabled));
        AttachTooltip(helpText);
    }

    public static void AttachTooltip(string text)
    {
        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
        {
            ImGui.BeginTooltip();
            ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35f);
            if (text.Contains(TooltipSeparator, StringComparison.Ordinal))
            {
                var splitText = text.Split(TooltipSeparator, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < splitText.Length; i++)
                {
                    ImGui.Text(splitText[i]);
                    if (i != splitText.Length - 1) ImGui.Separator();
                }
            }
            else
            {
                ImGui.Text(text);
            }

            ImGui.PopTextWrapPos();
            ImGui.EndTooltip();
        }
    }

    private static void DrawProgressBarStatus(string? statusText, Vector4? statusColour)
    {
        if (string.IsNullOrWhiteSpace(statusText))
        {
            return;
        }

        if (statusColour.HasValue)
        {
            ImGui.TextColored(statusColour.Value, statusText);
            return;
        }

        ImGui.TextDisabled(statusText);
    }
}
