using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace ElezenTools.UI;

public class ElezenImgui
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
}